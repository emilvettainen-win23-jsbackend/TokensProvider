using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace TokensProvider.Services;

public class ValidateTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
	private readonly string _audience;

    public ValidateTokenService(string secret, string issuer, string audience)
    {
        _secret = secret;
        _issuer = issuer;
        _audience = audience;
    }

    public async Task<TokenValidationResult> ValidateToken(string token)
    {
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),

				ValidateIssuer = true,
				ValidIssuer = _issuer,

				ValidateAudience = true,
				ValidAudience = _audience,

				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromSeconds(5)
			};
			
			return await tokenHandler.ValidateTokenAsync(token, validationParameters);

		}
		catch (Exception)
		{
            throw;
		}
    }
}
