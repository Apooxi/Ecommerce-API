using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StockManagementAPI.Data;
using StockManagementAPI.DTOs;
using StockManagementAPI.Models;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

namespace StockManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegisterRequest request)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { Messaage = "Bu email adresi zaten kayitli" });
        }
        
        string passwordHas = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHas
        };
        
        _context.Users.Add(newUser);
        _context.SaveChanges();
        
        return Ok(new { Message = "Kullanici basariyla kaydedildi" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { Message = "Email veya sifre hatali" });
        }
        
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        
        if (!isPasswordValid)
        {
            return Unauthorized(new { Message = "Email veya sifre hatali" });
        }
        
        var tokenString = GenerateJwtToken(user);
        
        return Ok(new { Token = tokenString, Message = "Giris basarili"});
        
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecretKey = _configuration.GetSection("JwtSettings:SecretKey").Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), 
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);

    }
}