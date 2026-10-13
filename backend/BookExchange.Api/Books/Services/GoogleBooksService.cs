using System.Text.Json;
using BookExchange.Api.Books.Dtos;

namespace BookExchange.Api.Books.Services;

public class GoogleBooksService : IGoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleBooksService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<BookSearchResultDto>> SearchAsync(string query, int maxResults = 10)
    {
        var url = BuildUrl($"intitle:{Uri.EscapeDataString(query)}", maxResults);
        return await FetchVolumeAsync(url);
    }

    public async Task<BookSearchResultDto?> GetByIsbnAsync(string isbn)
    {
        var url = BuildUrl($"isbn:{isbn}", maxResults: 1);
        var results = await FetchVolumeAsync(url);
        return results.FirstOrDefault();
    }

    public async Task<BookSearchResultDto?> GetByIdAsync(string googleBooksId)
    {
        // Google Books volume IDs aren't ISBNs, so they need the dedicated /volumes/{id}
        // endpoint rather than a q=isbn:... search.
        var apiKey = _configuration["GoogleBooks:ApiKey"];
        var keyPart = string.IsNullOrEmpty(apiKey) ? "" : $"?key={apiKey}";
        var url = $"https://www.googleapis.com/books/v1/volumes/{Uri.EscapeDataString(googleBooksId)}{keyPart}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return ParseVolume(doc.RootElement);
    }

            private string BuildUrl(string q, int maxResults)
    {
        var apiKey = _configuration["GoogleBooks:ApiKey"];
        var keyPart = string.IsNullOrEmpty(apiKey) ? "" : $"&key={apiKey}";
        return $"https://www.googleapis.com/books/v1/volumes?q={q}&maxResults={maxResults}{keyPart}";
    }

    private async Task<IReadOnlyList<BookSearchResultDto>> FetchVolumeAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!doc.RootElement.TryGetProperty("items", out var items))
        {
            return [];
        }

        var results = new List<BookSearchResultDto>();
        foreach (var item in items.EnumerateArray())
        {
            var dto = ParseVolume(item);
            if (dto is not null)
            {
                results.Add(dto);
            }
        }

        return results;
    }

    private static BookSearchResultDto? ParseVolume(JsonElement item)
    {
        if (!item.TryGetProperty("id", out var idProp))
        {
            return null;
        }

        var googleBooksId = idProp.GetString() ?? "";
        var info = item.GetProperty("volumeInfo");

        var title = info.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";

        var authors = info.TryGetProperty("authors", out var a)
            ? a.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => s != "").ToList()
            : [];

        var categories = info.TryGetProperty("categories", out var c)
            ? c.EnumerateArray()
                .SelectMany(x => (x.GetString() ?? "").Split('/'))
                .Select(s => s.Trim())
                .Where(s => s != "")
                .Distinct()
                .ToList()
            : [];

        var publisher = info.TryGetProperty("publisher", out var pub) ? pub.GetString() : null;
        var description = info.TryGetProperty("description", out var d) ? d.GetString() : null;
        int? pageCount = info.TryGetProperty("pageCount", out var pc) ? pc.GetInt32() : null;

        int? publishedYear = null;
        if (info.TryGetProperty("publishedDate", out var pd))
        {
            var dateStr = pd.GetString() ?? "";
            if (dateStr.Length >= 4 && int.TryParse(dateStr[..4], out var year))
            {
                publishedYear = year;
            }
        }

        string? coverUrl = null;
        if (info.TryGetProperty("imageLinks", out var images) &&
            images.TryGetProperty("thumbnail", out var thumb))
        {
            coverUrl = thumb.GetString();
        }

        string? isbn10 = null, isbn13 = null;
        if (info.TryGetProperty("industryIdentifiers", out var ids))
        {
            foreach (var idEntry in ids.EnumerateArray())
            {
                var type = idEntry.GetProperty("type").GetString();
                var value = idEntry.GetProperty("identifier").GetString();
                if (type == "ISBN_10") isbn10 = value;
                if (type == "ISBN_13") isbn13 = value;
            }
        }

        return new BookSearchResultDto(
            CatalogId: null,
            GoogleBooksId: googleBooksId,
            Title: title,
            Authors: authors,
            Categories: categories,
            Publisher: publisher,
            PublishedYear: publishedYear,
            PageCount: pageCount,
            CoverImageUrl: coverUrl,
            Description: description,
            Isbn10: isbn10,
            Isbn13: isbn13,
            Source: BookSearchResultSource.GoogleBooks);
    }
}

