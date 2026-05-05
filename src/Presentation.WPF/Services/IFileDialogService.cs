namespace Presentation.WPF.Services;

public interface IFileDialogService
{
    string? ShowOpenCsvDialog();

    string? ShowSaveCsvDialog();
}
