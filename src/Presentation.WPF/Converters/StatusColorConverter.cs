using System.Windows.Data;
using System.Windows.Media;

namespace Presentation.WPF.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var status = value?.ToString() ?? "";
        return status.ToLower() switch
        {
            "completed" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // Green
            "in progress" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // Amber
            "pending" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),       // Red
            "cancelled" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),   // Grey
            _ => new SolidColorBrush(Color.FromRgb(33, 150, 243))               // Blue
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
