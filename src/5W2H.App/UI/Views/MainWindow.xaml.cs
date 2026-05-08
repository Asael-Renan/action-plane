using System.Windows;
using _5W2H.App.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace _5W2H.App.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        var serviceProvider = ((_5W2H.App.App)System.Windows.Application.Current).ServiceProvider;
        DataContext = serviceProvider.GetRequiredService<MainViewModel>();

        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadTasks();
            }
        };
    }
}
