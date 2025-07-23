using Microsoft.Extensions.Configuration;
using symbol_extractor.Models;
using System.IO;

namespace symbol_extractor.Services;

public class ConfigurationService
{
    public IConfiguration Configuration { get; }

    public ConfigurationService()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public AppSettings GetAppSettings()
    {
        return Configuration.GetSection("Settings").Get<AppSettings>();
    }
} 