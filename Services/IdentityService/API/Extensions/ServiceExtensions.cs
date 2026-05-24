using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Application.Validators;
using IdentityService.Infrastructure.Kafka;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using IdentityService.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using SharedKernel.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Middleware;

namespace IdentityService.API.Extensions;

public static class ServiceExtensions
{
    // ── Database ────────────────────────────────────────────────────
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("IdentityDb"),
                npgsql => npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null)));

        return services;
    }

    // ── JWT Authentication ─────────────────────────────────────────
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection(ConfigSections.Jwt).Get<JwtSettings>()!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero  // No tolerance — tokens expire exactly on time
            };

            // Return 401 JSON instead of redirect
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
                        """{"success":false,"message":"You do not have permission to access this resource.","errors":[]}""");
                }
            };
        });

        return services;
    }

    // ── FluentValidation ──────────────────────────────────────────
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }

    // ── Application Services ──────────────────────────────────────
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        return services;
    }

    // ── HTTP Clients with Polly ────────────────────────────────────
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration config)
    {
        var serviceUrls = config.GetSection(ConfigSections.ServiceUrls).Get<ServiceUrlSettings>()!;

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
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }

    // ── Settings ──────────────────────────────────────────────────
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection(ConfigSections.Jwt));
        services.Configure<KafkaSettings>(config.GetSection(ConfigSections.Kafka));
        services.Configure<ServiceUrlSettings>(config.GetSection(ConfigSections.ServiceUrls));
        return services;
    }

    // ── Swagger ───────────────────────────────────────────────────
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PMS Identity Service",
                Version = "v1",
                Description = "Authentication and user management for Placement Management System"
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

    // ── Global Exception Handler ──────────────────────────────────
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddSingleton<IWebHostEnvironmentAccessor, WebHostEnvironmentAccessor>();
        return services;
    }
}