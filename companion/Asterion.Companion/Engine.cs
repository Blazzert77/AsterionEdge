using System.Diagnostics;
using System.Text.Json;
using Asterion.Core;
using Binding = Asterion.Core.Binding;
namespace Asterion.Companion;

public sealed class Engine : IDisposable
{
    public readonly Config Config;
    public readonly DebugLog Log=new();
    public readonly object Gate=new();
    public readonly List<ActionSpec> Actions;
    readonly Dictionary<string,DefaultBindingSpec> defaultBindings;
    public const string CurrentVersion = "0.3.0-dev.3";
    readonly ContextMachine machine=new();
    readonly LogTail tail=new();
    readonly SemaphoreSlim actionLock=new(1,1);
    List<Binding> bindings=[];
    readonly Queue<object> feed=[];
    public bool Running { get; private set; }
    public bool Simulation { get; private set; }
    public string? Ship { get; private set; }
    public string? Location { get; private set; }
    public string? Shard { get; private set; }
    public long? AuecBalance { get; private set; }
    public long? SessionEarnings { get; private set; }
    public long? SessionCashflow { get; private set; }
    public string? ActiveMission { get; private set; }
    public string Build { get; private set; } = "Unavailable";
    public string BindingError { get; private set; } = "";
    public UpdateState Update { get; private set; } = new(false,false,CurrentVersion,"","","");
    public int Clients;
    public event Action? Changed;
    public string BranchPath => Path.Combine(Config.StarCitizenPath,Config.Branch);
    public string LogPath => Path.Combine(BranchPath,"Game.log");
    public string Profile => FindProfile();
    string FindProfile()
    {
        if(!string.IsNullOrWhiteSpace(Config.BindingProfile) && File.Exists(Config.BindingProfile)) return Config.BindingProfile;
        string mappings=Path.Combine(BranchPath,"USER","Client","0","Controls","Mappings");
        string preferred=Path.Combine(mappings,"layout_asterion_exported.xml");
        if(File.Exists(preferred)) return preferred;
        try
        {
            if(Directory.Exists(mappings))
            {
                var exported=Directory.GetFiles(mappings,"layout_*_exported.xml").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
                if(exported!=null)return exported;
            }
        }
        catch(IOException) { } catch(UnauthorizedAccessException) { }
        return Path.Combine(BranchPath,"USER","Client","0","Profiles","default","actionmaps.xml");
    }
    public Engine(Config config)
    {
        Config=config;
        if(string.IsNullOrWhiteSpace(Config.StarCitizenPath))
        {
            var found=DetectFolders();
            if(found.Count==1){Config.StarCitizenPath=found[0];Config.Branch=new[]{"LIVE","PTU","EPTU"}.First(b=>Directory.Exists(Path.Combine(found[0],b)));Config.Save();}
        }
        Actions=JsonSerializer.Deserialize<Catalog>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Mappings","4.10.json")),Config.Json)!.Actions;
        string defaultsPath=Path.Combine(AppContext.BaseDirectory,"Mappings","4.10-default-keyboard.json");
        defaultBindings=File.Exists(defaultsPath)
            ? JsonSerializer.Deserialize<DefaultBindingCatalog>(File.ReadAllText(defaultsPath),Config.Json)?.Actions.ToDictionary(x=>x.Id,StringComparer.OrdinalIgnoreCase) ?? new(StringComparer.OrdinalIgnoreCase)
            : new(StringComparer.OrdinalIgnoreCase);
        Rescan();
    }
    public static List<string> DetectFolders()
    {
        HashSet<string> paths=new(StringComparer.OrdinalIgnoreCase);
        foreach(var drive in DriveInfo.GetDrives().Where(d=>d.IsReady && d.DriveType==DriveType.Fixed))
        foreach(string relative in new[]{"StarCitizen","Roberts Space Industries/StarCitizen","Program Files/Roberts Space Industries/StarCitizen","Games/StarCitizen","Games/Roberts Space Industries/StarCitizen"})
        {
            string p=Path.Combine(drive.RootDirectory.FullName,relative.Replace('/',Path.DirectorySeparatorChar));
            if(new[]{"LIVE","PTU","EPTU"}.Any(b=>Directory.Exists(Path.Combine(p,b)))) paths.Add(p);
        }
        return paths.ToList();
    }
    public void Rescan()
    {
        lock(Gate)
        {
            bindings=[]; BindingError="";
            try { if(File.Exists(Profile)) bindings=Bindings.Parse(File.ReadAllText(Profile)); else BindingError="Profile not found — export or select XML"; }
            catch(Exception e) when(e is IOException or System.Xml.XmlException or UnauthorizedAccessException) { BindingError=e.Message; Log.Write("Bindings unreadable"); }
            Build="Unavailable";
            try { using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(BranchPath,"build_manifest.id"))); Build=j.RootElement.GetProperty("Data").GetProperty("Version").GetString()??"Unavailable"; }catch(Exception e) when(e is IOException or JsonException or KeyNotFoundException or UnauthorizedAccessException) { }
        }
        Changed?.Invoke();
    }
    public (string Input,int Press,string Source) Resolve(ActionSpec action)
    {
        if(Config.Overrides.TryGetValue(action.Id,out var manual)&&KeyChord.TryParse(manual.Input,out _))
            return(manual.Input,Math.Clamp(manual.PressMs,30,1500),"MANUEL");

        var matches=bindings.Where(b=>string.Equals(b.Map,action.Map,StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(b.Action,action.Action,StringComparison.OrdinalIgnoreCase)).ToArray();
        var keyboard=matches.Where(b=>b.Status=="BOUND" && b.Input.StartsWith("kb1_",StringComparison.OrdinalIgnoreCase))
                            .Select(b=>b.Input).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if(keyboard.Length==1)return(keyboard[0],action.PressMs,"PROFIL JOUEUR");
        if(keyboard.Length>1)return("",0,"BINDING CLAVIER AMBIGU");

        if(defaultBindings.TryGetValue(action.Id,out var fallback)
           && string.Equals(fallback.Map,action.Map,StringComparison.OrdinalIgnoreCase)
           && string.Equals(fallback.Action,action.Action,StringComparison.OrdinalIgnoreCase)
           && KeyChord.TryParse(fallback.Input,out _))
            return(fallback.Input,action.PressMs,"DÉFAUT 4.10");

        return("",0,matches.Length>0?"AUCUN BINDING CLAVIER":"NON LIÉ");
    }
    public object Snapshot()
    {
        lock(Gate) return new {
            protocol=2, running=Running,simulation=Simulation,branch=Config.Branch,build=Build,logFound=File.Exists(LogPath),
            context=machine.Current.ToString(),contextSource=Simulation?"SIMULATION":machine.Source,ship=Ship,location=Location,shard=Shard,
            autoContext=Config.AutoContext,accent=Accent(),theme=Config.Theme,manufacturerColors=Config.ManufacturerColors,language=Config.Language,holdDuration=Config.HoldDuration,clients=Clients,
            appearance=new { theme=Config.Theme,accent=Accent(),accent2=Config.Accent2,background=Config.Background,panel=Config.Panel,panelOpacity=Config.PanelOpacity,fontScale=Config.FontScale,radius=Config.Radius,glow=Config.Glow,quickColumns=Config.QuickColumns,density=Config.Density,animations=Config.Animations,manufacturerColors=Config.ManufacturerColors },
            economy=new { balance=AuecBalance,sessionEarnings=SessionEarnings,sessionCashflow=SessionCashflow,mission=ActiveMission,source=(SessionEarnings.HasValue||SessionCashflow.HasValue)?"GAME.LOG":"UNAVAILABLE",note="Flux observé uniquement — pas un solde ni une comptabilité complète" },
            update=new { checkedRemote=Update.Checked,available=Update.Available,current=Update.CurrentVersion,latest=Update.LatestVersion,url=Update.Url,error=Update.Error },
            bindingCount=bindings.Count, defaultBindingCount=defaultBindings.Count, bindingError=BindingError,feed=feed.ToArray(),
            actions=Actions.Select(a=>{ var b=Resolve(a);return new {a.Id,a.Label,a.Page,a.Dangerous,a.Evidence,bound=b.Input!="",input=b.Input,source=b.Source,pressMs=b.Press,stateKnown=false};}).ToArray()
        };
    }
    string Accent()
    {
        if(!Config.ManufacturerColors||Ship==null)return Config.Accent;
        foreach(var p in new Dictionary<string,string>{{"RSI","#72e4d1"},{"Origin","#b0c8ff"},{"Drake","#efaa65"},{"Anvil","#accf83"},{"Aegis","#e97878"},{"Crusader","#b497ee"},{"MISC","#e0c579"},{"Argo","#ff9f56"}})if(Ship.Contains(p.Key,StringComparison.OrdinalIgnoreCase))return p.Value;
        return Config.Accent;
    }
    void Add(string text,string source)
    {
        feed.Enqueue(new { time=DateTimeOffset.Now.ToString("HH:mm:ss"),text,source }); while(feed.Count>5)feed.Dequeue();
    }
    public void Context(string? context)
    {
        lock(Gate)
        {
            machine.Force(context=="AUTO"?null:Enum.Parse<Asterion.Core.Context>(context??"UNKNOWN"));
            Add("Context: "+machine.Current,"MANUAL");
        }
        Changed?.Invoke();
    }
    public void Simulate(string scenario)
    {
        lock(Gate)
        {
            if(scenario=="STOP") { Simulation=false; machine.Reset();Ship=null;Location=null;Shard=null; Add("Simulation ended","SYSTEM"); }
            else
            {
                if(!new[]{"ON_FOOT","BOARD_SHIP","COMBAT","EXIT_SHIP"}.Contains(scenario))throw new ArgumentException("Unknown scenario");
                Simulation=true;
                bool aboard=scenario is "BOARD_SHIP" or "COMBAT";
                machine.Force(aboard?Asterion.Core.Context.FLIGHT:Asterion.Core.Context.ON_FOOT);
                Ship=aboard?"RSI Constellation Andromeda":null; Location="Area18 · simulated"; Shard="SIM-LOCAL";
                Add(scenario.Replace('_',' '),"SIMULATION");
            }
        }
        Changed?.Invoke();
    }
    public async Task Execute(string id)
    {
        if(!await actionLock.WaitAsync(0))throw new InvalidOperationException("Another command is pending");
        try
        {
            ActionSpec spec; (string Input,int Press,string Source) b; bool sim;
            lock(Gate) { spec=Actions.SingleOrDefault(a=>a.Id==id)??throw new ArgumentException("Unknown action"); b=Resolve(spec);sim=Simulation; }
            if(sim) { lock(Gate)Add("Test command: "+spec.Label,"SIMULATION · NO INPUT");Changed?.Invoke();return; }
            if(!KeyChord.TryParse(b.Input,out var chord))throw new InvalidOperationException("Binding absent or unsupported");
            if(!Running)throw new InvalidOperationException("Star Citizen is stopped");
            await WindowsInput.Send(chord,b.Press,Config.RestoreFocusFromIcue);
            lock(Gate)Add("Command sent: "+spec.Label,"INPUT · STATE UNKNOWN");
            Log.Write("Command sent: "+spec.Id);Changed?.Invoke();
        }
        finally { actionLock.Release(); }
    }
    static string RequireColor(string? value,string field)
    {
        if(!System.Text.RegularExpressions.Regex.IsMatch(value??"","^#[0-9a-fA-F]{6}$"))throw new ArgumentException($"Invalid {field} color");
        return value!.ToUpperInvariant();
    }
    static int RequireInt(string? value,int min,int max,string field)
    {
        if(!int.TryParse(value,out var parsed))throw new ArgumentException($"Invalid {field}");
        return Math.Clamp(parsed,min,max);
    }
    void SaveAppearance(Action change){lock(Gate){change();Config.Save();}Changed?.Invoke();}
    public void SetAccent(string value)=>SaveAppearance(()=>Config.Accent=RequireColor(value,"accent"));
    public void SetAccent2(string value)=>SaveAppearance(()=>Config.Accent2=RequireColor(value,"secondary accent"));
    public void SetBackground(string value)=>SaveAppearance(()=>Config.Background=RequireColor(value,"background"));
    public void SetPanel(string value)=>SaveAppearance(()=>Config.Panel=RequireColor(value,"panel"));
    public void SetPanelOpacity(string value)=>SaveAppearance(()=>Config.PanelOpacity=RequireInt(value,60,100,"panel opacity"));
    public void SetFontScale(string value)=>SaveAppearance(()=>Config.FontScale=RequireInt(value,85,120,"font scale"));
    public void SetRadius(string value)=>SaveAppearance(()=>Config.Radius=RequireInt(value,0,20,"radius"));
    public void SetGlow(string value)=>SaveAppearance(()=>Config.Glow=RequireInt(value,0,100,"glow"));
    public void SetQuickColumns(string value)=>SaveAppearance(()=>Config.QuickColumns=RequireInt(value,6,12,"quick columns"));
    public void SetDensity(string value)
    {
        value=(value??"").ToLowerInvariant();
        if(!new[]{"compact","comfortable","large"}.Contains(value))throw new ArgumentException("Invalid density");
        SaveAppearance(()=>Config.Density=value);
    }
    public void SetAnimations(bool enabled)=>SaveAppearance(()=>Config.Animations=enabled);
    public void SetTheme(string value)
    {
        value=(value??"").ToLowerInvariant();
        if(!new[]{"nebula","graphite","tactical","minimal"}.Contains(value))throw new ArgumentException("Invalid theme");
        SaveAppearance(()=>
        {
            Config.Theme=value;
            switch(value)
            {
                case "graphite": Config.Accent="#7FC8FF";Config.Accent2="#AFA7FF";Config.Background="#0C0F13";Config.Panel="#151A20";Config.PanelOpacity=96;Config.Radius=6;Config.Glow=18;break;
                case "tactical": Config.Accent="#68E0B0";Config.Accent2="#D8AA5B";Config.Background="#07100D";Config.Panel="#101B17";Config.PanelOpacity=94;Config.Radius=4;Config.Glow=22;break;
                case "minimal": Config.Accent="#A8C7E6";Config.Accent2="#7994A8";Config.Background="#0D1117";Config.Panel="#141A20";Config.PanelOpacity=100;Config.Radius=2;Config.Glow=4;break;
                default: Config.Accent="#20E0D0";Config.Accent2="#2B8CFF";Config.Background="#061019";Config.Panel="#0D1B25";Config.PanelOpacity=92;Config.Radius=8;Config.Glow=32;break;
            }
        });
    }
    public void SetManufacturerColors(bool enabled)=>SaveAppearance(()=>Config.ManufacturerColors=enabled);

    public void OpenUpdate()
    {
        string url;
        lock(Gate)url=Update.Available?Update.Url:"";
        if(string.IsNullOrWhiteSpace(url))throw new InvalidOperationException("Aucune mise à jour disponible");
        Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
    }
    public void TestSignal()
    {
        lock(Gate)Add("Windows companion received test signal","POC · NO GAME INPUT");
        Log.Write("POC signal received");Changed?.Invoke();
    }
    public async Task Monitor(CancellationToken cancel)
    {
        int tick=0; DateTime bindingTime=default; DateTime nextUpdateCheck=DateTime.MinValue;
        while(!cancel.IsCancellationRequested)
        {
            try
            {
                lock(Gate)
                {
                    var processes=Process.GetProcessesByName("StarCitizen"); bool running=processes.Length>0;
                    try
                    {
                        if(processes.Length==1)
                        {
                            string? exe=processes[0].MainModule?.FileName;
                            var branch=exe==null?null:Directory.GetParent(Path.GetDirectoryName(exe)!)?.FullName;
                            if(branch!=null && new[]{"LIVE","PTU","EPTU"}.Contains(Path.GetFileName(branch).ToUpperInvariant()))
                            {
                                string root=Directory.GetParent(branch)!.FullName;
                                if(Config.StarCitizenPath!=root||Config.Branch!=Path.GetFileName(branch))
                                {Config.StarCitizenPath=root;Config.Branch=Path.GetFileName(branch);Config.BindingProfile="";Config.Save();machine.Reset();Ship=null;Location=null;Shard=null;Rescan();}
                            }
                        }
                    }catch(System.ComponentModel.Win32Exception) { }catch(InvalidOperationException) { }
                    finally { foreach(var p in processes)p.Dispose(); }
                    if(running!=Running) { Running=running;if(!Simulation){machine.Reset();Ship=null;Location=null;Shard=null;ActiveMission=null;SessionEarnings=running?0:null;SessionCashflow=running?0:null;}Add(running?"Star Citizen démarré":"Star Citizen arrêté","PROCESS"); }
                    tail.SetPath(LogPath);
                    foreach(var line in tail.Read(++tick%10==0))
                    {
                        if(Simulation||!Running)continue;
                        var e=GameLog.Parse(line,Config.LogRules); if(e==null)continue;
                        machine.Apply(e);
                        if(e.Context.HasValue && e.Ship!=null)Ship=e.Ship;
                        if(!string.IsNullOrWhiteSpace(e.Location))Location=e.Location;
                        if(!string.IsNullOrWhiteSpace(e.Shard))Shard=e.Shard;
                        if(!string.IsNullOrWhiteSpace(e.Mission))ActiveMission=e.Mission;
                        if(e.Amount.HasValue)
                        {
                            SessionCashflow=(SessionCashflow??0)+e.Amount.Value;
                            if(e.Kind=="award")SessionEarnings=(SessionEarnings??0)+e.Amount.Value;
                        }
                        if(e.Kind is "mission_complete" or "mission_abandon")ActiveMission=null;
                        Add(e.Message,"GAME.LOG");
                    }
                    var timestamp=File.Exists(Profile)?File.GetLastWriteTimeUtc(Profile):default;
                    if(timestamp!=bindingTime){bindingTime=timestamp;Rescan();}
                }
                Changed?.Invoke();
                if(Config.CheckForUpdates && DateTime.UtcNow>=nextUpdateCheck)
                {
                    nextUpdateCheck=DateTime.UtcNow.AddHours(6);
                    var result=await UpdateChecker.CheckGitHub(Config.UpdateRepository,CurrentVersion,cancel);
                    lock(Gate)
                    {
                        bool newlyAvailable=result.Available && (!Update.Available || Update.LatestVersion!=result.LatestVersion);
                        Update=result;
                        if(newlyAvailable)Add($"Mise à jour disponible : v{result.LatestVersion}","UPDATE");
                    }
                    Changed?.Invoke();
                }
            }
            catch(Exception e) when(e is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception) {Log.Write("Monitor: "+e.GetType().Name);}
            await Task.Delay(1000,cancel);
        }
    }
    public List<Binding> AllBindings(){lock(Gate)return bindings.ToList();}
    public void Dispose(){tail.Dispose();actionLock.Dispose();}
}
