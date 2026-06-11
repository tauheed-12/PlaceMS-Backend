using Microsoft.EntityFrameworkCore;
using Serilog;
using CollegeService.API.Extensions;
using CollegeService.Infrastructure.Persistence;
using SharedKernel.Middleware;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CollegeService...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithProperty("Service", "CollegeService")
              .WriteTo.Console(
                  outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}"));

    // ── Services ─────────────────────────────────────────────────
    builder.Services
        .AddSettings(builder.Configuration)
        .AddDatabase(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddValidation()
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
        var db = scope.ServiceProvider.GetRequiredService<CollegeDbContext>();
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
    Log.Fatal(ex, "CollegeService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}