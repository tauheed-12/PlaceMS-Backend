using NotificationService.API.Extensions;
using NotificationService.API.Hubs;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharedKernel.Middleware;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithProperty("Service", "NotificationService")
              .WriteTo.Console());

    builder.Services
        .AddSettings(builder.Configuration)
        .AddDatabase(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddApplicationServices()
        .AddKafkaConsumer()
        .AddSignalR()
        .AddGlobalExceptionHandling()
        .AddSwagger()
        .AddControllers();

    builder.Services.AddCors(options =>
        options.AddPolicy("AllowGateway", policy =>
            policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
                  .AllowAnyHeader().AllowAnyMethod().AllowCredentials())); // AllowCredentials needed for SignalR

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await db.Database.MigrateAsync();
    }

    app.UseGlobalExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS Notification Service v1"); c.RoutePrefix = string.Empty; });
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowGateway");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");   // SignalR endpoint

    await app.RunAsync();
}
catch (Exception ex) { Log.Fatal(ex, "NotificationService failed to start."); }
finally { Log.CloseAndFlush(); }