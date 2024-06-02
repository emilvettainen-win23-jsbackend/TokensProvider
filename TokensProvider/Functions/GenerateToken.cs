using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using TokensProvider.Infrastructure.Helper.Validations;
using TokensProvider.Infrastructure.Models;
using TokensProvider.Infrastructure.Services;

namespace TokensProvider.Functions;

public class GenerateToken(ILogger<GenerateToken> logger, ITokenGenerator tokenGenerator, IRefreshTokenService refreshTokenService)
{
    private readonly ILogger<GenerateToken> _logger = logger;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;


    [Function("GenerateToken")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/generate")] HttpRequest req)
    {
        try
        {
            var request = await ValidateRequestAsync(req);
            if (request == null)
                return new BadRequestObjectResult(new {Error = "Invalid request body. Parameters userId and email must be provided" });

            using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

            var refreshTokenResult = await _tokenGenerator.GenerateRefreshTokenAsync(request.UserId, cts.Token);
            if (refreshTokenResult.StatusCode != (int)HttpStatusCode.OK)
                return new ObjectResult(new { refreshTokenResult.Error}){ StatusCode = refreshTokenResult.StatusCode};

            var accessTokenResult = _tokenGenerator.GenerateAccessToken(request, refreshTokenResult.Token);
            if (accessTokenResult.StatusCode != (int)HttpStatusCode.OK)
                return new ObjectResult(new {accessTokenResult.Error}) { StatusCode = accessTokenResult.StatusCode };

            if (refreshTokenResult.Token != null && refreshTokenResult.CookieOptions != null)
                req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.CookieOptions);

            return new OkObjectResult( new {AccessToken = accessTokenResult.Token});
        }
        catch (OperationCanceledException)
        {
            return new StatusCodeResult(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateToken.Run() :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }


    private async Task <TokenRequest> ValidateRequestAsync(HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        var requestBody = await reader.ReadToEndAsync();
        var request = JsonConvert.DeserializeObject<TokenRequest>(requestBody);
        if (request == null)
        {
            return null!;
        }
        var modelState = CustomValidation.ValidateModel(request);
        return modelState.IsValid ? request : null!;
    }
}