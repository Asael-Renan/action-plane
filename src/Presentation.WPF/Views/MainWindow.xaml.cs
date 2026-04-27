using System.Windows;
using Presentation.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.WPF.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        var serviceProvider = ((App)System.Windows.Application.Current).ServiceProvider;
        DataContext = serviceProvider.GetRequiredService<MainViewModel>();
    }
}
