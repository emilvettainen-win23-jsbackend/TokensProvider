using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokensProvider.Infrastructure.Services;

public interface IValidateTokenService
{
    Task<TokenValidationResult> ValidateToken(string token);
}

public class ValidateTokenService : IValidateTokenService
{

    private readonly IConfiguration _configuration;

    public ValidateTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<TokenValidationResult> ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationsParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["TokenKey"]!)),

                ValidateIssuer = true,
                ValidIssuer = _configuration["TokenIssuer"],

                ValidateAudience = true,
                ValidAudience = _configuration["TokenAudience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)

            };

            return await tokenHandler.ValidateTokenAsync(token, validationsParameters);
        }
        catch (Exception)
        {

            return null!;
        }
    }
}
