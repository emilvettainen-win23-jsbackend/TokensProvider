using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TokensProvider.Services;

namespace TokensProvider.Functions;

public class GenerateTokenFunction
{
    private readonly ILogger<GenerateTokenFunction> _logger;
   
    private readonly TokenProviderService _tokenProviderService;

    public GenerateTokenFunction(ILogger<GenerateTokenFunction> logger, TokenProviderService tokenProviderService)
    {
        _logger = logger;
        
        _tokenProviderService = tokenProviderService;
    }

    [Function("GenerateTokenFunction")]
    public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("GenerateTokenFunction processed a request.");

            var request = await new StreamReader(req.Body).ReadToEndAsync();
            if (request == null)
            {
                return new BadRequestResult();
            }

            var token = _tokenProviderService.GenerateToken(request);
            return new OkObjectResult(token);

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : TokensProvider.GenerateTokenFunction.cs :: {ex.Message}");
            return new StatusCodeResult(500);
        }
    }
}
