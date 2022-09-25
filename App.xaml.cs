using System.Windows;

namespace ImageCabinet
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Config.Current.Initialize(e);
            MainWindow wnd = new(Config.Current.StartupDirectory);
            wnd.Show();
        }
    }
}
