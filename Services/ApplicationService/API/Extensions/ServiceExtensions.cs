using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApplicationService.Application.Interfaces;
using ApplicationService.Infrastructure.Http;
using ApplicationService.Infrastructure.Persistence;
using ApplicationService.Infrastructure.Repositories;
using ApplicationService.Infrastructure.Services;
using ApplicationService.Infrastructure.Settings;
using ApplicationService.Infrastructure.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedKernel.Middleware;
using SharedKernel.Abstractions;
using SharedKernel.Constants;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;
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

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var secret = config["JwtSettings:SecretKey"]!;
        var issuer = config["JwtSettings:Issuer"]!;
        var audience = config["JwtSettings:Audience"]!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            """{"success":false,"message":"Authentication required.","errors":[]}""");
                    },
                    OnForbidden = ctx =>
                    {
                        ctx.Response.StatusCode = 403;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            """{"success":false,"message":"Forbidden.","errors":[]}""");
                    }
                };
            });
        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        // services.AddValidatorsFromAssemblyContaining<CreateCollegeRequestValidator>();
        // services.AddValidatorsFromAssemblyContaining<UpdateCollegeRequestValidator>();
        // services.AddValidatorsFromAssemblyContaining<CollegeFilterRequestValidator>();
        // services.AddValidatorsFromAssemblyContaining<CreateTpoRequestValidator>();
        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceUrlSettings>(configuration.GetSection("ServiceUrls"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IApplicationService, Application.Services.ApplicationService>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IStudentServiceClient, StudentServiceClient>();
        services.AddScoped<IDriveServiceClient, DriveServiceClient>();
        services.AddScoped<ServiceAuthenticationHandler>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
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

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PMS Application Service",
                Version = "v1",
                Description = "Application management for Placement Management System"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }

    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddSingleton<IWebHostEnvironmentAccessor, WebHostEnvironmentAccessor>();
        return services;
    }
}
