using Infrastructure.Database;
using Infrastructure.DependencyInjection;
using Presentation.WPF.ViewModels;
using Presentation.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.IO;

namespace Presentation.WPF;

public partial class App :  System.Windows.Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Configure database   
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "5W2H-Management",
            "data.db"
        );

        var connectionString = $"Data Source={dbPath};Version=3;";

        // Ensure database directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // Register services
        services.AddInfrastructureServices(connectionString);
        services.AddScoped<MainViewModel>();
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<MainWindow>();
        services.AddScoped<DashboardWindow>();

        ServiceProvider = services.BuildServiceProvider();

        // Initialize database
        try
        {
            var initializer = new DatabaseInitializer(connectionString);
            await initializer.InitializeAsync();
            await initializer.SeedSampleDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBoxButton.OK);
        }

        // Show main window
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Load tasks on startup
        if (mainWindow.DataContext is MainViewModel viewModel)
        {
            await viewModel.LoadTasksCommand.ExecuteAsync(null);
        }
    }
}
