using BookExchange.Api.Data;
using BookExchange.Api.Books.Dtos;
using BookExchange.Api.Books.Entities;
using BookExchange.Api.Books.Extensions;
using BookExchange.Api.Books.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookExchange.Api.Books.Services;


namespace BookExchange.Api.Books.Endpoints;

public static class BooksEndpoints
{
    const string GetBookEndpointName = "GetBook";

    public static RouteGroupBuilder MapBooksEndpoints(this WebApplication app) 
    {
        var group = app.MapGroup("books").WithParameterValidation();

        // GET /books/search?q=hobbit  -> local catalog first (title/author/ISBN/genre), Google Books as fallback (see above)
        group.MapGet("/search", async (string q, BookExchangeContext dbContext, IGoogleBooksService googleBooks) =>
        {
            var localMatches = await dbContext.BookCatalogs
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Where(b =>
                    EF.Functions.Like(b.Title, $"%{q}%") ||
                    b.Authors.Any(a => EF.Functions.Like(a.Name, $"%{q}%")) ||
                    b.Genres.Any(g => EF.Functions.Like(g.Name, $"%{q}%")) ||
                    (b.ISBN10 != null && b.ISBN10 == q) ||
                    (b.ISBN13 != null && b.ISBN13 == q))
                .Select(b => b.ToSearchResultDto())
                .ToListAsync();

            if (localMatches.Count > 0)
            {
                return Results.Ok(localMatches);
            }

            return Results.Ok(await googleBooks.SearchAsync(q));
        });

        // GET /books  -> list all active listings
        group.MapGet("/", async (BookExchangeContext dbContext) =>
            await dbContext.Listings
                .Include(l => l.BookCatalog).ThenInclude(b => b!.Authors)
                .Include(l => l.Condition)
                .Where(l => l.Status == ListingStatus.Available)
                .Select(l => l.ToSummaryDto())
                .ToListAsync());

        // GET /books/1 -> one listing's full details
        group.MapGet("/{id}", async (int id, BookExchangeContext dbContext) =>
        {
            var listing = await dbContext.Listings
                .Include(l => l.BookCatalog).ThenInclude(b => b!.Authors)
                .Include(l => l.BookCatalog).ThenInclude(b => b!.Genres)
                .Include(l => l.Condition)
                .Include(l => l.Owner).ThenInclude(o => o!.Profile)
                .FirstOrDefaultAsync(l => l.Id == id);

            return listing is null ? Results.NotFound() : Results.Ok(listing.ToDetailsDto());
        })
        .WithName(GetBookEndpointName);

        // POST /books  -> create a new listing, reusing an existing catalog row (CatalogId)
        // or fetching-and-caching one from Google Books for the first time (GoogleBooksId)
        group.MapPost("/", async (
            CreateListingDto dto,
            ClaimsPrincipal user,
            IGoogleBooksService googleBooks,
            BookExchangeContext dbContext) =>
        {
            if (dto.CatalogId is null == dto.GoogleBooksId is null)
            {
                return Results.BadRequest("Provide exactly one of CatalogId or GoogleBooksId.");
            }

            BookCatalog catalog;

            if (dto.CatalogId is not null)
            {
                // Reusing a Local or Manual search result — no external call needed.
                // Authors/Genres must be included here (not just FindAsync) since
                // ToDetailsDto() below needs them populated.
                catalog = await dbContext.BookCatalogs
                    .Include(b => b.Authors)
                    .Include(b => b.Genres)
                    .FirstOrDefaultAsync(b => b.Id == dto.CatalogId)
                    ?? throw new InvalidOperationException("Catalog entry not found");
            }
            else
            {
                var existing = await dbContext.BookCatalogs
                    .Include(b => b.Authors)
                    .Include(b => b.Genres)
                    .FirstOrDefaultAsync(b => b.GoogleBooksId == dto.GoogleBooksId);

                if (existing is not null)
                {
                    catalog = existing;
                }
                else
                {
                    var volume = await googleBooks.GetByIdAsync(dto.GoogleBooksId!)
                        ?? throw new InvalidOperationException("Book not found");

                    catalog = await BuildCatalogFromVolumeAsync(volume, dbContext);
                    dbContext.BookCatalogs.Add(catalog);
                }
            }

            var listing = new Listing
            {
                BookCatalog = catalog,
                OwnerId = user.GetUserId(),
                ConditionId = dto.ConditionId,
                Notes = dto.Notes,
                Status = ListingStatus.Available
            };

            dbContext.Listings.Add(listing);
            await dbContext.SaveChangesAsync();

            await LoadDetailsNavigationsAsync(listing, dbContext);
            return Results.CreatedAtRoute(GetBookEndpointName, new { id = listing.Id }, listing.ToDetailsDto());
        });

        // POST /books/manual  -> last-resort creation after a search came back empty; re-verifies
        // by ISBN against the local catalog and Google Books before trusting hand-typed fields
        group.MapPost("/manual", async (
            CreateManualBookDto dto,
            ClaimsPrincipal user,
            IGoogleBooksService googleBooks,
            BookExchangeContext dbContext) =>
        {
            BookCatalog? catalog = null;
            var isbn = dto.Isbn13 ?? dto.Isbn10;

            if (isbn is not null)
            {
                // 1. Does this ISBN already exist locally — as a Google-sourced OR a previously
                //    manual row? Either way, it's the same physical book; reuse it.
                catalog = await dbContext.BookCatalogs
                    .Include(b => b.Authors)
                    .Include(b => b.Genres)
                    .FirstOrDefaultAsync(b =>
                        (dto.Isbn13 != null && b.ISBN13 == dto.Isbn13) ||
                        (dto.Isbn10 != null && b.ISBN10 == dto.Isbn10));

                // 2. Not cached locally — ask Google Books directly by ISBN. A missed search
                //    result (typo, different edition shown, etc.) shouldn't still let a
                //    duplicate/incorrect manual entry through if Google Books actually has it.
                if (catalog is null)
                {
                    var volume = await googleBooks.GetByIsbnAsync(isbn);
                    if (volume is not null)
                    {
                        catalog = await BuildCatalogFromVolumeAsync(volume, dbContext);
                        dbContext.BookCatalogs.Add(catalog);
                    }
                }
            }

            // 3. Truly not found anywhere (or no ISBN was provided at all, e.g. a zine with none) —
            //    this is the only case where the user's typed-in fields are actually trusted.
            if (catalog is null)
            {
                catalog = new BookCatalog
                {
                    Title = dto.Title,
                    ISBN10 = dto.Isbn10,
                    ISBN13 = dto.Isbn13,
                    Publisher = dto.Publisher,
                    PublishedYear = dto.PublishedYear,
                    PageCount = dto.PageCount,
                    CoverImageUrl = dto.CoverImageUrl,
                    Description = dto.Description,
                    GoogleBooksId = null, // the defining trait of a genuinely manual catalog row
                    Authors = await ResolveAuthorsAsync(dto.Authors, dbContext),
                    Genres = await ResolveGenresAsync(dto.Genres ?? [], dbContext)
                };
                dbContext.BookCatalogs.Add(catalog);
            }

            var listing = new Listing
            {
                BookCatalog = catalog,
                OwnerId = user.GetUserId(),
                ConditionId = dto.ConditionId,
                Notes = dto.Notes,
                Status = ListingStatus.Available
            };

            dbContext.Listings.Add(listing);
            await dbContext.SaveChangesAsync();

            await LoadDetailsNavigationsAsync(listing, dbContext);
            return Results.CreatedAtRoute(GetBookEndpointName, new { id = listing.Id }, listing.ToDetailsDto());
        });

        return group;
    }

