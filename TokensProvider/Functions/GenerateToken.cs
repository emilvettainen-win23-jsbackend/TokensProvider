using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TokensProvider.Infrastructure.Models;
using TokensProvider.Infrastructure.Services;

namespace TokensProvider.Functions
{
    public class GenerateToken(ILogger<GenerateToken> logger, IRefreshTokenService refreshTokenService, ITokenGenerator tokenGenerator)
    {
        private readonly ILogger<GenerateToken> _logger = logger;
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
        private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

        [Function("GenerateToken")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/generate")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);


            if (tokenRequest == null || tokenRequest.UserId == null || tokenRequest.Email == null) 
                return new BadRequestObjectResult(new { Error = "Please provide a valid userid and email address." });

            try
            {
                RefreshTokenResult refreshTokenResult = null!;
                AccessTokenResult accessTokenResult = null!;

                using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(120*1000));
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

                req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
                if (!string.IsNullOrEmpty(refreshToken))
                    refreshTokenResult = await _refreshTokenService.GetRefreshTokenAsync(refreshToken, cts.Token);

                if (refreshTokenResult == null || refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1))
                    refreshTokenResult = await _tokenGenerator.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);

                accessTokenResult = _tokenGenerator.GenerateAccessToken(tokenRequest, refreshTokenResult.Token);

                if (refreshTokenResult.Token != null && refreshTokenResult.CookieOptions != null)
                    req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.CookieOptions);               

                if (accessTokenResult != null && accessTokenResult.Token != null && refreshTokenResult.Token != null)                    
                    return new OkObjectResult(new { AccessToken = accessTokenResult.Token, refreshTokenResult.Token });           
            }
            catch { }

            return new ObjectResult(new { Error = "An unexpected error occured while generating tokens." }) { StatusCode = 500 };
        }
    }
}
