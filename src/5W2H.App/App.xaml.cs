using FiveW2H.App.Core.Services;
using FiveW2H.App.Data;
using FiveW2H.App.UI.Services;
using FiveW2H.App.UI.ViewModels;
using FiveW2H.App.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.IO;
using Velopack;

namespace FiveW2H.App;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

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
        services.AddSingleton<IAppUpdateService, VelopackAppUpdateService>();

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
            _ = viewModel.CheckForUpdatesOnStartupAsync();
        }
    }
}