    // After SaveChangesAsync, the navigations ToDetailsDto() needs (Condition, Owner/Profile)
    // aren't loaded yet on a freshly-created Listing — load them explicitly before mapping.
    private static async Task LoadDetailsNavigationsAsync(Listing listing, BookExchangeContext dbContext)
    {
        await dbContext.Entry(listing).Reference(l => l.Condition).LoadAsync();
        await dbContext.Entry(listing).Reference(l => l.Owner).LoadAsync();
        if (listing.Owner is not null)
        {
            await dbContext.Entry(listing.Owner).Reference(o => o.Profile).LoadAsync();
        }
    }

    // Builds a new BookCatalog from a Google Books search result, resolving/creating
    // its Author and Genre rows along the way. Does not save — caller adds + SaveChanges.
    private static async Task<BookCatalog> BuildCatalogFromVolumeAsync(BookSearchResultDto volume, BookExchangeContext dbContext) =>
        new()
        {
            Title = volume.Title,
            ISBN10 = volume.Isbn10,
            ISBN13 = volume.Isbn13,
            Publisher = volume.Publisher,
            PublishedYear = volume.PublishedYear,
            PageCount = volume.PageCount,
            CoverImageUrl = volume.CoverImageUrl,
            Description = volume.Description,
            GoogleBooksId = volume.GoogleBooksId,
            Authors = await ResolveAuthorsAsync(volume.Authors, dbContext),
            Genres = await ResolveGenresAsync(volume.Categories, dbContext)
        };

    // "Find or create" so the same author name is never duplicated across catalog rows.
    private static async Task<List<Author>> ResolveAuthorsAsync(List<string> authorNames, BookExchangeContext dbContext)
    {
        var authors = new List<Author>();
        foreach (var name in authorNames)
        {
            var existing = await dbContext.Authors.FirstOrDefaultAsync(a => a.Name == name);
            if (existing is null)
            {
                existing = new Author { Name = name };
                dbContext.Authors.Add(existing);
            }
            authors.Add(existing);
        }
        return authors;
    }

    // "Find or create" so the same genre name is never duplicated across catalog rows.
    private static async Task<List<Genre>> ResolveGenresAsync(List<string> categoryNames, BookExchangeContext dbContext)
    {
        var genres = new List<Genre>();
        foreach (var name in categoryNames)
        {
            var existing = await dbContext.Genres.FirstOrDefaultAsync(g => g.Name == name);
            if (existing is null)
            {
                existing = new Genre { Name = name };
                dbContext.Genres.Add(existing);
            }
            genres.Add(existing);
        }
        return genres;
    }
}
