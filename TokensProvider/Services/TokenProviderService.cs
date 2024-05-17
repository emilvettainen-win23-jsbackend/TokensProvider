using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TokensProvider.Services;

public class TokenProviderService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenProviderService(string secret, string issuer, string audience)
    {
        _secret = secret;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(string username)
    {
        try
        {

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
              {
                        new Claim("id", Guid.NewGuid().ToString()),
                        new Claim("email", username),
                        new Claim(ClaimTypes.Name, username),
              }),
                Issuer = _issuer,
                Audience =_audience,
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
        catch (Exception)
        {
            throw;
        }
    }
}
