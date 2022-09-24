using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ImageCabinet
{
    public class TagButton : ButtonBase
    {
        public enum FilterMode
        {
            None,
            Include,
            Exclude,
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(TagButton), new PropertyMetadata());
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register("ToolTipText", typeof(string), typeof(TagButton), new PropertyMetadata());
        public string ToolTipText
        {
            get { return (string)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        private static readonly DependencyPropertyKey CurrentFilterModePropertyKey = DependencyProperty.RegisterReadOnly("CurrentFilterMode", typeof(FilterMode), typeof(TagButton), new PropertyMetadata(FilterMode.None));
        public static readonly DependencyProperty CurrentFilterModeProperty = CurrentFilterModePropertyKey.DependencyProperty;
        public FilterMode CurrentFilterMode
        {
            get { return (FilterMode)GetValue(CurrentFilterModeProperty); }
            private set { SetValue(CurrentFilterModePropertyKey, value); }
        }

        public TagButton()
        {
            Click += TagButton_Click;
        }

        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            var modes = Enum.GetValues(typeof(FilterMode)).Cast<FilterMode>().ToList();
            int currentIndex = modes.IndexOf(CurrentFilterMode);
            currentIndex = ++currentIndex % modes.Count;
            CurrentFilterMode = modes[currentIndex];
        }
    }
}
