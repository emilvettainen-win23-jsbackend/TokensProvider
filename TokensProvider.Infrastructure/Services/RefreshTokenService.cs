using Microsoft.EntityFrameworkCore;
using System.Net;
using TokensProvider.Infrastructure.Data.Contexts;
using TokensProvider.Infrastructure.Data.Entities;
using TokensProvider.Infrastructure.Models;

namespace TokensProvider.Infrastructure.Services;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> SaveRefreshTokenAsync(string refreshToken, string userId,  CancellationToken cancellationToken);
}

public class RefreshTokenService(IDbContextFactory<DataContext> dbContextFactory) : IRefreshTokenService
{
    private readonly IDbContextFactory<DataContext> _dbContextFactory = dbContextFactory;


    #region GetRefreshTokenAsync
    public async Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        RefreshTokenResult refreshTokenResult = null!;

        try
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var refreshTokenEntity = await context.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.ExpiryDate > DateTime.Now, cancellationToken);
            if (refreshTokenEntity != null)
            {
                refreshTokenResult = new RefreshTokenResult()
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Token = refreshTokenEntity.RefreshToken,
                    ExpiryDate = refreshTokenEntity.ExpiryDate
                };
            }
            else
            {
                refreshTokenResult = new RefreshTokenResult()
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Error = "Refresh token not found or expired"
                };
            }

            return refreshTokenResult;
        }
        catch (Exception ex)
        {
            refreshTokenResult = new RefreshTokenResult()
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = ex.Message
            };
        }

        return refreshTokenResult;
    }

    #endregion

    #region SaveRefreshTokenAsync
    public async Task<bool> SaveRefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var tokenLifetime = double.TryParse(Environment.GetEnvironmentVariable("TOKEN_REFRESHTOKEN_LIFETIME"), out double refreshTokenLifeTime) ? refreshTokenLifeTime : 7;

            await using var context = _dbContextFactory.CreateDbContext();
            var refreshTokenEntity = new RefreshTokenEntity()
            {
                RefreshToken = refreshToken,
                UserId = userId,
                ExpiryDate = DateTime.Now.AddDays(tokenLifetime)
            };

            context.RefreshTokens.Add(refreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
