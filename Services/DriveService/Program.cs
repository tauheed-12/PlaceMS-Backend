using Microsoft.EntityFrameworkCore;
using Serilog;
using DriveService.API.Extensions;
using DriveService.Infrastructure.Persistence;
using SharedKernel.Middleware;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting DriveService...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithProperty("Service", "DriveService")
              .WriteTo.Console(
                  outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}"));

    // ── Services ─────────────────────────────────────────────────
    builder.Services
        .AddSettings(builder.Configuration)
        .AddKafka()
        .AddDatabase(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddValidation()
        .AddHttpClients(builder.Configuration)
        .AddApplicationServices()
        .AddGlobalExceptionHandling()
        .AddSwagger()
        .AddControllers();

    builder.Services.AddCors(options =>
    options.AddPolicy("AllowGateway", policy =>
        policy.WithOrigins(
                  builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()));

    // ── Build ────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Auto-migrate ─────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DriveDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migration applied.");
    }

    // ── Middleware Pipeline ───────────────────────────────────────
    app.UseGlobalExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS Student Service v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseCors("AllowGateway");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DriveService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}