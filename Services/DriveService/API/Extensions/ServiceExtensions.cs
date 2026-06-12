using DriveService.Application.Interfaces;
using DriveService.Application.Validators;
using DriveService.Infrastructure.Kafka;
using DriveService.Infrastructure.Persistence;
using DriveService.Infrastructure.Repositories;
using DriveService.Infrastructure.Services;
using DriveService.Infrastructure.Settings;
using DriveService.Infrastructure.Http;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace DriveService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceUrlSettings>(configuration.GetSection("ServiceUrls"));
        services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));
        return services;
    }

    public static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.AddSingleton<SharedKernel.Abstractions.IEventPublisher, KafkaEventPublisher>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DriveDb")
            ?? configuration["ConnectionStrings:DriveDb"]
            ?? "Host=localhost;Database=drive_db;Username=postgres;Password=postgres";

        services.AddDbContext<DriveDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDriveRepository, DriveRepository>();
        services.AddScoped<IDriveService, Application.Services.DriveService>();
        services.AddScoped<ICollegeServiceClient, CollegeServiceClient>();
        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateDriveRequestValidator>();
        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls").Get<ServiceUrlSettings>()!;

        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient<ICollegeServiceClient, CollegeServiceClient>(client =>
        {
            client.BaseAddress = new Uri(serviceUrls.CollegeService);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddHttpMessageHandler<ServiceAuthenticationHandler>()
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}
