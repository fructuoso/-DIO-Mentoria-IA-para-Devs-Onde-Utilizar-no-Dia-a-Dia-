using Microsoft.AspNetCore.Mvc;
using Shared.Services;
using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    [HttpPost("login")]
    public ActionResult<LoginResponseDto> Login(LoginDto loginDto)
    {
        try
        {
            // In a real application, you would validate credentials against a database
            // For this demo, we'll use hardcoded credentials
            if (!ValidateCredentials(loginDto.Username, loginDto.Password))
            {
                _logger.LogWarning("Login attempt failed for username: {Username}", loginDto.Username);
                return Unauthorized("Invalid credentials");
            }

            // Determine user role (in real app, this would come from database)
            var role = GetUserRole(loginDto.Username);
            
            // Generate JWT token
            var token = _jwtService.GenerateToken(loginDto.Username, role);
            
            _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
            
            return Ok(new LoginResponseDto
            {
                Token = token,
                Username = loginDto.Username,
                Role = role,
                ExpiresIn = 24 * 60 * 60 // 24 hours in seconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Validates JWT token
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<TokenValidationResponseDto> ValidateToken(TokenValidationDto tokenDto)
    {
        try
        {
            var principal = _jwtService.ValidateToken(tokenDto.Token);
            
            if (principal == null)
            {
                return Ok(new TokenValidationResponseDto { IsValid = false });
            }

            var userId = _jwtService.GetUserIdFromToken(tokenDto.Token);
            var role = _jwtService.GetRoleFromToken(tokenDto.Token);

            return Ok(new TokenValidationResponseDto
            {
                IsValid = true,
                UserId = userId,
                Role = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Ok(new TokenValidationResponseDto { IsValid = false });
        }
    }

    private bool ValidateCredentials(string username, string password)
    {
        // Demo credentials - in real app, validate against database with hashed passwords
        var validCredentials = new Dictionary<string, string>
        {
            { "admin", "admin123" },
            { "customer1", "customer123" },
            { "customer2", "customer123" },
            { "user", "user123" }
        };

        return validCredentials.ContainsKey(username) && validCredentials[username] == password;
    }

    private string GetUserRole(string username)
    {
        // Demo role assignment - in real app, get from database
        return username == "admin" ? "Admin" : "Customer";
    }
}

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class TokenValidationDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class TokenValidationResponseDto
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? Role { get; set; }
}
