﻿using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TokensProvider.Infrastructure.Models;

namespace TokensProvider.Infrastructure.Services;

public interface ITokenGenerator
{
    AccessTokenResult GenerateAccessToken(TokenRequest tokenRequest, string? refreshToken);
    Task<RefreshTokenResult> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken);
}

public class TokenGenerator(IRefreshTokenService refreshTokenService, IConfiguration configuration) : ITokenGenerator
{
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IConfiguration _configuration = configuration;

    #region GenerateRefreshTokenAsync
    public async Task<RefreshTokenResult> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.BadRequest, Error = "Invalid body request. No userId was found." };

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));
            if (token == null)
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occured while token was generated." };

            var cookieOptions = CookieGenerator.GenerateCookie(DateTimeOffset.Now.AddDays(7));
            if (cookieOptions == null)
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occured while cookie was generated." };

            var result = await _refreshTokenService.SaveRefreshTokenAsync(token, userId, cancellationToken);
            if (!result)
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occured while saving refresh token." };

            return new RefreshTokenResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                Token = token,
                CookieOptions = cookieOptions
            };
        }
        catch (Exception ex)
        {
            return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = ex.Message };
        }
    }

    #endregion

    #region GenerateAccessToken

    public AccessTokenResult GenerateAccessToken(TokenRequest tokenRequest, string? refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(tokenRequest.UserId) || string.IsNullOrEmpty(tokenRequest.Email))
                return new AccessTokenResult { StatusCode = (int)HttpStatusCode.BadRequest, Error = "Invalid request body. Parameters userId and email must be provided." };

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tokenRequest.UserId),
                new Claim(ClaimTypes.Name, tokenRequest.Email),
                new Claim(ClaimTypes.Email, tokenRequest.Email)
            };

            if (!string.IsNullOrEmpty(refreshToken))
                claims = [.. claims, new Claim("refreshToken", refreshToken)];

            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));
            if (token == null)
                return new AccessTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An unexpected error occured while token was generated." };

            return new AccessTokenResult { StatusCode = (int)HttpStatusCode.OK, Token = token };
        }
        catch (Exception ex)
        {
            return new AccessTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = ex.Message };
        }
    }

    #endregion

    #region GenerateJwtToken
    public static string GenerateJwtToken(ClaimsIdentity claims, DateTime expires)
    {

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = expires,
            Issuer = Environment.GetEnvironmentVariable("TokenIssuer"),
            Audience = Environment.GetEnvironmentVariable("TokenAudience"),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TokenKey")!)), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion
}
