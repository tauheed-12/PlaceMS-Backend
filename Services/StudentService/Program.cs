using Microsoft.EntityFrameworkCore;
using Serilog;
using StudentService.API.Extensions;
using StudentService.Infrastructure.Persistence;
using SharedKernel.Middleware;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting StudentService...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithProperty("Service", "StudentService")
              .WriteTo.Console(
                  outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}"));

    // ── Services ─────────────────────────────────────────────────
    builder.Services
        .AddSettings(builder.Configuration)
        .AddDatabase(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddValidation()
        .AddApplicationServices()
        .AddMinioStorage(builder.Configuration)
        .AddKafkaConsumers(builder.Configuration)
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
        var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
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

    app.UseSerilogRequestLogging(opts =>
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms");

    app.UseCors("AllowGateway");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "StudentService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}