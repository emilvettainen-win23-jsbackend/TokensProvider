using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TokensProvider.Infrastructure.Services;

namespace TokensProvider.Functions
{
    public class ValidateToken(ILogger<ValidateToken> logger, IValidateTokenService validateTokenService)
    {
        private readonly ILogger<ValidateToken> _logger = logger;
        private readonly IValidateTokenService _validateTokenService = validateTokenService;

        [Function("ValidateToken")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/validate")] HttpRequest req)
        {
            try
            {
                string authHeader = req.Headers.Authorization!;
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer".Length).Trim();
                    var result = await _validateTokenService.ValidateToken(token);
                    if (result.IsValid)
                        return new OkResult();
                }
                return new UnauthorizedObjectResult("Invalid token");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : ValidateToken.Run() :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}