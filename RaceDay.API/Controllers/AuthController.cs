using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(
            RaceDayDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return Conflict(new { message = "Email is already registered." });

            var requestedRole = string.IsNullOrWhiteSpace(request.Role) ? "Participant" : request.Role.Trim();
            string role;
            if (requestedRole.Equals("Organiser", StringComparison.OrdinalIgnoreCase)) role = "Organiser";
            else if (requestedRole.Equals("Participant", StringComparison.OrdinalIgnoreCase)) role = "Participant";
            else return BadRequest(new { message = "Role must be Organiser or Participant." });

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber,
                Role = role
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful. You can now log in."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            var verification = _hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (verification == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password." });

            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
                return StatusCode(500, new { message = "Authentication is not configured." });

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                email = user.Email,
                role = user.Role,
                userId = user.UserID
            });
        }
    }

    public class RegisterRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string FirstName { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        public string LastName { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(8)]
        public string Password { get; set; } = "";

        public string? PhoneNumber { get; set; }

        // "Organiser" or "Participant" (defaults to Participant)
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = "";
    }
}