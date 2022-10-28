using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ImageCabinet.UIHelper
{
    public class UIHelper
    {
        #region toggle content
        private static readonly DependencyProperty OffToggledContentProperty = DependencyProperty.RegisterAttached("OffToggledContent", typeof(object), typeof(UIHelper));
        public static void SetOffToggledContent(ToggleButton toggleButton, object value) { toggleButton.SetValue(OffToggledContentProperty, value); }
        public static object GetOffToggledContent(ToggleButton toggleButton) { return toggleButton.GetValue(OffToggledContentProperty); }

        public static readonly DependencyProperty ToggledContentProperty = DependencyProperty.RegisterAttached("ToggledContent", typeof(object), typeof(UIHelper), new PropertyMetadata()
        {
            PropertyChangedCallback = OnToggledContentChanged
        });
        public static void SetToggledContent(ToggleButton toggleButton, object value) { toggleButton.SetValue(ToggledContentProperty, value); }
        public static object GetToggledContent(ToggleButton toggleButton) { return toggleButton.GetValue(ToggledContentProperty); }

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
        #endregion toggle content

        #region popup helper
        public static readonly DependencyProperty OpenPopupOnClickProperty = DependencyProperty.RegisterAttached("OpenPopupOnClick", typeof(Popup), typeof(UIHelper), new PropertyMetadata()
        {
            PropertyChangedCallback = OpenPopupOnClickChanged
        });
        public static void SetOpenPopupOnClick(ButtonBase button, Popup value) { button.SetValue(OpenPopupOnClickProperty, value); }
        public static Popup GetOpenPopupOnClick(ButtonBase button) { return (Popup)button.GetValue(OpenPopupOnClickProperty); }

        private static void OpenPopupOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ButtonBase button)) return;

            button.Click -= OpenPopupOnClick_Click;
            if (e.NewValue is Popup popup)
            {
                button.Click += OpenPopupOnClick_Click;
            }
        }

        private static void OpenPopupOnClick_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is ButtonBase button) || !(GetOpenPopupOnClick(button) is Popup popup)) return;
            popup.IsOpen = true;
        }
        #endregion popup helper

        #region corner radius
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(UIHelper), new PropertyMetadata());
        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) { obj.SetValue(CornerRadiusProperty, value); }
        public static CornerRadius GetCornerRadius(DependencyObject obj) { return (CornerRadius)obj.GetValue(CornerRadiusProperty); }
        #endregion corner radius

        public static Uri MakePackUri(string relativeFile)
        {
            System.Reflection.Assembly a = typeof(UIHelper).Assembly;
            string assemblyShortName = a.ToString().Split(',')[0];
            string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
            return new Uri(uriString);
        }

        public static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            if (parent == null) return null;
            T? child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                else
                {
                    break;
                }
            }
            return child;
        }

        public static bool TryParseDouble(object? value, out double doubleValue)
        {
            var doubleString = value?.ToString();
            if (doubleString == null)
            {
                doubleValue = 0;
                return false;
            }
            if (doubleString.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
            {
                return double.TryParse(doubleString, NumberStyles.Any, CultureInfo.CurrentCulture, out doubleValue);
            }
            return double.TryParse(doubleString, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue);
        }

        public static void HandleGenericException([System.Runtime.CompilerServices.CallerMemberName] string member = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            HandleGenericException(string.Empty, member, filePath);
        }

        public static void HandleGenericException(Exception exception, [System.Runtime.CompilerServices.CallerMemberName] string member = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            HandleGenericException(exception.Message, member, filePath);
        }

        public static void HandleGenericException(string? message, [System.Runtime.CompilerServices.CallerMemberName] string member = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            System.Diagnostics.Trace.WriteLine("[" + (string.IsNullOrEmpty(fileName) ? "" : fileName + ":") + member + "] Caught exception" + (string.IsNullOrEmpty(message) ? "" : ": " + message));
        }
    }
}
