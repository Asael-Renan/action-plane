using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering Infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds infrastructure services to the DI container.</summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        // Register repository
        services.AddScoped<IFiveW2HRepository>(
            _ => new FiveW2HRepository(connectionString)
        );

        // Register application services
        services.AddScoped<IFiveW2HTaskService, FiveW2HTaskService>();
        services.AddScoped<IDataImportExportService, CsvTaskDataTransferService>();
        services.AddScoped<IDataExportService, DataExportService>();

        return services;
    }
}
