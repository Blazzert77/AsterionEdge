using System.Security.Cryptography;
using System.Text.Json;
using Asterion.Core;
namespace Asterion.Companion;

public record ActionSpec(string Id,string Label,string Page,string Map,string Action,bool Dangerous,int PressMs,string Evidence);
public record Catalog(string Version,string Note,List<ActionSpec> Actions);
public record ManualBinding(string Input,int PressMs = 90);
public record DefaultBindingSpec(string Id,string Map,string Action,string Input,string Evidence);
public record DefaultBindingCatalog(string Version,string Note,List<DefaultBindingSpec> Actions);
public sealed class Config
{
    public string StarCitizenPath { get; set; } = "";
    public string Branch { get; set; } = "LIVE";
    public string BindingProfile { get; set; } = "";
    public int Port { get; set; } = 32147;
    // Kept for backwards compatibility with 0.1.x configs. v0.2 auto-links on loopback and does not require manual pairing.
    public string Token { get; set; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    public bool RequirePairing { get; set; } = false;
    public string Accent { get; set; } = "#20E0D0";
    public string Accent2 { get; set; } = "#2B8CFF";
    public string Background { get; set; } = "#061019";
    public string Panel { get; set; } = "#0D1B25";
    public int PanelOpacity { get; set; } = 92;
    public int FontScale { get; set; } = 100;
    public int Radius { get; set; } = 8;
    public int Glow { get; set; } = 32;
    public int QuickColumns { get; set; } = 10;
    public string Density { get; set; } = "comfortable";
    public bool Animations { get; set; } = true;
    public bool ManufacturerColors { get; set; } = true;
    public string Theme { get; set; } = "nebula";
    public string Language { get; set; } = "fr";
    public bool CheckForUpdates { get; set; } = true;
    public string UpdateRepository { get; set; } = "Blazzert77/AsterionEdge";
    // Optional custom feed for future self-hosted distribution. Empty = GitHub Releases.
    public string UpdateFeedUrl { get; set; } = "";
    public bool AutoContext { get; set; } = true;
    public int HoldDuration { get; set; } = 1800;
    public bool StartWithWindows { get; set; }
    public bool RestoreFocusFromIcue { get; set; } = true;
    public bool DebugLogging { get; set; }
    public Dictionary<string,ManualBinding> Overrides { get; set; } = [];
    public List<LogRule> LogRules { get; set; } = [];
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented=true, Converters={ new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    public static string DataDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AsterionEdge");
    public static string FilePath => Path.Combine(DataDir,"config.json");
    public static Config Load()
    {
        Directory.CreateDirectory(DataDir);
        var c=File.Exists(FilePath)? JsonSerializer.Deserialize<Config>(File.ReadAllText(FilePath),Json) ?? new():new Config();
        c.Port=Math.Clamp(c.Port,1024,65535); c.HoldDuration=Math.Clamp(c.HoldDuration,1500,3000);
        if(c.Token.Length<32) c.Token=Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        static bool Color(string v)=>System.Text.RegularExpressions.Regex.IsMatch(v??"","^#[0-9a-fA-F]{6}$");
        if(!Color(c.Accent))c.Accent="#20E0D0";
        if(!Color(c.Accent2))c.Accent2="#2B8CFF";
        if(!Color(c.Background))c.Background="#061019";
        if(!Color(c.Panel))c.Panel="#0D1B25";
        c.PanelOpacity=Math.Clamp(c.PanelOpacity,60,100);
        c.FontScale=Math.Clamp(c.FontScale,85,120);
        c.Radius=Math.Clamp(c.Radius,0,20);
        c.Glow=Math.Clamp(c.Glow,0,100);
        c.QuickColumns=Math.Clamp(c.QuickColumns,6,12);
        if(!new[]{"compact","comfortable","large"}.Contains(c.Density,StringComparer.OrdinalIgnoreCase))c.Density="comfortable";
        if(!new[]{"nebula","graphite","tactical","minimal"}.Contains(c.Theme,StringComparer.OrdinalIgnoreCase))c.Theme="nebula";
        if(c.Language!="fr"&&c.Language!="en")c.Language="fr";
        c.Save(); return c;
    }
    public void Save()
    {
        string tmp=FilePath+".tmp"; File.WriteAllText(tmp,JsonSerializer.Serialize(this,Json)); File.Move(tmp,FilePath,true);
    }
}
public sealed class DebugLog
{
    readonly object gate=new();
    public void Write(string message)
    {
        lock(gate) try
        {
            string file=Path.Combine(Config.DataDir,"companion.log");
            if(File.Exists(file)&&new FileInfo(file).Length>1_000_000) File.Move(file,file+".1",true);
            File.AppendAllText(file,$"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        } catch(IOException) { }
    }
}
