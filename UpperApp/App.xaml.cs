using System.Windows;
using UpperApp.Services;

namespace UpperApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppServices.ConfigureServices();
            base.OnStartup(e);
        }
    }
}
