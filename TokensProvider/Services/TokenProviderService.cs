using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var claims = new[]
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub, username),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            //};
            //var token = new JwtSecurityToken
            //(
            //    issuer: _issuer,
            //    audience: _audience,
            //    claims: claims,
            //    expires: DateTime.Now.AddMinutes(120),
            //    signingCredentials: creds
            //);

            //return new JwtSecurityTokenHandler().WriteToken(token);


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
