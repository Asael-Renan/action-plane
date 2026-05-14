using _5W2H.App.Core.Services;
using _5W2H.App.Data;
using _5W2H.App.UI.Services;
using _5W2H.App.UI.ViewModels;
using _5W2H.App.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.IO;

namespace _5W2H.App;

public partial class App : Application
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

        // Register data services
        services.AddScoped<ITaskRepository>(sp => new TaskRepository(connectionString));
        services.AddScoped<AppDbContext>(sp => new AppDbContext(connectionString));

        // Register business services
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IBackupService, BackupService>();

        // Register UI services
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IFileDialogService, FileDialogService>();
        services.AddScoped<IMessageDialogService, MessageDialogService>();

        // Register ViewModels
        services.AddScoped<MainViewModel>();

        // Register Views
        services.AddScoped<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        // Initialize database
        try
        {
            var context = ServiceProvider.GetRequiredService<AppDbContext>();
            await context.InitializeAsync();
            await context.SeedSampleDataAsync();
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

