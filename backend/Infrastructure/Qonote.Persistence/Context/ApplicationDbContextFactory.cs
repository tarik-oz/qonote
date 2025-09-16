using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Qonote.Infrastructure.Persistence.Context;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // This path logic navigates from the Persistence project directory up to the solution root,
        // and then down into the API project to find the appsettings.json.
        string apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Presentation", "Qonote.Api");

        // Get environment, defaulting to Development for design-time tools.
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        // Try to load user secrets.json from API project's UserSecretsId
        try
        {
            var csprojPath = Path.Combine(apiProjectPath, "Qonote.Api.csproj");
            if (File.Exists(csprojPath))
            {
                var doc = XDocument.Load(csprojPath);
                var secretsId = doc.Descendants("UserSecretsId").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(secretsId))
                {
                    string secretsPath;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        secretsPath = Path.Combine(appData, "Microsoft", "UserSecrets", secretsId, "secrets.json");
                    }
                    else
                    {
                        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        secretsPath = Path.Combine(home, ".microsoft", "usersecrets", secretsId, "secrets.json");
                    }

                    if (File.Exists(secretsPath))
                    {
                        configBuilder.AddJsonFile(secretsPath, optional: true, reloadOnChange: false);
                    }
                }
            }
        }
        catch
        {
            // ignore errors loading secrets at design time
        }

        IConfiguration config = configBuilder
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("QonoteDbConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

