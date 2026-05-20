using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FiveW2H.App.UI.Converters;

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
        return priority.ToLowerInvariant() switch
        {
            "critical" => new SolidColorBrush(Color.FromRgb(220, 38, 38)),
            "high" => new SolidColorBrush(Color.FromRgb(249, 115, 22)),
            "medium" => new SolidColorBrush(Color.FromRgb(234, 179, 8)),
            "low" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
            _ => new SolidColorBrush(Color.FromRgb(34, 197, 94))
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
        return status.ToLowerInvariant() switch
        {
            "completed" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
            "inprogress" or "in progress" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            "pending" => new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            "onhold" or "on hold" => new SolidColorBrush(Color.FromRgb(251, 146, 60)),
            "cancelled" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            _ => new SolidColorBrush(Color.FromRgb(148, 163, 184))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Completed" => "Concluido",
            "InProgress" => "Em Andamento",
            "Pending" => "Pendente",
            "OnHold" => "Em Espera",
            "Cancelled" => "Cancelado",
            var status => status ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PriorityTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Critical" => "Critica",
            "High" => "Alta",
            "Medium" => "Media",
            "Low" => "Baixa",
            var priority => priority ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Displays TaskStatus and Priority enum values in Portuguese for combo boxes.
/// </summary>
public class EnumDisplayConverter : IValueConverter
{
    private static readonly StatusTextConverter StatusText = new();
    private static readonly PriorityTextConverter PriorityText = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            FiveW2H.App.Core.Models.TaskStatus status => StatusText.Convert(status, targetType, parameter, culture),
            FiveW2H.App.Core.Models.Priority priority => PriorityText.Convert(priority, targetType, parameter, culture),
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TaskCategoryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Geral";
        }

        var firstSentence = text.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstSentence))
        {
            return "Geral";
        }

        return firstSentence.Length <= 24 ? firstSentence : firstSentence[..24];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
