using ImageCabinet.Settings;
using System.Windows;

namespace ImageCabinet
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Config.Current.Initialize(e);
            UIHelper.ThemeManager.LoadTheme(Config.Current.Theme);
            MainWindow wnd = new(Config.Current.StartupDirectory);
            wnd.Left = Config.Current.WindowPositionX;
            wnd.Top = Config.Current.WindowPositionY;
            if (!double.IsNaN(Config.Current.WindowWidth))
            {
                wnd.Width = Config.Current.WindowWidth;
            }
            if (!double.IsNaN(Config.Current.WindowHeight))
            {
                wnd.Height = Config.Current.WindowHeight;
            }
            wnd.Show();
        }
    }
}
