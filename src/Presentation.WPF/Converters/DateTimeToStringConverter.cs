using System.Globalization;
using System.Windows.Data;

namespace Presentation.WPF.Converters;

/// <summary>
/// Converts DateTime to a formatted string for display.
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm", culture);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (DateTime.TryParse(value?.ToString(), culture, System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }
        return DateTime.Now;
    }
}
