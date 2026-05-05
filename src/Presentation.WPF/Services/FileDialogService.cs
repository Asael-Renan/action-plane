using Microsoft.Win32;

namespace Presentation.WPF.Services;

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
