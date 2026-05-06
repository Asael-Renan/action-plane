using Microsoft.Win32;

namespace _5W2H.App.UI.Services;

public interface IFileDialogService
{
    string? ShowOpenCsvDialog();
    string? ShowSaveCsvDialog();
}

public interface IMessageDialogService
{
    bool Confirm(string message, string title);
    void ShowWarning(string message, string title);
}

public interface IDialogService
{
    Task<bool> ShowEditItemDialogAsync(_5W2H.App.UI.Models.TaskModel task);
}

public class FileDialogService : IFileDialogService
{
    public string? ShowOpenCsvDialog()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Importar dados",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveCsvDialog()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Exportar dados",
            Filter = "CSV files (*.csv)|*.csv",
            DefaultExt = "csv",
            AddExtension = true,
            FileName = $"5w2h-records-{DateTime.Now:yyyyMMdd-HHmm}.csv"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

public class MessageDialogService : IMessageDialogService
{
    public bool Confirm(string message, string title)
    {
        return System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
    }

    public void ShowWarning(string message, string title)
    {
        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
    }
}
