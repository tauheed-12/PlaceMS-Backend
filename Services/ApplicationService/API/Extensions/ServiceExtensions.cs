using ApplicationService.Application.Interfaces;
using ApplicationService.Infrastructure.Http;
using ApplicationService.Infrastructure.Persistence;
using ApplicationService.Infrastructure.Repositories;
using ApplicationService.Infrastructure.Services;
using ApplicationService.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace ApplicationService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("ApplicationDb") ?? string.Empty,
                npgsql => npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceUrlSettings>(configuration.GetSection("ServiceUrls"));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IApplicationService, Application.Services.ApplicationService>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls").Get<ServiceUrlSettings>() ?? new ServiceUrlSettings();

        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient("IdentityServiceTokenClient", client =>
        {
            if (string.IsNullOrWhiteSpace(serviceUrls.IdentityService))
            {
                throw new InvalidOperationException(
                    "IdentityService base URL is not configured. Set ServiceUrls:IdentityService in appsettings.");
            }

            client.BaseAddress = new Uri(serviceUrls.IdentityService);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<IServiceTokenProvider>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("IdentityServiceTokenClient");
            return new ServiceTokenProvider(
                client,
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ILogger<ServiceTokenProvider>>());
        });

        services.AddHttpClient<IStudentServiceClient, StudentServiceClient>(client =>
        {
            if (string.IsNullOrWhiteSpace(serviceUrls.StudentService))
            {
                throw new InvalidOperationException(
                    "StudentService base URL is not configured. Set ServiceUrls:IdentityService in appsettings.");
            }

            client.BaseAddress = new Uri(serviceUrls.StudentService);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<ServiceAuthenticationHandler>();

        services.AddHttpClient<IDriveServiceClient, DriveServiceClient>(client =>
        {
            if (string.IsNullOrWhiteSpace(serviceUrls.DriveService))
            {
                throw new InvalidOperationException(
                    "DriveService base URL is not configured. Set ServiceUrls:IdentityService in appsettings.");
            }

            client.BaseAddress = new Uri(serviceUrls.DriveService);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<ServiceAuthenticationHandler>();

        return services;
    }
}
