using Microsoft.UI.Xaml;
using Gustavsberg.Application;
using Gustavsberg.Infrastructure;
namespace Gustavsberg.WinUI;

public partial class App : Microsoft.UI.Xaml.Application
{
    private Window? window;
    private SqliteTeamRepository? repository;
    private System.Threading.Mutex? mutex;
    public App() => InitializeComponent();
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // The document repository has one writer. Avoid stale state from a second app instance.
        mutex = new System.Threading.Mutex(true,"Local\\GustavsbergBandy",out var first);
        if (!first) { Exit(); return; }
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GustavsbergBandy","team.db");
            repository = new SqliteTeamRepository(path);
            var service = new TeamService(repository);
            service.ImportFixtures(Fixtures.Load());
            window = new MainWindow(service);
            window.Closed += (_,_) => { repository.Dispose(); mutex.ReleaseMutex(); mutex.Dispose(); };
        }
        catch (Exception error)
        {
            window = new Window { Title = "Kunde inte öppna databasen", Content = new Microsoft.UI.Xaml.Controls.TextBlock { Text = error.Message, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(32) } };
        }
        window.Activate();
    }
}
