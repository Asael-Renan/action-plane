using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FiveW2H.App.UI.Helpers;

/// <summary>
/// Keeps the DatePicker calendar popup inside the work area and aligned with the field.
/// </summary>
public static class DatePickerPopupHelper
{
    public static readonly DependencyProperty AutoAlignPopupProperty =
        DependencyProperty.RegisterAttached(
            "AutoAlignPopup",
            typeof(bool),
            typeof(DatePickerPopupHelper),
            new PropertyMetadata(false, OnAutoAlignPopupChanged));

    public static bool GetAutoAlignPopup(DependencyObject element) =>
        (bool)element.GetValue(AutoAlignPopupProperty);

    public static void SetAutoAlignPopup(DependencyObject element, bool value) =>
        element.SetValue(AutoAlignPopupProperty, value);

    private static void OnAutoAlignPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DatePicker datePicker)
        {
            return;
        }

        datePicker.Loaded -= OnDatePickerLoaded;

        if ((bool)e.NewValue)
        {
            datePicker.Loaded += OnDatePickerLoaded;
            if (datePicker.IsLoaded)
            {
                TryAttach(datePicker);
            }
        }
    }

    private static void OnDatePickerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is DatePicker datePicker)
        {
            TryAttach(datePicker);
        }
    }

    private static void TryAttach(DatePicker datePicker)
    {
        if (!GetAutoAlignPopup(datePicker) || datePicker.Template is null)
        {
            return;
        }

        datePicker.ApplyTemplate();
        if (datePicker.Template.FindName("PART_Popup", datePicker) is not Popup popup)
        {
            return;
        }

        popup.Placement = PlacementMode.Custom;
        popup.CustomPopupPlacementCallback = (popupSize, targetSize, offset) =>
        {
            var horizontalOffset = 0d;
            var verticalOffset = targetSize.Height;
            var anchor = datePicker.PointToScreen(new Point(0, 0));
            var workArea = SystemParameters.WorkArea;

            if (anchor.X + popupSize.Width > workArea.Right)
            {
                horizontalOffset = targetSize.Width - popupSize.Width;
            }

            if (anchor.X + horizontalOffset < workArea.Left)
            {
                horizontalOffset = workArea.Left - anchor.X;
            }

            return
            [
                new CustomPopupPlacement(
                    new Point(horizontalOffset, verticalOffset),
                    PopupPrimaryAxis.Horizontal)
            ];
        };
    }
}
