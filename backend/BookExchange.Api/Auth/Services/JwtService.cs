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
    private readonly IJwtService _context;

    public JwtService(IConfiguration configuration, IJwtService context)
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
          new("LastName", user.LastName),  
        };
    
    // Add role claims
    foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
    }
}
