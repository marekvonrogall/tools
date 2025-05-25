using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vrmo.Data;
using vrmo.Dto;
using vrmo.Models;
using vrmo.Services;

namespace vrmo.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasherService _hasher;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, PasswordHasherService hasher, IConfiguration config)
    {
        _db = db;
        _hasher = hasher;
        _config = config;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !_hasher.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            return Unauthorized("Invalid username or password.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "admin" : "user")
        };

        var keyString = Environment.GetEnvironmentVariable("VRMO_JWT_KEY");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("VRMO_JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("VRMO_JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
