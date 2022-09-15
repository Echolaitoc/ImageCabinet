using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ImageCabinet.UIHelper
{
    public class UIHelper
    {
        private static readonly DependencyProperty OffToggledContentProperty = DependencyProperty.RegisterAttached("OffToggledContent", typeof(object), typeof(UIHelper));
        public static void SetOffToggledContent(UIElement element, object value)
        {
            element.SetValue(OffToggledContentProperty, value);
        }
        public static object GetOffToggledContent(UIElement element)
        {
            return element.GetValue(OffToggledContentProperty);
        }

        public static readonly DependencyProperty ToggledContentProperty = DependencyProperty.RegisterAttached("ToggledContent", typeof(object), typeof(UIHelper), new PropertyMetadata()
        {
            PropertyChangedCallback = OnToggledContentChanged
        });
        public static void SetToggledContent(UIElement element, object value)
        {
            element.SetValue(ToggledContentProperty, value);
        }
        public static object GetToggledContent(UIElement element)
        {
            return element.GetValue(ToggledContentProperty);
        }

        private static void OnToggledContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ToggleButton toggleButton)) return;
            if (GetOffToggledContent(toggleButton) == null)
            {
                SetOffToggledContent(toggleButton, toggleButton.Content);
            }
            toggleButton.Checked -= ToggleButton_Checked;
            toggleButton.Unchecked -= ToggleButton_Unchecked;
            if (e.NewValue != null)
            {
                toggleButton.Checked += ToggleButton_Checked;
                toggleButton.Unchecked += ToggleButton_Unchecked;
            }
        }

        private static void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggleButton)) return;
            var offContent = GetOffToggledContent(toggleButton);
            if (offContent != null)
            {
                toggleButton.Content = offContent;
            }
        }

        private static void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggleButton)) return;
            var toggledContent = GetToggledContent(toggleButton);
            if (toggledContent != null)
            {
                toggleButton.Content = toggledContent;
            }
        }
    }
}
