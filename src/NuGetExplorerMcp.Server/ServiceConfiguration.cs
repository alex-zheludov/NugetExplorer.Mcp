using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NuGetExplorerMcp.Application.Interfaces;
using NuGetExplorerMcp.Application.Services;
using NuGetExplorerMcp.Infrastructure.Services;

namespace NuGetExplorerMcp.Server;

/// <summary>
/// Configures dependency injection for the NuGet Explorer MCP Server.
/// Follows Dependency Inversion Principle (DIP) by registering interfaces to implementations.
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddNuGetExplorerServices(this IServiceCollection services)
    {
        // Infrastructure services
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IPackageSourceManager, PackageSourceManager>();
        services.AddSingleton<IUpdateChecker, UpdateChecker>();
        services.AddSingleton<IVulnerabilityScanner, VulnerabilityScanner>();
        services.AddSingleton<ILicenseAnalyzer, LicenseAnalyzer>();

        // Application services
        services.AddSingleton<IPackageAnalyzer, PackageAnalyzer>();

        // HttpClient for VulnerabilityScanner
        services.AddHttpClient<IVulnerabilityScanner, VulnerabilityScanner>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "NuGetExplorerMcp");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
