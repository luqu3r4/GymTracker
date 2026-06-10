using QuestPDF.Infrastructure;
using System.Windows;

namespace GymTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            base.OnStartup(e);
        }
    }
}
