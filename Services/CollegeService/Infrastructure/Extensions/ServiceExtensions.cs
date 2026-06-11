using CollegeService.Application.Interfaces;
using CollegeService.Application.Interfaces.Clients;
using CollegeService.Application.Interfaces.Repositories;
using CollegeService.Application.Interfaces.Services;
using CollegeService.Infrastructure.Persistence;
using CollegeService.Infrastructure.Repositories;
using CollegeService.Infrastructure.Services;
using CollegeService.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using SharedKernel.Constants;

namespace CollegeService.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection(ConfigSections.Kafka));
        services.Configure<ServiceUrlSettings>(configuration.GetSection(ConfigSections.ServiceUrls));
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CollegeDb")
            ?? configuration[$"{ConfigSections.ConnectionStrings}:CollegeDb"]
            ?? "Host=localhost;Database=college_db;Username=postgres;Password=postgres";

        services.AddDbContext<CollegeDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICollegeRepository, CollegeRepository>();
        services.AddScoped<ICollegeTpoRepository, CollegeTpoRepository>();
        services.AddScoped<IAdminCollegeScopeRepository, AdminCollegeScopeRepository>();
        services.AddScoped<ICollegeService, Application.Services.CollegeService>();
        services.AddScoped<ICollegeQueryService, Application.Services.CollegeQueryService>();
        services.AddScoped<ICollegeTpoService, Application.Services.CollegeTpoService>();
        services.AddScoped<IAdminCollegeScopeService, Application.Services.AdminCollegeScopeService>();
        services.AddScoped<IDomainEventPublisher, Infrastructure.Kafka.DomainEventPublisher>();
        services.AddSingleton<SharedKernel.Abstractions.IEventPublisher, Infrastructure.Kafka.KafkaEventPublisher>();
        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection(ConfigSections.ServiceUrls).Get<ServiceUrlSettings>()!;

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.IdentityService);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddPolicyHandler(retryPolicy);

        return services;
    }
}
