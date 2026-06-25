using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DriveService.Application.Interfaces;
using DriveService.Application.Validators;
using DriveService.Infrastructure.Kafka;
using DriveService.Infrastructure.Persistence;
using DriveService.Infrastructure.Repositories;
using DriveService.Infrastructure.Services;
using DriveService.Infrastructure.Settings;
using DriveService.Infrastructure.Http;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Polly;
using SharedKernel.Middleware;
using SharedKernel.Abstractions;

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
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
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
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddScoped<ICollegeServiceClient, CollegeServiceClient>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateDriveRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateDriveRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ApproveDriveRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RejectDriveRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RequestChangesRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<DriveListQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<AvailableDrivesQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<AdminDriveListQueryValidator>();
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

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PMS Drive Service",
                Version = "v1",
                Description = "Drive management for Placement Management System"
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
