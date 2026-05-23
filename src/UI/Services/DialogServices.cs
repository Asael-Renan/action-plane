using Microsoft.Win32;

namespace FiveW2H.App.UI.Services;

public interface IFileDialogService
{
    string? ShowOpenImportDialog();
    string? ShowSaveExportDialog();
}

public interface IMessageDialogService
{
    bool Confirm(string message, string title);
    void ShowWarning(string message, string title);
}

public interface IDialogService
{
    Task<bool> ShowEditItemDialogAsync(FiveW2H.App.UI.Models.TaskModel task);
}

public class FileDialogService : IFileDialogService
{
    public string? ShowOpenImportDialog()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Importar dados",
            Filter = "Planilhas Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv|Todos os arquivos (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveExportDialog()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Exportar dados",
            Filter = "Planilhas Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv",
            DefaultExt = "xlsx",
            AddExtension = true,
            FileName = $"5w2h-records-{DateTime.Now:yyyyMMdd-HHmm}.xlsx"
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
