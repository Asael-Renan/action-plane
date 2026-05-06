using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace _5W2H.App.UI.Converters;

/// <summary>
/// Converts boolean values to Visibility for WPF binding.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

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

public class WidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double width)
        {
            return width - 60; // Subtract padding
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
