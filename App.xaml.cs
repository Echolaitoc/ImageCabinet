using System.Windows;

namespace ImageCabinet
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = e.Args.Length > 0 ? new(e.Args[0]) : new();
            wnd.Show();
        }
    }
}
