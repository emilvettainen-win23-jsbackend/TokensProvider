using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokensProvider.Infrastructure.Data.Contexts;
using TokensProvider.Infrastructure.Services;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContextFactory<DataContext>(options => { 
            options.UseSqlServer(context.Configuration.GetValue<string>("SQLServer"));
        });

        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IValidateTokenService, ValidateTokenService>();
    })
    .Build();

host.Run();