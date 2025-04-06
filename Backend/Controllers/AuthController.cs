using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SqlDefinitionStorageExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string ValidUsername = "jimkeecn";
        private const string ValidPassword = "jimkeecn";
        private const string SecretKey = "telerik-designer-web-super-key-123!";

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model.Username != ValidUsername || model.Password != ValidPassword)
                return Unauthorized("Invalid credentials.");

            var token = GenerateToken(model.Username);
            return Ok(new { token });
        }

        private string GenerateToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "ReportEditor") // you can add roles if needed
            };

            var token = new JwtSecurityToken(
                issuer: "telerik-designer-web",
                audience: "telerik-designer-web",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
