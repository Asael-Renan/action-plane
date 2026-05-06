using System.Windows;
using _5W2H.App.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace _5W2H.App.UI.Views;

public partial class DashboardWindow : Window
{
    public DashboardWindow()
    {
        InitializeComponent();
        
        var serviceProvider = ((_5W2H.App.App)System.Windows.Application.Current).ServiceProvider;
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
