using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Kafka.Consumers;
using NotificationService.Infrastructure.Kafka.Handlers;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services.Email;
using NotificationService.Infrastructure.Services.InApp;
using NotificationService.Infrastructure.Settings;
using SharedKernel.Middleware;

namespace NotificationService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("NotificationDb"),
                npgsql => npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationAppService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IInAppNotificationService, SignalRInAppService>();
        services.AddSingleton<ITemplateEngine, TemplateEngine>();
        return services;
    }

    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        // Register all handlers — consumer discovers them via DI
        services.AddScoped<INotificationEventHandler, EmailVerificationHandler>();
        services.AddScoped<INotificationEventHandler, PasswordResetHandler>();
        services.AddScoped<INotificationEventHandler, UserDeactivatedHandler>();
        services.AddScoped<INotificationEventHandler, ApplicationStatusChangedHandler>();
        services.AddScoped<INotificationEventHandler, DriveApprovalRequestedHandler>();
        services.AddScoped<INotificationEventHandler, DriveApprovedHandler>();
        services.AddScoped<INotificationEventHandler, DriveRejectedHandler>();
        services.AddScoped<INotificationEventHandler, DriveChangesRequestedHandler>();
        services.AddScoped<INotificationEventHandler, DriveResubmittedHandler>();
        services.AddScoped<INotificationEventHandler, DriveDeactivatedHandler>();
        services.AddHostedService<NotificationKafkaConsumer>();
        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
        services.Configure<KafkaSettings>(config.GetSection("KafkaSettings"));
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "PMS Notification Service", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }, Array.Empty<string>()
            }});
        });
        return services;
    }

    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddSingleton<IWebHostEnvironmentAccessor, WebHostEnvironmentAccessor>();
        return services;
    }
}