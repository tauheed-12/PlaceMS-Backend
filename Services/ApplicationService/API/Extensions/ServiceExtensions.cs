using ApplicationService.Application.Interfaces;
using ApplicationService.Application.Services;
using ApplicationService.Infrastructure.Persistence;
using ApplicationService.Infrastructure.Repositories;
using ApplicationService.Infrastructure.Services;
using ApplicationService.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IApplicationService, ApplicationService.Application.Services.ApplicationService>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls").Get<ServiceUrlSettings>() ?? new ServiceUrlSettings();

        if (!string.IsNullOrWhiteSpace(serviceUrls.StudentService))
        {
            services.AddHttpClient<IStudentServiceClient, StudentServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceUrls.StudentService);
            });
        }

        if (!string.IsNullOrWhiteSpace(serviceUrls.DriveService))
        {
            services.AddHttpClient<IDriveServiceClient, DriveServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceUrls.DriveService);
            });
        }

        return services;
    }
}
