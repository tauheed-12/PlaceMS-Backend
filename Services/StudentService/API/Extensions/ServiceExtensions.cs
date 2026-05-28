using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using StudentService.Application.Interfaces;
using StudentService.Application.Services;
using StudentService.Application.Validators;
using StudentService.Infrastructure.Kafka;
using StudentService.Infrastructure.Persistence;
using StudentService.Infrastructure.Repositories;
using StudentService.Infrastructure.Settings;
using StudentService.Infrastructure.Services;
using SharedKernel.Middleware;

namespace StudentService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<StudentDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("StudentDb"),
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
        services.AddValidatorsFromAssemblyContaining<UpdatePersonalInfoValidator>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStudentProfileService, StudentProfileService>();
        services.AddScoped<IResumeService, ResumeService>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        return services;
    }

    public static IServiceCollection AddMinioStorage(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection("MinioSettings").Get<MinioSettings>()!;
        services.Configure<MinioSettings>(config.GetSection("MinioSettings"));

        services.AddMinio(client => client
            .WithEndpoint(settings.Endpoint)
            .WithCredentials(settings.AccessKey, settings.SecretKey)
            .WithSSL(settings.UseSSL)
            .Build());

        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        return services;
    }

    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<KafkaSettings>(config.GetSection("KafkaSettings"));
        services.AddHostedService<UserRegisteredConsumer>();
        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MinioSettings>(config.GetSection("MinioSettings"));
        services.Configure<KafkaSettings>(config.GetSection("KafkaSettings"));
        services.Configure<ServiceUrlSettings>(config.GetSection("ServiceUrls"));
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PMS Student Service",
                Version = "v1",
                Description = "Student profile management for Placement Management System"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
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