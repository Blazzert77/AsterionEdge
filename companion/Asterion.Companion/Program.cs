namespace Asterion.Companion;
static class Program
{
    [STAThread] static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        bool portable=args.Contains("--portable");
        bool background=args.Contains("--background");
        bool simulate=args.Contains("--simulate");
        if(portable) Config.DataDir=Path.Combine(AppContext.BaseDirectory,"data");
        int index=Array.IndexOf(args,"--data");if(index>=0&&index+1<args.Length)Config.DataDir=Path.GetFullPath(args[index+1]);
        using var mutex=new Mutex(true,@"Local\\AsterionEdgeCompanion",out bool first);
        if(!first){if(!background)MessageBox.Show("Asterion Edge est déjà ouvert.");return;}
        try
        {
            var config=Config.Load();
            if(!portable)
            {
                // The installer enables autostart. Keep the UI in sync with the real Windows state.
                config.StartWithWindows=StartupManager.IsEnabled();
                config.Save();
            }
            using var engine=new Engine(config);
            Application.Run(new MainForm(engine,simulate,background,portable));
        }
        catch(Exception e){MessageBox.Show(e.Message,"Asterion Edge — démarrage impossible");}
    }
}
