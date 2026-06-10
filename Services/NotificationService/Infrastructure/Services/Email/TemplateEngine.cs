using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Services.Email;

/// <summary>
/// Simple token-replacement template engine.
/// Templates are .html files in Application/Templates/.
/// Tokens: {{Name}}, {{DriveTitle}}, {{ActionUrl}} etc.
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly string _templatesPath;

    // In-memory cache — templates are static files, load once
    private static readonly Dictionary<string, string> _cache = new();
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public TemplateEngine(ILogger<TemplateEngine> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _templatesPath = Path.Combine(env.ContentRootPath, "Application", "Templates");
    }

    public string Render(string templateName, Dictionary<string, string> data)
    {
        var template = GetTemplate(templateName);

        foreach (var (key, value) in data)
            template = template.Replace($"{{{{{key}}}}}", value);

        return template;
    }

    private string GetTemplate(string templateName)
    {
        if (_cache.TryGetValue(templateName, out var cached))
            return cached;

        var filePath = Path.Combine(_templatesPath, $"{templateName}.html");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Email template '{templateName}' not found at {filePath}");

        var content = File.ReadAllText(filePath);
        _cache[templateName] = content;

        return content;
    }
}