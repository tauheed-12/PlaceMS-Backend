using IdentityService.API.Extensions;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharedKernel.Middleware;

// ── Bootstrap Logger (before DI builds) ─────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IdentityService...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ─────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithProperty("Service", "IdentityService")
              .WriteTo.Console(
                  outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}"));

    // ── Services ─────────────────────────────────────────────────
    builder.Services
        .AddSettings(builder.Configuration)
        .AddDatabase(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddValidation()
        .AddApplicationServices()
        .AddHttpClients(builder.Configuration)
        .AddGlobalExceptionHandling()
        .AddSwagger()
        .AddControllers();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowGateway", policy =>
            policy.WithOrigins(
                      builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // ── Build ────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Auto-migrate on startup ──────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migration applied successfully.");
    }

    // ── Middleware Pipeline (ORDER MATTERS) ───────────────────────
    app.UseGlobalExceptionHandler();   // 1. Catch everything

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS Identity Service v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseCors("AllowGateway");
    app.UseAuthentication();           // 2. Who are you?
    app.UseAuthorization();            // 3. What can you do?

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "IdentityService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}