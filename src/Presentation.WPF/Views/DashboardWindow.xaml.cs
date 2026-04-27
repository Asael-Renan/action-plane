using System.Windows;
using Presentation.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.WPF.Views;

public partial class DashboardWindow : Window
{
    public DashboardWindow()
    {
        InitializeComponent();
        
        var serviceProvider = ((App)System.Windows.Application.Current).ServiceProvider;
        DataContext = serviceProvider.GetRequiredService<DashboardViewModel>();
    }

    private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            viewModel.LoadDashboardCommand.Execute(null);
        }
    }
}
