using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using FiveW2H.App.Core.Models;
using TaskStatusModel = FiveW2H.App.Core.Models.TaskStatus;

namespace FiveW2H.App.UI.Helpers;

public static class ComboBoxAutoComplete
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ComboBoxAutoComplete),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static readonly DependencyProperty AutoCompleteStateProperty =
        DependencyProperty.RegisterAttached(
            "AutoCompleteState",
            typeof(AutoCompleteState),
            typeof(ComboBoxAutoComplete),
            new PropertyMetadata(null));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            var state = new AutoCompleteState(comboBox);
            comboBox.SetValue(AutoCompleteStateProperty, state);
            state.Attach();
            return;
        }

        if (comboBox.GetValue(AutoCompleteStateProperty) is AutoCompleteState existingState)
        {
            existingState.Detach();
            comboBox.ClearValue(AutoCompleteStateProperty);
        }
    }

    private sealed class AutoCompleteState
    {
        private readonly ComboBox _comboBox;
        private TextBox? _editableTextBox;
        private ICollectionView? _view;
        private bool _isUpdatingText;
        private string _filterText = string.Empty;

        public AutoCompleteState(ComboBox comboBox)
        {
            _comboBox = comboBox;
        }

        public void Attach()
        {
            _comboBox.Loaded += OnLoaded;
            _comboBox.Unloaded += OnUnloaded;
            _comboBox.LostKeyboardFocus += OnComboBoxLostKeyboardFocus;
            _comboBox.IsEditable = true;
            _comboBox.IsTextSearchEnabled = false;
            _comboBox.StaysOpenOnEdit = true;

            if (_comboBox.IsLoaded)
            {
                Initialize();
            }
        }

        public void Detach()
        {
            _comboBox.Loaded -= OnLoaded;
            _comboBox.Unloaded -= OnUnloaded;
            _comboBox.LostKeyboardFocus -= OnComboBoxLostKeyboardFocus;
            _comboBox.SelectionChanged -= OnSelectionChanged;
            DetachEditableTextBox();

            if (_view is not null)
            {
                _view.Filter = null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => Initialize();

        private void OnUnloaded(object sender, RoutedEventArgs e) => DetachEditableTextBox();

        private void Initialize()
        {
            _comboBox.ApplyTemplate();
            _view = CollectionViewSource.GetDefaultView(_comboBox.ItemsSource ?? _comboBox.Items);
            _comboBox.SelectionChanged -= OnSelectionChanged;
            _comboBox.SelectionChanged += OnSelectionChanged;

            DetachEditableTextBox();
            _editableTextBox = _comboBox.Template.FindName("PART_EditableTextBox", _comboBox) as TextBox;
            if (_editableTextBox is null)
            {
                return;
            }

            _editableTextBox.TextChanged += OnEditableTextBoxTextChanged;
            _editableTextBox.GotKeyboardFocus += OnEditableTextBoxGotKeyboardFocus;
            _editableTextBox.PreviewKeyDown += OnEditableTextBoxPreviewKeyDown;
            _editableTextBox.PreviewMouseLeftButtonUp += OnEditableTextBoxPreviewMouseLeftButtonUp;
        }

        private void DetachEditableTextBox()
        {
            if (_editableTextBox is null)
            {
                return;
            }

            _editableTextBox.TextChanged -= OnEditableTextBoxTextChanged;
            _editableTextBox.GotKeyboardFocus -= OnEditableTextBoxGotKeyboardFocus;
            _editableTextBox.PreviewKeyDown -= OnEditableTextBoxPreviewKeyDown;
            _editableTextBox.PreviewMouseLeftButtonUp -= OnEditableTextBoxPreviewMouseLeftButtonUp;
            _editableTextBox = null;
        }

        private void OnEditableTextBoxGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            _comboBox.IsDropDownOpen = true;
        }

        private void OnEditableTextBoxPreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _comboBox.IsDropDownOpen = true;
        }

        private void OnEditableTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText)
            {
                return;
            }

            _filterText = _editableTextBox?.Text?.Trim() ?? string.Empty;
            ClearSelectionIfTextNoLongerMatches();
            ApplyFilter();
            _comboBox.IsDropDownOpen = _comboBox.IsKeyboardFocusWithin;
        }

        private void OnEditableTextBoxPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Down && !_comboBox.IsDropDownOpen)
            {
                _comboBox.IsDropDownOpen = true;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_editableTextBox is null || _comboBox.SelectedItem is null)
            {
                return;
            }

            var selectedText = GetItemText(_comboBox.SelectedItem);
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                return;
            }

            _isUpdatingText = true;
            _editableTextBox.Text = selectedText;
            _editableTextBox.CaretIndex = _editableTextBox.Text.Length;
            _isUpdatingText = false;
            _filterText = string.Empty;
            ResetFilter();
            _comboBox.IsDropDownOpen = false;
        }

        private void ApplyFilter()
        {
            if (_view is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_filterText))
            {
                ResetFilter();
                return;
            }

            _view.Filter = item =>
            {
                var itemText = GetItemText(item);
                return itemText.Contains(_filterText, StringComparison.CurrentCultureIgnoreCase);
            };

            _view.Refresh();
        }

        private void ResetFilter()
        {
            if (_view is null)
            {
                return;
            }

            _view.Filter = null;
            _view.Refresh();
        }

        private void ClearSelectionIfTextNoLongerMatches()
        {
            if (_comboBox.SelectedItem is null)
            {
                return;
            }

            var selectedText = GetItemText(_comboBox.SelectedItem);
            if (string.Equals(selectedText, _filterText, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _comboBox.SelectedItem = null;
        }

        private void OnComboBoxLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            _comboBox.Dispatcher.BeginInvoke(() =>
            {
                if (_comboBox.IsKeyboardFocusWithin)
                {
                    return;
                }

                _comboBox.IsDropDownOpen = false;
                ResetFilter();
            }, DispatcherPriority.Background);
        }

        private string GetItemText(object? item)
        {
            if (item is null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(_comboBox.DisplayMemberPath))
            {
                var property = TypeDescriptor.GetProperties(item)[_comboBox.DisplayMemberPath];
                if (property?.GetValue(item) is object value)
                {
                    return FormatItemText(value);
                }
            }

            return FormatItemText(item);
        }

        private static string FormatItemText(object? item)
        {
            return item switch
            {
                TaskStatusModel status => status switch
                {
                    TaskStatusModel.Completed => "Concluido",
                    TaskStatusModel.InProgress => "Em andamento",
                    TaskStatusModel.Pending => "Pendente",
                    _ => status.ToString()
                },
                Priority priority => priority switch
                {
                    Priority.Critical => "Critica",
                    Priority.High => "Alta",
                    Priority.Medium => "Media",
                    Priority.Low => "Baixa",
                    _ => priority.ToString()
                },
                _ => Convert.ToString(item, CultureInfo.CurrentCulture) ?? string.Empty
            };
        }
    }
}
