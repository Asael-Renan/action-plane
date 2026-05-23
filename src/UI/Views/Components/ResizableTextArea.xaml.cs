using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FiveW2H.App.UI.Views.Components;

public partial class ResizableTextArea : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ResizableTextArea),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty EditorHeightProperty =
        DependencyProperty.Register(
            nameof(EditorHeight),
            typeof(double),
            typeof(ResizableTextArea),
            new PropertyMetadata(120d));

    public static readonly DependencyProperty MinEditorHeightProperty =
        DependencyProperty.Register(
            nameof(MinEditorHeight),
            typeof(double),
            typeof(ResizableTextArea),
            new PropertyMetadata(110d));

    public static readonly DependencyProperty MaxEditorHeightProperty =
        DependencyProperty.Register(
            nameof(MaxEditorHeight),
            typeof(double),
            typeof(ResizableTextArea),
            new PropertyMetadata(260d));

    public ResizableTextArea()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double EditorHeight
    {
        get => (double)GetValue(EditorHeightProperty);
        set => SetValue(EditorHeightProperty, value);
    }

    public double MinEditorHeight
    {
        get => (double)GetValue(MinEditorHeightProperty);
        set => SetValue(MinEditorHeightProperty, value);
    }

    public double MaxEditorHeight
    {
        get => (double)GetValue(MaxEditorHeightProperty);
        set => SetValue(MaxEditorHeightProperty, value);
    }

    private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        var nextHeight = EditorHeight + e.VerticalChange;
        EditorHeight = Math.Clamp(nextHeight, MinEditorHeight, MaxEditorHeight);
    }
}
