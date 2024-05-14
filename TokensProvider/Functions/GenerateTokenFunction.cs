using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TokenProvider.Services;
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

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody)!;
            //string id = data.id;
            //string email = data.email;
            //string username = data.username;

            //string token = _generateToken.Token(id, email, username);

            //if (token != null)
            //    return new OkObjectResult(token);
            //else
            //    return new BadRequestObjectResult("Failed to generate token.");

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
