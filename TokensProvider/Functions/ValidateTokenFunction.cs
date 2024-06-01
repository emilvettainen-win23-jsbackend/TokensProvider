using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TokensProvider.Services;

namespace TokensProvider.Functions;

public class ValidateTokenFunction
{
    private readonly ILogger<ValidateTokenFunction> _logger;
    
    private readonly ValidateTokenService _validateTokenService;

    public ValidateTokenFunction(ILogger<ValidateTokenFunction> logger, ValidateTokenService validateTokenService)
    {
        _logger = logger;
       
        _validateTokenService = validateTokenService;
    }

    [Function("ValidateTokenFunction")]
    public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            string authHeader = req.Headers["Authorization"]!;

            if (authHeader != null && authHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer".Length).Trim();

                var result = await _validateTokenService.ValidateToken(token);

                if (result.IsValid)
                {
                    return new OkObjectResult("Token is valid");
                }
            }

            return new UnauthorizedObjectResult("Invalid token");

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : TokensProvider.ValidateTokenFunction.cs :: {ex.Message}");
            return new StatusCodeResult(500);
        }
    }
}
