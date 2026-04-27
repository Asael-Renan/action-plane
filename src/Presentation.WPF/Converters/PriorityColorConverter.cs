using System.Windows.Data;
using System.Windows.Media;

namespace Presentation.WPF.Converters;

public class PriorityColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var priority = value?.ToString() ?? "";
        return priority.ToLower() switch
        {
            "high" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),          // Red
            "medium" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),        // Orange
            "low" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),           // Green
            _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))              // Grey
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
