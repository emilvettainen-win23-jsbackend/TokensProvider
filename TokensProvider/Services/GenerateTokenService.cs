using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TokenProvider.Services;

public class GenerateTokenService
{
    private readonly ILogger<GenerateTokenService> _logger;
    private readonly IConfiguration _configuration;
    public GenerateTokenService(ILogger<GenerateTokenService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Token(string id, string email, string username)
    {

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.UTF8.GetBytes($"{_configuration["Token:Secret"]}");



            //var secret = _configuration["Token:Secret"];
            //if (string.IsNullOrEmpty(secret))
            //{
            //    _logger.LogError("Token secret is missing or empty.");
            //    return null!;
            //}
            //var key = Encoding.UTF8.GetBytes(secret);



            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {                      
                        new Claim("id", id),
                        new Claim("email", email),
                        new Claim(ClaimTypes.Name, username),
                }),
                Issuer = _configuration["Token:Issuer"],
                Audience = _configuration["Token:Audience"],
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:Secret"]!)), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : TokenProvider.GenerateToken.cs :: {ex.Message}");
            return null!;
        }
    }
}
