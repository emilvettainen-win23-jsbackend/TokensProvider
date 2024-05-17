using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using TokenProvider.Services;
using TokensProvider.Configurations;
using TokensProvider.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        //services.AddSingleton<GenerateTokenService>();

        //services.AddSingleton(new TokenProviderService("YjQwOWZmYmItY2I4YS00YjMxLTg0MDYtOWM5NDEzYmUzZDM0", "TokenProvider", "Silicon"));
        //services.AddSingleton(new ValidateTokenService("YjQwOWZmYmItY2I4YS00YjMxLTg0MDYtOWM5NDEzYmUzZDM0", "TokenProvider", "Silicon"));

        IConfiguration configuration = context.Configuration;

        string tokenSecret = configuration["Secret"]!;
        string tokenIssuer = configuration["Issuer"]!;
        string tokenAudience = configuration["Audience"]!;

        services.AddSingleton(new TokenProviderService(tokenSecret, tokenIssuer, tokenAudience));
        services.AddSingleton(new ValidateTokenService(tokenSecret, tokenIssuer, tokenAudience));

        services.RegisterJwt(context.Configuration);
        services.AddAuthorization();
        services.AddAuthentication();
    })
    .Build();

host.Run();
