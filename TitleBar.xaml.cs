using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageCabinet
{
    public partial class TitleBar : UserControl
    {
        public static readonly DependencyProperty ButtonNeighborContentProperty = DependencyProperty.Register("ButtonNeighborContent", typeof(object), typeof(TitleBar), new FrameworkPropertyMetadata());
        public object ButtonNeighborContent
        {
            get { return GetValue(ButtonNeighborContentProperty); }
            set { SetValue(ButtonNeighborContentProperty, value); }
        }

        public TitleBar()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
        }

        private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            e.CanExecute = window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            e.CanExecute = window.ResizeMode != ResizeMode.NoResize;
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            SystemCommands.MaximizeWindow(window);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            SystemCommands.MinimizeWindow(window);
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            SystemCommands.RestoreWindow(window);
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            window.Close();
        }
    }
}
