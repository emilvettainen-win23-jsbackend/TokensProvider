using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokensProvider.Configurations;
using TokensProvider.Infrastructure.Data.Contexts;
using TokensProvider.Infrastructure.Services;
using TokensProvider.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        //IConfiguration configuration = context.Configuration;

        //string tokenSecret = configuration["Secret"]!;
        //string tokenIssuer = configuration["Issuer"]!;
        //string tokenAudience = configuration["Audience"]!;

        //services.AddSingleton(new TokenProviderService(tokenSecret, tokenIssuer, tokenAudience));
        //services.AddSingleton(new ValidateTokenService(tokenSecret, tokenIssuer, tokenAudience));
        //?


        services.AddDbContextFactory<DataContext>(options =>
        {
            options.UseSqlServer(Environment.GetEnvironmentVariable("SQLServer"));
        });

        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();



        services.RegisterJwt(context.Configuration);
        services.AddAuthorization();
        services.AddAuthentication();
    })
    .Build();

host.Run();
