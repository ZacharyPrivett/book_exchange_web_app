# Complete Authentication Implementation Guide
### ASP.NET Core Identity + JWT + OAuth + Refresh Tokens + Email Verification + Roles

---

## 📋 Table of Contents
1. [Overview](#overview)
2. [Phase 1: Setup & Dependencies](#phase-1-setup--dependencies)
3. [Phase 2: Entity & Database Migration](#phase-2-entity--database-migration)
4. [Phase 3: JWT Configuration](#phase-3-jwt-configuration)
5. [Phase 4: Registration Endpoint](#phase-4-registration-endpoint)
6. [Phase 5: Login Endpoint](#phase-5-login-endpoint)
7. [Phase 6: Refresh Token System](#phase-6-refresh-token-system)
8. [Phase 7: Email Verification](#phase-7-email-verification)
9. [Phase 8: Password Reset Flow](#phase-8-password-reset-flow)
10. [Phase 9: OAuth Providers (Google/Microsoft)](#phase-9-oauth-providers-googlemicrosoft)
11. [Phase 10: Role-Based Authorization](#phase-10-role-based-authorization)
12. [Phase 11: Testing](#phase-11-testing)

---

## Overview

### What We'll Build
- ✅ User registration with email/password
- ✅ Login with JWT token generation
- ✅ Refresh token for automatic re-authentication
- ✅ Email verification
- ✅ Password reset/forgot password
- ✅ OAuth login (Google, Microsoft)
- ✅ Role-based authorization (Admin, User, Moderator)
- ✅ ASP.NET Core Identity for password hashing & user management

### Current State
- Custom `User`, `AuthIdentity`, `Provider` tables (will be replaced)
- Basic JWT Bearer authentication configured
- SQLite database

### Target Architecture
```
User Registration/Login → ASP.NET Core Identity → Generate JWT + Refresh Token
                      ↓
            Store in IdentityUser table
                      ↓
            Protected Endpoints (with [Authorize])
```

### Why This Architecture?

**Why ASP.NET Core Identity?**
- **Battle-tested security:** Microsoft's Identity framework has been hardened over years with security best practices built-in
- **Password hashing:** Automatically uses PBKDF2 with salt, protecting against rainbow table attacks
- **Account lockout:** Prevents brute force attacks by locking accounts after failed attempts
- **Email confirmation & password reset:** Built-in token generation with time-based expiration
- **Less code to maintain:** You don't need to implement password hashing, token generation, or account lockout logic yourself

**Why JWT (JSON Web Tokens)?**
- **Stateless authentication:** Server doesn't need to store session data, making it scalable
- **Works across domains:** Perfect for SPA (Single Page Applications) like your Next.js frontend
- **Contains claims:** Token includes user information (id, roles) so you don't need database lookups on every request
- **Industry standard:** Compatible with many libraries and services

**Why Refresh Tokens?**
- **Security:** Keep access tokens short-lived (1 hour) to minimize damage if stolen
- **User experience:** Refresh tokens allow automatic re-authentication without forcing users to log in again
- **Revocable:** Unlike JWTs, refresh tokens can be revoked in the database

**Why Both JWT and OAuth?**
- **Flexibility:** Some users prefer traditional login, others want "Sign in with Google"
- **User convenience:** OAuth eliminates password fatigue and registration friction
- **Trust:** Users trust Google/Microsoft's security more than unknown sites

---

## Phase 1: Setup & Dependencies

### Step 1.1: Install Required NuGet Packages

Open terminal in your project directory and run:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.24
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.3.1
dotnet add package Microsoft.AspNetCore.Authentication.Google --version 8.0.24
dotnet add package Microsoft.AspNetCore.Authentication.MicrosoftAccount --version 8.0.24
```

**📘 What Each Package Does:**

1. **Microsoft.AspNetCore.Identity.EntityFrameworkCore**
   - Provides `IdentityDbContext` that includes all Identity tables (Users, Roles, Claims, etc.)
   - Contains `UserManager<T>` for user operations (create, find, update, password management)
   - Contains `SignInManager<T>` for authentication operations (password verification, lockout)
   - Contains `RoleManager<T>` for role management
   - **Why we need it:** Core framework for user management with security best practices

2. **System.IdentityModel.Tokens.Jwt**
   - Provides `JwtSecurityTokenHandler` for creating and validating JWT tokens
   - Contains `JwtSecurityToken` class for token representation
   - Supports claims-based authentication
   - **Why we need it:** To generate and validate JWT access tokens for API authentication

3. **Microsoft.AspNetCore.Authentication.Google**
   - OAuth 2.0 middleware for Google authentication
   - Handles the OAuth redirect flow automatically
   - **Why we need it:** Lets users sign in with their Google account

4. **Microsoft.AspNetCore.Authentication.MicrosoftAccount**
   - OAuth 2.0 middleware for Microsoft authentication
   - Supports personal Microsoft accounts and Azure AD
   - **Why we need it:** Lets users sign in with their Microsoft/Outlook account

### Step 1.2: Create Configuration Settings

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "BookExchange": "Data Source=BookExchange.db"
  },
  "Jwt": {
    "SecretKey": "THIS-IS-A-SUPER-SECRET-KEY-CHANGE-THIS-IN-PRODUCTION-MIN-32-CHARS",
    "Issuer": "BookExchangeApi",
    "Audience": "BookExchangeClient",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_MICROSOFT_CLIENT_ID",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET"
    }
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@bookexchange.com",
    "SenderPassword": "YOUR_EMAIL_PASSWORD"
  },
  "AppUrls": {
    "FrontendUrl": "http://localhost:3000",
    "ApiUrl": "https://localhost:7259"
  }
}
```

**📘 Configuration Explained:**

**JWT Settings:**
- **SecretKey:** Used to sign JWT tokens (HMAC-SHA256 algorithm). Must be kept secret and at least 32 characters for security
  - **Why 32+ chars?** Shorter keys are vulnerable to brute force attacks. 256-bit keys (32 bytes) are industry standard
  - **Security tip:** In production, store this in Azure Key Vault or environment variables, NEVER commit to git
  
- **Issuer:** Identifies who created the token. Set to your API's name
  - **Why needed?** Prevents tokens from other systems being accepted by your API
  
- **Audience:** Identifies who the token is intended for (your frontend/client)
  - **Why needed?** Prevents tokens meant for other applications from being used on your API
  
- **ExpiryInMinutes (60):** Access tokens expire after 1 hour
  - **Why short?** If an access token is stolen, attacker only has 1 hour to use it
  - **Trade-off:** Shorter = more secure, but users need to refresh more often
  
- **RefreshTokenExpiryInDays (7):** Refresh tokens last 7 days
  - **Why longer?** Users can stay authenticated for a week without re-entering password
  - **Security:** These are stored in the database and can be revoked if compromised

**OAuth Settings:**
- **ClientId & ClientSecret:** Credentials from Google/Microsoft developer consoles
  - **What they do:** Authenticate your application to Google/Microsoft's OAuth servers
  - **Security:** ClientSecret must be kept private, never exposed to frontend

**Email Settings:**
- **SmtpHost/Port:** Mail server details for sending emails
  - Port 587 uses STARTTLS (encrypted connection after initial handshake)
  - **Why needed?** Send verification emails and password reset links
  
- **SenderEmail/Password:** Credentials for the email account
  - **Gmail tip:** You'll need to create an "App Password" if using Gmail with 2FA enabled

**AppUrls:**
- **FrontendUrl:** Where your Next.js app runs
  - **Why needed?** Used to construct links in emails (confirmation, password reset) that redirect to frontend
  
- **ApiUrl:** Where your API runs
  - **Why needed?** OAuth providers need to know where to redirect after authentication

Update `appsettings.Development.json`:

```json
{
  "Jwt": {
    "SecretKey": "development-secret-key-min-32-characters-long-for-security",
    "Issuer": "BookExchangeApi-Dev",
    "Audience": "BookExchangeClient-Dev",
    "ExpiryInMinutes": 1440,
    "RefreshTokenExpiryInDays": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**📘 Why Different Development Settings?**

1. **ExpiryInMinutes: 1440 (24 hours)**
   - **Development benefit:** You won't be logged out every hour while coding
   - **Production:** Kept at 60 minutes for security
   
2. **RefreshTokenExpiryInDays: 30**
   - **Development benefit:** Stay authenticated for a month during development
   - **Production:** 7 days forces periodic re-authentication for security

3. **Different Issuer/Audience names**
   - **Why?** Ensures development tokens can't be used in production and vice versa
   - **Prevents accidents:** Can't accidentally use a production token in dev environment

4. **Logging settings**
   - `"Default": "Information"` - See all important log messages
   - `"Microsoft.AspNetCore": "Warning"` - Reduces noise from framework logging
   - **Development tip:** Change to `"Debug"` when troubleshooting authentication issues

---

## Phase 2: Entity & Database Migration

**🎯 Goal of This Phase:**
Migrate from your custom `User`, `AuthIdentity`, `Provider` tables to ASP.NET Core Identity's standardized tables. Identity creates tables like `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc., which provide a complete user management system.

**Why Migrate?**
- Your custom tables require manual implementation of password hashing, lockout, email confirmation
- Identity provides these features out-of-the-box with security best practices
- Identity integrates with `UserManager`, `SignInManager`, and role-based authorization
- Identity tables are battle-tested across millions of applications

### Step 2.1: Create ApplicationUser (Extends IdentityUser)

Create `backend/BookExchange.Api/Auth/Entities/ApplicationUser.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace BookExchange.Api.Auth.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    
    // Navigation property for refresh tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
```

**📘 Code Explanation:**

**Why Extend `IdentityUser`?**
- `IdentityUser` provides built-in properties: `Id`, `UserName`, `Email`, `PasswordHash`, `EmailConfirmed`, `SecurityStamp`, `PhoneNumber`, etc.
- **Security features included:**
  - `PasswordHash`: Hashed password (never stored in plain text)
  - `SecurityStamp`: Changes when credentials change, invalidating old tokens
  - `EmailConfirmed`: Tracks email verification status
  - `LockoutEnd`: Implements account lockout for brute force protection
  - `AccessFailedCount`: Tracks failed login attempts

**Custom Properties We're Adding:**
1. **FirstName & LastName**: User's display name
   - **Why separate from UserName?** Username is for login (usually email), display name is for UI
   
2. **AvatarUrl**: Profile picture URL
   - **Why nullable?** Not all users will upload a profile picture
   
3. **DateOfBirth**: User's age information
   - **Why DateOnly?** More precise than DateTime for birthdays, doesn't include time
   
4. **CreatedAt**: Account creation timestamp
   - **Why useful?** Track when users registered, useful for analytics and debugging
   
5. **IsActive & DeletedAt**: Soft delete pattern
   - **Why not hard delete?** Preserve data for audit logs, foreign key integrity
   - **When user "deletes" account:** Set `IsActive = false`, `DeletedAt = DateTime.UtcNow`
   
6. **RefreshTokens navigation property**: One-to-many relationship
   - **Why collection?** Users can be logged in on multiple devices (phone, laptop, tablet)
   - Each device gets its own refresh token

### Step 2.2: Create RefreshToken Entity

Create `backend/BookExchange.Api/Auth/Entities/RefreshToken.cs`:

```csharp
namespace BookExchange.Api.Auth.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
}
```

**📘 Refresh Token Security Deep Dive:**

**What is a Refresh Token?**
A refresh token is a long-lived credential stored in the database that allows getting new access tokens without re-entering the password.

**Why We Need a Separate Table?**
Unlike JWT access tokens (which are stateless), refresh tokens are stored in the database because they need to be:
1. **Revocable**: If a token is compromised, you can mark it as revoked
2. **Trackable**: Know what devices/sessions a user has active
3. **Rotatable**: Create new refresh tokens and invalidate old ones
4. **Limited**: User can only have X active sessions

**Property Breakdown:**

1. **Token (required string)**
   - A cryptographically secure random string (64 bytes, base64-encoded)
   - **Why random?** Impossible to guess or forge
   - **Generated by:** `RandomNumberGenerator.Create()` in JwtService
   
2. **UserId (required string)**
   - Foreign key linking to ApplicationUser
   - **Why string?** IdentityUser uses string Ids (GUIDs) by default
   
3. **User navigation property**
   - EF Core relationship to ApplicationUser
   - `= null!` tells compiler "this will be set by EF Core, don't warn about null"
   
4. **ExpiresAt (DateTime)**
   - When this refresh token expires (typically 7-30 days)
   - **Why expiration?** Even refresh tokens shouldn't last forever
   - **Checked on every refresh:** Expired tokens are rejected
   
5. **CreatedAt (DateTime)**
   - When token was generated
   - **Useful for:** Audit logs, detecting suspicious activity
   
6. **IsRevoked (bool)**
   - Has this token been manually revoked?
   - **When to revoke:**
     - User logs out
     - Password changed (revoke all tokens)
     - Admin disables account
     - Suspicious activity detected
   
7. **RevokedAt (nullable DateTime)**
   - Timestamp of revocation
   - **Why track this?** Security audit trail
   
8. **ReplacedByToken (nullable string)**
   - Token rotation strategy: when refreshing, store the new token value here
   - **Why useful?** Create an audit chain showing how tokens evolved
   - **Security benefit:** If old token is used after rotation, you know something is wrong

**The Refresh Token Flow:**
```
1. User logs in → Receive Access Token (1hr) + Refresh Token (7 days)
2. Access token expires → Frontend calls /auth/refresh with both tokens
3. Server validates refresh token (is it in DB? Not revoked? Not expired?)
4. Server revokes old refresh token
5. Server creates new refresh token
6. Return new access token + new refresh token
```

**Why Rotate Refresh Tokens?**
- Limits the time window for stolen tokens to be useful
- If an old refresh token is reused, it indicates theft (automatic revocation possible)

### Step 2.3: Update BookExchangeContext

Replace `Data/BookExchangeContext.cs`:

```csharp
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Data;

public class BookExchangeContext : IdentityDbContext<ApplicationUser>
{
    public BookExchangeContext(DbContextOptions<BookExchangeContext> options)
        : base(options)
    {
    }

    // Existing DbSets
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Condition> Condition => Set<Condition>();
    
    // New DbSets for Auth
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // IMPORTANT: Call base for Identity tables

        // Existing Genre seed data
        modelBuilder.Entity<Genre>().HasData(
            new { Id = 1, Name = "Fantasy" },
            new { Id = 2, Name = "Science Fiction" },
            new { Id = 3, Name = "Romance" },
            new { Id = 4, Name = "History" },
            new { Id = 5, Name = "Military Thriller" }
        );

        // Existing Condition seed data
        modelBuilder.Entity<Condition>().HasData(
            new { Id = 1, Name = "New" },
            new { Id = 2, Name = "Like New" },
            new { Id = 3, Name = "Good" },
            new { Id = 4, Name = "Fair" },
            new { Id = 5, Name = "Worn" }
        );

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Roles
        var adminRoleId = "1";
        var userRoleId = "2";
        var moderatorRoleId = "3";

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER" },
            new IdentityRole { Id = moderatorRoleId, Name = "Moderator", NormalizedName = "MODERATOR" }
        );
    }
}
```

**📘 DbContext Changes Explained:**

**Key Change: `IdentityDbContext<ApplicationUser>` instead of `DbContext`**

**What This Base Class Provides:**
When you inherit from `IdentityDbContext<ApplicationUser>`, Entity Framework automatically creates these tables:
- `AspNetUsers` - User accounts (your ApplicationUser data)
- `AspNetRoles` - Roles (Admin, User, Moderator)
- `AspNetUserRoles` - Many-to-many relationship (which users have which roles)
- `AspNetUserClaims` - Additional claims per user (extra permissions/data)
- `AspNetRoleClaims` - Claims assigned to roles
- `AspNetUserLogins` - External login providers (Google, Microsoft) linked to users
- `AspNetUserTokens` - Security tokens (email confirmation, password reset)

**⚠️ CRITICAL: `base.OnModelCreating(modelBuilder)`**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder); // MUST be first!
    // ... your configurations
}
```
**Why this matters:**
- The base method configures all Identity tables
- If you skip this, Identity tables won't be created and you'll get runtime errors
- **Must be the first line** in `OnModelCreating` before your customizations

**RefreshToken Configuration:**

1. **Unique Index on Token:**
```csharp
entity.HasIndex(e => e.Token).IsUnique();
```
- **Why?** Each refresh token must be unique across the entire database
- **Performance:** Index makes lookups by token value fast (O(log n) instead of O(n))
- **Integrity:** Database enforces uniqueness, preventing duplicate tokens

2. **One-to-Many Relationship:**
```csharp
entity.HasOne(e => e.User)           // Each RefreshToken has one User
      .WithMany(u => u.RefreshTokens) // Each User has many RefreshTokens
      .HasForeignKey(e => e.UserId)   // Foreign key column
      .OnDelete(DeleteBehavior.Cascade); // Delete tokens when user deleted
```
- **Cascade Delete:** When a user is deleted, all their refresh tokens are automatically deleted
- **Why cascade?** Orphaned tokens would be useless and clutter the database
- **Alternative:** `DeleteBehavior.Restrict` would prevent user deletion if tokens exist

**Role Seeding:**

```csharp
modelBuilder.Entity<IdentityRole>().HasData(
    new IdentityRole { 
        Id = "1", 
        Name = "Admin", 
        NormalizedName = "ADMIN" // MUST be uppercase
    },
    // ... other roles
);
```

**Why Seed Roles?**
- Roles need to exist before you can assign them to users
- Seeding ensures these roles are created when the database is initialized
- **NormalizedName**: Identity stores uppercase versions for case-insensitive lookups
  - When you check `if (user.IsInRole("admin"))`, it compares against `NormalizedName`

**Role Purposes:**
- **Admin**: Full system access, user management, configuration
- **User**: Default role for regular registered users
- **Moderator**: Can manage content but not users/system settings

**Why string IDs ("1", "2", "3")?**
- `IdentityRole` uses string IDs by default (GUIDs in production)
- Simple strings are fine for seed data since these are created once
- In production, you might use GUIDs: `Guid.NewGuid().ToString()`

### Step 2.4: Update Program.cs

Replace `Program.cs`:

```csharp
using System.Text;
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Auth.Services;
using BookExchange.Api.Data;
using BookExchange.Api.Endpoints;
using BookExchange.Api.UserManagement.UserEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Database
var connString = builder.Configuration.GetConnectionString("BookExchange");
builder.Services.AddDbContext<BookExchangeContext>(options =>
    options.UseSqlite(connString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true; // Require email confirmation
})
.AddEntityFrameworkStores<BookExchangeContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
})
.AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? "";
});

// Authorization
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register custom services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapBooksEndpoints();
app.MapGenresEndpoints();
app.MapConditionsEndpoints();
app.MapAuthEndpoints(); // New auth endpoints
app.MapUserEndpoints();  // Protected user endpoints

// Database migration
await app.MigrateDbAsync();

app.Run();
```

**📘 Program.cs Configuration Explained (Line by Line):**

---

**1. Identity Configuration:**

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { ... })
```

**What `AddIdentity` does:**
- Registers `UserManager<ApplicationUser>` for user CRUD operations
- Registers `SignInManager<ApplicationUser>` for authentication
- Registers `RoleManager<IdentityRole>` for role management
- Configures cookie-based authentication (we'll override this with JWT)

**Password Settings:**
```csharp
options.Password.RequireDigit = true;           // Must contain 0-9
options.Password.RequireLowercase = true;       // Must contain a-z
options.Password.RequireUppercase = true;       // Must contain A-Z
options.Password.RequireNonAlphanumeric = false; // NO special characters required (!@#$%)
options.Password.RequiredLength = 8;            // Minimum 8 characters
```

**Why these rules?**
- **Industry standard**: Strong enough to resist brute force, not too complex users forget them
- **No special chars**: Often causes UX issues (users forget which symbol they used)
- **8 characters minimum**: NIST recommends 8+ characters
- **Example valid password**: `Password123` (has upper, lower, digit, 8+ chars)

**Lockout Settings:**
```csharp
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.AllowedForNewUsers = true;
```

**Why lockout?**
- **Prevents brute force attacks**: Attackers can't try thousands of passwords
- **5 attempts**: Generous enough for typos, tight enough for security
- **15 minute lockout**: Long enough to deter bots, short enough users aren't frustrated
- **New users included**: Even unconfirmed accounts are protected

**How lockout works:**
1. User fails login → `AccessFailedCount++`
2. After 5 failures → `LockoutEnd = DateTime.UtcNow + 15 minutes`
3. All login attempts rejected until `LockoutEnd` expires
4. Successful login → `AccessFailedCount = 0`

**User Settings:**
```csharp
options.User.RequireUniqueEmail = true;
options.SignIn.RequireConfirmedEmail = true;
```

- **RequireUniqueEmail**: Prevents multiple accounts with same email
  - **Why?** One person shouldn't have multiple accounts, avoids abuse
  - Database enforces uniqueness on `Email` column
  
- **RequireConfirmedEmail**: User must click email link before they can log in
  - **Why?** Verifies the email address is real and user owns it
  - **Security**: Prevents fake email registration spam
  - **UX consideration**: User gets "Please confirm email" error if they try to login before confirming

**AddEntityFrameworkStores & AddDefaultTokenProviders:**
```csharp
.AddEntityFrameworkStores<BookExchangeContext>()
.AddDefaultTokenProviders();
```

- **AddEntityFrameworkStores**: Tells Identity to use your `BookExchangeContext` for data storage
- **AddDefaultTokenProviders**: Enables token generation for:
  - Email confirmation tokens
  - Password reset tokens
  - Two-factor authentication tokens
  - These tokens are time-limited and cryptographically secure

---

**2. JWT Authentication Configuration:**

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
```

**DefaultAuthenticateScheme:**
- Tells ASP.NET Core to use JWT Bearer tokens by default
- When `[Authorize]` attribute is used, this scheme is checked
- **Alternative schemes**: Cookie, Certificate, Windows authentication

**DefaultChallengeScheme:**
- What happens when authentication fails
- JWT returns `401 Unauthorized` HTTP status
- **Cookie scheme would**: Redirect to login page (not suitable for APIs)

**Token Validation Parameters:**
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    ClockSkew = TimeSpan.Zero
};
```

**Each validation explained:**

1. **ValidateIssuer = true** (check who created the token)
   - Verifies token's `iss` claim matches `ValidIssuer` ("BookExchangeApi")
   - **Protects against**: Tokens from other systems being accepted
   - **Example attack**: Attacker creates token claiming to be from your API

2. **ValidateAudience = true** (check who token is for)
   - Verifies token's `aud` claim matches `ValidAudience` ("BookExchangeClient")
   - **Protects against**: Tokens meant for different apps being used on your API
   - **Example attack**: Token for mobile app used on web API

3. **ValidateLifetime = true** (check if token expired)
   - Verifies token's `exp` claim (expiration) hasn't passed
   - **Protects against**: Old stolen tokens being reused indefinitely
   - **How it works**: `DateTime.UtcNow < token.exp`

4. **ValidateIssuerSigningKey = true** (check token signature)
   - Verifies token was signed with your secret key
   - **Most critical validation**: Prevents token forgery
   - **How it works**: Re-computes HMAC-SHA256 signature and compares

5. **IssuerSigningKey** (your secret key)
   - Used to verify the signature
   - **HMAC-SHA256 algorithm**: Creates a hash of the token using this key
   - **Why symmetric?** Same key signs and validates (vs asymmetric RSA with public/private keys)

6. **ClockSkew = TimeSpan.Zero** (no grace period)
   - **Default behavior**: ASP.NET Core allows 5 extra minutes after token expiration
   - **Why remove it?** Stricter security - tokens expire exactly when they should
   - **Trade-off**: Servers with out-of-sync clocks might reject valid tokens
   - **Production tip**: Keep small clock skew (30 seconds) to handle minor time differences

**How Token Validation Flows:**
```
1. Request arrives with: Authorization: Bearer eyJhbGc...
2. Extract JWT token from header
3. Decode token (base64) into header, payload, signature
4. Verify signature using IssuerSigningKey
5. Check Issuer matches "BookExchangeApi"
6. Check Audience matches "BookExchangeClient"
7. Check exp (expiration) < DateTime.UtcNow
8. If all pass → User authenticated, populate ClaimsPrincipal
9. If any fail → Return 401 Unauthorized
```

---

**3. OAuth Provider Configuration:**

```csharp
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
})
```

**What this does:**
- Registers Google OAuth 2.0 authentication middleware
- Handles the entire OAuth flow automatically:
  1. Redirect user to Google login page
  2. User authenticates with Google
  3. Google redirects back with authorization code
  4. Middleware exchanges code for user info (email, name, id)
  5. Creates `ExternalLoginInfo` with user's Google data

**Why Google OAuth?**
- **No password management**: You don't store or validate passwords
- **Trust**: Users trust Google more than unknown sites
- **Less friction**: One-click login without registration form
- **Security**: Google handles 2FA, account security, breach notifications

**ClientId vs ClientSecret:**
- **ClientId**: Public identifier for your app (safe to expose)
- **ClientSecret**: Private key proving your app's identity (MUST be secret)
- **Security**: ClientSecret stays on your server, never sent to frontend

---

**4. CORS Configuration:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

**Why CORS?**
- **Browser security**: By default, browsers block requests from `http://localhost:3000` (frontend) to `https://localhost:7259` (API)
- **Cross-Origin Resource Sharing**: Explicitly allows your frontend to call your API

**Configuration breakdown:**

1. **WithOrigins()**: Whitelist of allowed frontend URLs
   - **Why both http/https?** Development flexibility
   - **Production**: Change to your deployed domain `https://bookexchange.com`

2. **AllowAnyHeader()**: Allows any HTTP headers
   - **Examples**: `Authorization: Bearer ...`, `Content-Type: application/json`
   - **Security note**: In high-security apps, whitelist specific headers

3. **AllowAnyMethod()**: Allows GET, POST, PUT, DELETE, etc.
   - **Flexibility**: Frontend can use any HTTP verb
   - **Security note**: Your `[Authorize]` attributes still protect endpoints

4. **AllowCredentials()**: **CRITICAL for authentication**
   - Allows cookies and authorization headers to be sent
   - **Why needed?** Without this, `Authorization` header is stripped by browser
   - **Must be paired with**: Specific origins (can't use `AllowAnyOrigin()` with credentials)

---

**5. Custom Services Registration:**

```csharp
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
```

**What is Dependency Injection?**
- Instead of `new JwtService()` everywhere, ASP.NET Core injects instances automatically
- **Benefits**: Testability, loose coupling, easier to swap implementations

**AddScoped lifetime:**
- One instance created **per HTTP request**
- **Why scoped?** These services use `DbContext`, which is scoped
- **Alternatives:**
  - **Singleton**: One instance for entire app (good for stateless helpers, bad for database services)
  - **Transient**: New instance every time (expensive, rarely needed)

**Scoped lifetime flow:**
```
1. HTTP request arrives → Create new DbContext
2. Controller needs JwtService → Create new JwtService (injects DbContext)
3. JwtService uses DbContext to query refresh tokens
4. Request completes → Dispose DbContext and JwtService
5. Next request → Fresh instances created
```

---

**6. Middleware Pipeline:**

```csharp
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
```

**⚠️ ORDER MATTERS - THESE MUST BE IN THIS EXACT ORDER:**

1. **UseCors()** - MUST be before Authentication
   - **Why first?** Need to handle CORS preflight requests (OPTIONS) before auth
   - **Preflight**: Browser asks "Can I make this request?" before sending actual request

2. **UseAuthentication()** - MUST be before Authorization
   - **What it does**: Reads JWT token, validates it, populates `HttpContext.User`
   - **Output**: `ClaimsPrincipal` with user's ID, roles, claims

3. **UseAuthorization()** - MUST be after Authentication
   - **What it does**: Checks if authenticated user has permission
   - **Reads**: `HttpContext.User` (populated by Authentication middleware)
   - **Enforces**: `[Authorize]` attributes, role requirements, policies

**Why this order?**
```
Request → CORS check → Extract & validate JWT → Check permissions → Endpoint

If you swap Authorization/Authentication:
Request → Extract JWT → Check permissions (User = null!) → FAIL
```

---

**7. Endpoint Mapping:**

```csharp
app.MapAuthEndpoints();   // /auth/login, /auth/register, etc.
app.MapUserEndpoints();    // /users/me (requires authentication)
```

**Map vs MapGroup:**
- These are extension methods you'll create in `AuthEndpoints.cs`
- **MapGroup**: Creates route grouping with shared prefix
- **Example**: `app.MapGroup("auth")` creates `/auth/login`, `/auth/register`, etc.

This  is a MASSIVE amount of configuration, but each piece is critical for security!

### Step 2.5: Create Database Migration

Remove old User tables and create Identity tables:

```bash
# Remove old migrations (if you want a clean slate)
# rm -r Data/Migrations/*

# Create new migration
dotnet ef migrations add AddIdentityTables

# Apply migration
dotnet ef database update
```

---

## Phase 3: JWT Configuration

### Step 3.1: Create JWT Service Interface

Create `backend/BookExchange.Api/Auth/Services/IJwtService.cs`:

```csharp
using BookExchange.Api.Auth.Entities;
using System.Security.Claims;

namespace BookExchange.Api.Auth.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<RefreshToken> CreateRefreshTokenAsync(string userId);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}
```

### Step 3.2: Implement JWT Service

Create `backend/BookExchange.Api/Auth/Services/JwtService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookExchange.Api.Auth.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly BookExchangeContext _context;

    public JwtService(IConfiguration configuration, BookExchangeContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Don't validate lifetime here
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        var expiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> GetValidRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
```

---

## Phase 4: Registration Endpoint

### Step 4.1: Create DTOs

Create `backend/BookExchange.Api/Auth/Dtos/RegisterDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth
);
```

Create `backend/BookExchange.Api/Auth/Dtos/AuthResponseDto.cs`:

```csharp
namespace BookExchange.Api.Auth.Dtos;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    List<string> Roles
);
```

### Step 4.2: Create Auth Endpoints

Create `backend/BookExchange.Api/Auth/Endpoints/AuthEndpoints.cs`:

```csharp
using BookExchange.Api.Auth.Dtos;
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookExchange.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("auth").WithTags("Authentication");

        // Registration
        authGroup.MapPost("register", Register)
            .WithName("Register")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        return authGroup;
    }

    // REGISTER
    private static async Task<IResult> Register(
        RegisterDto dto,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return Results.BadRequest(new { message = "User with this email already exists" });
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // Assign default "User" role
        await userManager.AddToRoleAsync(user, "User");

        // Generate email confirmation token
        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = configuration["AppUrls:FrontendUrl"];
        var confirmationLink = $"{frontendUrl}/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";

        // Send confirmation email (implement this in EmailService)
        await emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);

        // Generate tokens
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtService.GenerateAccessToken(user, roles);
        var refreshToken = await jwtService.CreateRefreshTokenAsync(user.Id);

        var response = new AuthResponseDto(
            accessToken,
            refreshToken.Token,
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles.ToList()
        );

        return Results.Created($"/auth/user/{user.Id}", response);
    }
}
```

---

## Phase 5: Login Endpoint

### Step 5.1: Create Login DTO

Create `backend/BookExchange.Api/Auth/Dtos/LoginDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password,
    bool RememberMe = false
);
```

### Step 5.2: Add Login to AuthEndpoints

Add to `Auth/Endpoints/AuthEndpoints.cs`:

```csharp
// Add to MapAuthEndpoints method:
authGroup.MapPost("login", Login)
    .WithName("Login")
    .Produces<AuthResponseDto>(StatusCodes.Status200OK)
    .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

// Add this method:
private static async Task<IResult> Login(
    LoginDto dto,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService)
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    // Check if email is confirmed
    if (!await userManager.IsEmailConfirmedAsync(user))
    {
        return Results.BadRequest(new { message = "Email not confirmed. Please check your inbox." });
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

    if (!result.Succeeded)
    {
        if (result.IsLockedOut)
        {
            return Results.BadRequest(new { message = "Account locked due to multiple failed login attempts." });
        }
        return Results.Unauthorized();
    }

    // Generate tokens
    var roles = await userManager.GetRolesAsync(user);
    var accessToken = jwtService.GenerateAccessToken(user, roles);
    var refreshToken = await jwtService.CreateRefreshTokenAsync(user.Id);

    var response = new AuthResponseDto(
        accessToken,
        refreshToken.Token,
        user.Id,
        user.Email!,
        user.FirstName,
        user.LastName,
        roles.ToList()
    );

    return Results.Ok(response);
}
```

---

## Phase 6: Refresh Token System

### Step 6.1: Create Refresh Token DTO

Create `backend/BookExchange.Api/Auth/Dtos/RefreshTokenDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record RefreshTokenDto(
    [Required] string AccessToken,
    [Required] string RefreshToken
);
```

### Step 6.2: Add Refresh Token Endpoint

Add to `Auth/Endpoints/AuthEndpoints.cs`:

```csharp
// Add to MapAuthEndpoints method:
authGroup.MapPost("refresh", RefreshToken)
    .WithName("RefreshToken")
    .Produces<AuthResponseDto>(StatusCodes.Status200OK)
    .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

// Add this method:
private static async Task<IResult> RefreshToken(
    RefreshTokenDto dto,
    UserManager<ApplicationUser> userManager,
    IJwtService jwtService)
{
    // Validate the expired access token
    var principal = jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
    if (principal == null)
    {
        return Results.BadRequest(new { message = "Invalid access token" });
    }

    var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
    {
        return Results.BadRequest(new { message = "Invalid token claims" });
    }

    // Validate refresh token
    var refreshToken = await jwtService.GetValidRefreshTokenAsync(dto.RefreshToken);
    if (refreshToken == null || refreshToken.UserId != userId)
    {
        return Results.BadRequest(new { message = "Invalid or expired refresh token" });
    }

    var user = await userManager.FindByIdAsync(userId);
    if (user == null || !user.IsActive)
    {
        return Results.BadRequest(new { message = "User not found or inactive" });
    }

    // Revoke old refresh token and create new one
    await jwtService.RevokeRefreshTokenAsync(dto.RefreshToken);
    var newRefreshToken = await jwtService.CreateRefreshTokenAsync(user.Id);

    // Generate new access token
    var roles = await userManager.GetRolesAsync(user);
    var newAccessToken = jwtService.GenerateAccessToken(user, roles);

    var response = new AuthResponseDto(
        newAccessToken,
        newRefreshToken.Token,
        user.Id,
        user.Email!,
        user.FirstName,
        user.LastName,
        roles.ToList()
    );

    return Results.Ok(response);
}
```

---

## Phase 7: Email Verification

### Step 7.1: Create Email Service

Create `backend/BookExchange.Api/Auth/Services/IEmailService.cs`:

```csharp
namespace BookExchange.Api.Auth.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
}
```

Create `backend/BookExchange.Api/Auth/Services/EmailService.cs`:

```csharp
using System.Net;
using System.Net.Mail;

namespace BookExchange.Api.Auth.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        var subject = "Confirm your email - Book Exchange";
        var body = $@"
            <h1>Welcome to Book Exchange!</h1>
            <p>Please confirm your email by clicking the link below:</p>
            <a href='{confirmationLink}'>Confirm Email</a>
            <p>If you didn't create this account, please ignore this email.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        var subject = "Password Reset - Book Exchange";
        var body = $@"
            <h1>Password Reset Request</h1>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <p>If you didn't request this, please ignore this email.</p>
            <p>This link expires in 1 hour.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderPassword = _configuration["Email:SenderPassword"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail ?? "noreply@bookexchange.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // In production, you might want to queue this for retry
        }
    }
}
```

### Step 7.2: Add Email Confirmation Endpoint

Add to `Auth/Endpoints/AuthEndpoints.cs`:

```csharp
// Add to MapAuthEndpoints method:
authGroup.MapGet("confirm-email", ConfirmEmail)
    .WithName("ConfirmEmail")
    .Produces(StatusCodes.Status200OK)
    .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

// Add this method:
private static async Task<IResult> ConfirmEmail(
    string userId,
    string token,
    UserManager<ApplicationUser> userManager)
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return Results.BadRequest(new { message = "Invalid user ID" });
    }

    var result = await userManager.ConfirmEmailAsync(user, token);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    return Results.Ok(new { message = "Email confirmed successfully" });
}
```

---

## Phase 8: Password Reset Flow

### Step 8.1: Create Password Reset DTOs

Create `backend/BookExchange.Api/Auth/Dtos/ForgotPasswordDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record ForgotPasswordDto(
    [Required][EmailAddress] string Email
);
```

Create `backend/BookExchange.Api/Auth/Dtos/ResetPasswordDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record ResetPasswordDto(
    [Required][EmailAddress] string Email,
    [Required] string Token,
    [Required][MinLength(8)] string NewPassword
);
```

### Step 8.2: Add Password Reset Endpoints

Add to `Auth/Endpoints/AuthEndpoints.cs`:

```csharp
// Add to MapAuthEndpoints method:
authGroup.MapPost("forgot-password", ForgotPassword)
    .WithName("ForgotPassword")
    .Produces(StatusCodes.Status200OK);

authGroup.MapPost("reset-password", ResetPassword)
    .WithName("ResetPassword")
    .Produces(StatusCodes.Status200OK)
    .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

// Add these methods:
private static async Task<IResult> ForgotPassword(
    ForgotPasswordDto dto,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IConfiguration configuration)
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    
    // Don't reveal if user exists (security best practice)
    if (user == null || !await userManager.IsEmailConfirmedAsync(user))
    {
        return Results.Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
    var frontendUrl = configuration["AppUrls:FrontendUrl"];
    var resetLink = $"{frontendUrl}/auth/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={Uri.EscapeDataString(resetToken)}";

    await emailService.SendPasswordResetAsync(user.Email!, resetLink);

    return Results.Ok(new { message = "If the email exists, a reset link has been sent." });
}

private static async Task<IResult> ResetPassword(
    ResetPasswordDto dto,
    UserManager<ApplicationUser> userManager)
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        return Results.BadRequest(new { message = "Invalid request" });
    }

    var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    return Results.Ok(new { message = "Password reset successfully" });
}
```

---

## Phase 9: OAuth Providers (Google/Microsoft)

### Step 9.1: Setup OAuth Credentials

**Google OAuth Setup:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Navigate to **APIs & Services > Credentials**
4. Click **Create Credentials > OAuth client ID**
5. Application type: **Web application**
6. Authorized redirect URIs: `https://localhost:7259/signin-google`
7. Copy **Client ID** and **Client Secret** to `appsettings.json`

**Microsoft OAuth Setup:**
1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to **Azure Active Directory > App registrations**
3. Click **New registration**
4. Redirect URI: `https://localhost:7259/signin-microsoft`
5. Copy **Application (client) ID** and create **Client Secret**
6. Add to `appsettings.json`

### Step 9.2: Create External Login Endpoints

Add to `Auth/Endpoints/AuthEndpoints.cs`:

```csharp
// Add to MapAuthEndpoints method:
authGroup.MapGet("external-login/{provider}", ExternalLogin)
    .WithName("ExternalLogin")
    .Produces(StatusCodes.Status302Found);

authGroup.MapGet("external-login-callback", ExternalLoginCallback)
    .WithName("ExternalLoginCallback")
    .Produces<AuthResponseDto>(StatusCodes.Status200OK);

// Add these methods:
private static IResult ExternalLogin(
    string provider,
    string? returnUrl,
    IConfiguration configuration)
{
    var redirectUrl = $"{configuration["AppUrls:ApiUrl"]}/auth/external-login-callback?returnUrl={returnUrl}";
    var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = redirectUrl
    };

    return Results.Challenge(properties, new[] { provider });
}

private static async Task<IResult> ExternalLoginCallback(
    string? returnUrl,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    IConfiguration configuration)
{
    var info = await signInManager.GetExternalLoginInfoAsync();
    if (info == null)
    {
        return Results.Redirect($"{configuration["AppUrls:FrontendUrl"]}/auth/login?error=external_auth_failed");
    }

    // Try to sign in with external login provider
    var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

    ApplicationUser user;

    if (result.Succeeded)
    {
        // User already has account with this external provider
        user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) 
               ?? throw new Exception("User not found after successful external login");
    }
    else
    {
        // Create new account
        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        if (email == null)
        {
            return Results.Redirect($"{configuration["AppUrls:FrontendUrl"]}/auth/login?error=no_email");
        }

        user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.GivenName) ?? "",
                LastName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Surname) ?? "",
                EmailConfirmed = true, // Email verified by external provider
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Results.Redirect($"{configuration["AppUrls:FrontendUrl"]}/auth/login?error=user_creation_failed");
            }

            await userManager.AddToRoleAsync(user, "User");
        }

        // Link external login to user account
        await userManager.AddLoginAsync(user, info);
    }

    // Generate tokens
    var roles = await userManager.GetRolesAsync(user);
    var accessToken = jwtService.GenerateAccessToken(user, roles);
    var refreshToken = await jwtService.CreateRefreshTokenAsync(user.Id);

    // Redirect to frontend with tokens
    var frontendUrl = configuration["AppUrls:FrontendUrl"];
    return Results.Redirect($"{frontendUrl}/auth/callback?accessToken={accessToken}&refreshToken={refreshToken.Token}");
}
```

---

## Phase 10: Role-Based Authorization

### Step 10.1: Update UserEndpoints with Authorization

Update `UserManagement/UserEndpoints/UserEndpoints.cs`:

```csharp
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BookExchange.Api.UserManagement.UserEndpoints;

public static class UserEndpoint
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("users")
            .WithTags("Users")
            .RequireAuthorization(); // All endpoints require authentication

        // Get current user profile
        userGroup.MapGet("me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<UserProfileDto>(StatusCodes.Status200OK);

        // Get all users (Admin only)
        userGroup.MapGet("", GetAllUsers)
            .RequireAuthorization("Admin")
            .WithName("GetAllUsers")
            .Produces<List<UserProfileDto>>(StatusCodes.Status200OK);

        // Delete user (Admin only)
        userGroup.MapDelete("{userId}", DeleteUser)
            .RequireAuthorization("Admin")
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent);

        return userGroup;
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        UserManager<ApplicationUser> userManager)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);

        var userDto = new UserProfileDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.AvatarUrl,
            user.DateOfBirth,
            roles.ToList()
        );

        return Results.Ok(userDto);
    }

    private static async Task<IResult> GetAllUsers(
        UserManager<ApplicationUser> userManager)
    {
        var users = userManager.Users.Where(u => u.IsActive).ToList();
        
        var userDtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userDtos.Add(new UserProfileDto(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.DateOfBirth,
                roles.ToList()
            ));
        }

        return Results.Ok(userDtos);
    }

    private static async Task<IResult> DeleteUser(
        string userId,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound();
        }

        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return Results.NoContent();
    }
}

public record UserProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    DateOnly? DateOfBirth,
    List<string> Roles
);
```

### Step 10.2: Configure Authorization Policies

Add to `Program.cs` after `builder.Services.AddAuthorization()`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Moderator", policy => policy.RequireRole("Admin", "Moderator"));
    options.AddPolicy("User", policy => policy.RequireRole("Admin", "Moderator", "User"));
});
```

### Step 10.3: Create Role Management Endpoints (Admin Only)

Add to `Auth/Endpoints/AuthEndpoints.cs`:

```csharp
// Add to MapAuthEndpoints method:
var adminGroup = authGroup.MapGroup("admin")
    .RequireAuthorization("Admin")
    .WithTags("Admin");

adminGroup.MapPost("assign-role", AssignRole)
    .WithName("AssignRole");

adminGroup.MapPost("remove-role", RemoveRole)
    .WithName("RemoveRole");

// Add these methods:
private static async Task<IResult> AssignRole(
    AssignRoleDto dto,
    UserManager<ApplicationUser> userManager)
{
    var user = await userManager.FindByIdAsync(dto.UserId);
    if (user == null)
    {
        return Results.NotFound(new { message = "User not found" });
    }

    var result = await userManager.AddToRoleAsync(user, dto.RoleName);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    return Results.Ok(new { message = $"Role '{dto.RoleName}' assigned to user" });
}

private static async Task<IResult> RemoveRole(
    AssignRoleDto dto,
    UserManager<ApplicationUser> userManager)
{
    var user = await userManager.FindByIdAsync(dto.UserId);
    if (user == null)
    {
        return Results.NotFound(new { message = "User not found" });
    }

    var result = await userManager.RemoveFromRoleAsync(user, dto.RoleName);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    return Results.Ok(new { message = $"Role '{dto.RoleName}' removed from user" });
}
```

Create `Auth/Dtos/AssignRoleDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record AssignRoleDto(
    [Required] string UserId,
    [Required] string RoleName // "Admin", "User", "Moderator"
);
```

---

## Phase 11: Testing

### Step 11.1: Create Test HTTP File

Create `backend/BookExchange.Api/auth.http`:

```http
### Variables
@baseUrl = https://localhost:7259
@accessToken = 
@refreshToken = 

### 1. Register new user
POST {{baseUrl}}/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-01-01"
}

### 2. Login
POST {{baseUrl}}/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#"
}

### 3. Get current user (requires token)
GET {{baseUrl}}/users/me
Authorization: Bearer {{accessToken}}

### 4. Refresh token
POST {{baseUrl}}/auth/refresh
Content-Type: application/json

{
  "accessToken": "{{accessToken}}",
  "refreshToken": "{{refreshToken}}"
}

### 5. Forgot password
POST {{baseUrl}}/auth/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}

### 6. Reset password
POST {{baseUrl}}/auth/reset-password
Content-Type: application/json

{
  "email": "test@example.com",
  "token": "PASTE_TOKEN_FROM_EMAIL",
  "newPassword": "NewPassword123!@#"
}

### 7. Get all users (Admin only)
GET {{baseUrl}}/users
Authorization: Bearer {{accessToken}}

### 8. Assign role (Admin only)
POST {{baseUrl}}/auth/admin/assign-role
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "userId": "USER_ID_HERE",
  "roleName": "Admin"
}

### 9. External login (Google)
GET {{baseUrl}}/auth/external-login/Google?returnUrl=/dashboard

### 10. Confirm email
GET {{baseUrl}}/auth/confirm-email?userId=USER_ID&token=EMAIL_TOKEN
```

### Step 11.2: Test Workflow

1. **Run the application:**
   ```bash
   cd backend/BookExchange.Api
   dotnet run
   ```

2. **Test Registration:**
   - Send POST to `/auth/register`
   - Check email for confirmation link (or check logs)
   - Save the `accessToken` and `refreshToken` from response

3. **Confirm Email:**
   - Click confirmation link or call `/auth/confirm-email`

4. **Test Login:**
   - Send POST to `/auth/login`
   - Verify you receive tokens

5. **Test Protected Endpoint:**
   - Call `/users/me` with Bearer token

6. **Test Refresh Token:**
   - Wait for access token to expire, or force it
   - Call `/auth/refresh` to get new tokens

7. **Test Password Reset:**
   - Call `/auth/forgot-password`
   - Check email for reset link
   - Call `/auth/reset-password` with token

8. **Test OAuth:**
   - Navigate to `/auth/external-login/Google` in browser
   - Complete Google sign-in
   - Verify redirect with tokens

---

## 🎉 Congratulations!

You now have a fully functional authentication system with:

✅ **User Registration** with email/password  
✅ **Login** with JWT tokens  
✅ **Refresh Tokens** for automatic re-authentication  
✅ **Email Verification** after registration  
✅ **Password Reset** with forgot password flow  
✅ **OAuth Login** with Google and Microsoft  
✅ **Role-Based Authorization** (Admin, User, Moderator)  
✅ **Secure Password Storage** with ASP.NET Core Identity  
✅ **Protected API Endpoints** with [Authorize] attribute

---

## Next Steps

1. **Frontend Integration:** Connect your Next.js frontend to these endpoints
2. **Email Templates:** Improve email templates with HTML/CSS
3. **Rate Limiting:** Add rate limiting to prevent brute force attacks
4. **Two-Factor Authentication:** Add 2FA for extra security
5. **Audit Logging:** Log authentication events
6. **Token Blacklisting:** Add token revocation on logout
7. **Production Security:** Store secrets in Azure Key Vault or similar

---

## Troubleshooting

### Email not sending
- Check SMTP credentials in `appsettings.json`
- For Gmail, enable "Less secure app access" or use App Password
- Check firewall/antivirus blocking port 587

### OAuth not working
- Verify redirect URIs match exactly in Google/Microsoft console
- Check ClientId and ClientSecret are correct
- Ensure HTTPS is enabled (OAuth requires HTTPS)

### Token validation fails
- Verify `Jwt:SecretKey` is at least 32 characters
- Check clock skew between servers
- Verify Issuer and Audience match in token generation and validation

### Database errors
- Run `dotnet ef database update` after each migration
- Check connection string in `appsettings.json`
- Delete database file and recreate if needed (`BookExchange.db`)

---

## Need Help?

If you encounter any issues, check:
1. Application logs in console
2. Browser developer console (for frontend issues)
3. Database state with DB Browser for SQLite
4. Network tab in browser dev tools (check API responses)

Good luck! 🚀
