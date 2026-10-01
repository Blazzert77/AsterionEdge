using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Asterion.Core;

public enum Context { UNKNOWN, ON_FOOT, FLIGHT, GROUND_VEHICLE, MINING, SALVAGE }
public record LogEvent(string Kind, string Message, Context? Context = null, string? Ship = null, string? Location = null, string? Mission = null, long? Amount = null, string? Shard = null);
public record LogRule(string Pattern, string LocalActor, Context Context, string? Ship = null);
public static class GameLog
{
    // Only actual local HUD additions, not fade/replay lines or other players' vehicle activity.
    static readonly Regex ShipChannel = new(@"<SHUDEvent_OnNotification> Added notification ""Vous avez (rejoint|quitté)\s+canal '(?<ship>(?:Origin|RSI|Drake|Anvil|Aegis|Crusader|MISC|MIRAI|Argo|Gatac|Esperia|Banu|Consolidated Outland)\s+[^':\r\n]{1,80}?)\s*:\s*[^'\r\n]{1,64}'",RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(40));
    static readonly Regex DriverExit = new(@"<Vehicle Control Flow> CVehicleMovementBase::ClearDriver: Local client node \[\d+\] releasing control token for '[^']+' \[\d+\]",RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(40));
    static readonly Regex JoinPu = new(@"<Join PU>.*?\bshard\[([^\]]+)\](?:.*?\blocationId\[([^\]]*)\])?", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(40));
    static readonly Regex ShardUpdate = new(@"New Shard Id:\s*([^\s.]+)", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(40));
    static readonly Regex ContractAccepted = new("\"Contract Accepted:\\s*(.*?)\"\\s*MissionId", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(40));
    static readonly Regex Award = new(@"Awarded\s+([0-9][0-9, .]*)\s+aUEC", RegexOptions.IgnoreCase|RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(40));
    static readonly Regex TradeBuy = new(@"<CEntityComponentCommodityUIProvider::SendCommodityBuyRequest>.*?shopName\[([^\]]+)\].*?price\[([\d.]+)\]", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(40));
    static readonly Regex TradeSell = new(@"<CEntityComponentCommodityUIProvider::SendCommoditySellRequest>.*?shopName\[([^\]]+)\].*?amount\[([\d.]+)\]", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(40));

    public static LogEvent? Parse(string line, IEnumerable<LogRule>? rules = null)
    {
        if (line.Length > 16_384) return null;
        if (line.Contains("<SystemQuit>")) return new("session", "Arrêt de session détecté", Asterion.Core.Context.UNKNOWN);
        var channel=ShipChannel.Match(line);
        if(channel.Success)
        {
            bool entered=channel.Groups[1].Value=="rejoint";
            return new(entered?"ship_channel_enter":"ship_channel_leave",entered?"Canal du vaisseau rejoint · présence à bord présumée":"Canal du vaisseau quitté",entered?Asterion.Core.Context.FLIGHT:Asterion.Core.Context.ON_FOOT,Ship:channel.Groups["ship"].Value.Trim());
        }
        if(DriverExit.IsMatch(line))return new("seat_exit","Contrôle du véhicule quitté",Asterion.Core.Context.ON_FOOT);

        var join=JoinPu.Match(line);
        if(join.Success)
        {
            string shard=join.Groups[1].Value.Trim();
            string? location=join.Groups.Count>2 && join.Groups[2].Success && !string.IsNullOrWhiteSpace(join.Groups[2].Value) ? join.Groups[2].Value.Trim() : null;
            return new("session", location==null?$"Connexion PU · shard {shard}":$"Connexion PU · {location}", Location:location, Shard:shard);
        }
        if (line.Contains("<Join PU>")) return new("session", "Connexion au Persistent Universe");
        var shardUpdate=ShardUpdate.Match(line);
        if(shardUpdate.Success)
        {
            string shard=shardUpdate.Groups[1].Value.Trim();
            return new("shard", $"Shard : {shard}", Shard:shard);
        }

        if (line.Contains("CSessionManager::OnClientSpawned",StringComparison.OrdinalIgnoreCase))
            return new("session", "Personnage chargé · contexte à pied", Asterion.Core.Context.ON_FOOT);

        var accepted = ContractAccepted.Match(line);
        if (accepted.Success)
        {
            string title=accepted.Groups[1].Value.Trim();
            return new("mission_accepted", "Contrat accepté : "+title, Mission:title);
        }
        var award = Award.Match(line);
        if (award.Success)
        {
            string digits=Regex.Replace(award.Groups[1].Value,@"[^0-9]","");
            if(long.TryParse(digits,out var amount))return new("award", $"+{amount:N0} aUEC", Amount:amount);
        }

        var buy=TradeBuy.Match(line);
        if(buy.Success && decimal.TryParse(buy.Groups[2].Value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var buyTotal))
            return new("trade_buy", $"Achat · {buy.Groups[1].Value} · -{buyTotal:N0} aUEC", Amount:-(long)Math.Round(buyTotal));
        var sell=TradeSell.Match(line);
        if(sell.Success && decimal.TryParse(sell.Groups[2].Value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var sellTotal))
            return new("trade_sell", $"Vente · {sell.Groups[1].Value} · +{sellTotal:N0} aUEC", Amount:(long)Math.Round(sellTotal));

        if (line.Contains("<EndMission>") && line.Contains("CompletionType[Complete]")) return new("mission_complete", "Mission terminée");
        if (line.Contains("<EndMission>") && line.Contains("CompletionType[Abandon]")) return new("mission_abandon", "Mission abandonnée");
        foreach (var rule in rules ?? [])
        {
            if (string.IsNullOrWhiteSpace(rule.LocalActor) || !line.Contains(rule.LocalActor, StringComparison.Ordinal)) continue;
            try { if (Regex.IsMatch(line, rule.Pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(40))) return new("context", "Contexte issu d'une règle locale", rule.Context, rule.Ship); }
            catch (ArgumentException) { }
            catch (RegexMatchTimeoutException) { }
        }
        return null;
    }
}
public sealed class ContextMachine
{
    string detectedSource="UNAVAILABLE";
    public Context Detected { get; private set; } = Context.UNKNOWN;
    public Context? Manual { get; private set; }
    public Context Current => Manual ?? Detected;
    public string Source => Manual.HasValue ? "MANUAL" : Detected == Context.UNKNOWN ? "UNAVAILABLE" : detectedSource;
    public void Apply(LogEvent e) { if (e.Context.HasValue) { Detected = e.Context.Value;detectedSource=e.Kind.StartsWith("ship_channel_")?"SHIP CHANNEL":"LOG RULE"; } }
    public void Force(Context? context) => Manual = context;
    public void Reset() { Detected = Context.UNKNOWN; Manual = null;detectedSource="UNAVAILABLE"; }
}

public sealed class HoldGate(Func<long>? clock = null)
{
    readonly Func<long> now = clock ?? (() => Environment.TickCount64);
    readonly Dictionary<string, (string Action, long Start, long Pulse)> holds = [];
    public void Begin(string client, string action) => holds[client] = (action, now(), now());
    public void Pulse(string client) { if (holds.TryGetValue(client, out var h)) { if(now()-h.Pulse > 600) { holds.Remove(client); return; } holds[client] = (h.Action,h.Start,now()); } }
    public void Cancel(string client) => holds.Remove(client);
    public bool Commit(string client, string action, int duration)
    {
        if (!holds.Remove(client, out var h)) return false;
        return h.Action == action && now()-h.Start >= duration && now()-h.Pulse <= 600 && now()-h.Start <= duration+3000;
    }
}

public sealed class LogTail : IDisposable
{
    const int HeadSize = 256;
    string path = ""; long offset; string partial = ""; bool attached;
    // First bytes of the attached file. Star Citizen starts every Game.log with a timestamped header, so a different
    // head means a new session file even when Windows file-system tunnelling keeps the old creation time.
    byte[] head = [];
    // True until the file has been observed missing: only the log already present at startup is skipped;
    // a Game.log created later (new game session) is read from its first line.
    bool skipExistingHistory = true;
    readonly System.Text.Decoder decoder=System.Text.Encoding.UTF8.GetDecoder();
    FileSystemWatcher? watcher; volatile bool dirty = true;
    public void SetPath(string value)
    {
        if(path == value) return;
        watcher?.Dispose(); watcher = null; path=value; offset=0; partial=""; head=[]; dirty=true; attached=false; skipExistingHistory=true; decoder.Reset();
        string? directory=Path.GetDirectoryName(path);
        if(!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        {
            try
            {
                watcher=new(directory, Path.GetFileName(path)) { NotifyFilter=NotifyFilters.LastWrite|NotifyFilters.Size|NotifyFilters.FileName|NotifyFilters.CreationTime };
                watcher.Changed+=(_,_)=>dirty=true; watcher.Created+=(_,_)=>dirty=true; watcher.Deleted+=(_,_)=>dirty=true; watcher.Renamed+=(_,_)=>dirty=true; watcher.Error+=(_,_)=>dirty=true; watcher.EnableRaisingEvents=true;
            }
            catch(Exception e) when(e is ArgumentException or IOException or PlatformNotSupportedException) { watcher?.Dispose(); watcher=null; }
        }
    }
    void Reset() { offset=0; partial=""; head=[]; decoder.Reset(); }
    static byte[] ReadHead(FileStream stream)
    {
        stream.Seek(0,SeekOrigin.Begin);
        byte[] buffer=new byte[(int)Math.Min(HeadSize,stream.Length)]; int read=0;
        while(read<buffer.Length){int n=stream.Read(buffer,read,buffer.Length-read);if(n==0)break;read+=n;}
        return read==buffer.Length?buffer:buffer[..read];
    }
    public IEnumerable<string> Read(bool reconcile = false)
    {
        if(!dirty && !reconcile) return []; dirty=false;
        if(string.IsNullOrEmpty(path) || !File.Exists(path)) { Reset(); attached=false; if(!string.IsNullOrEmpty(path)) skipExistingHistory=false; return []; }
        using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
        if(!attached)
        {
            attached=true;
            if(skipExistingHistory) { skipExistingHistory=false; head=ReadHead(stream); offset=stream.Length; return []; }
            Reset();
        }
        else
        {
            byte[] current=ReadHead(stream);
            int common=Math.Min(current.Length,head.Length);
            bool replaced=stream.Length<offset || !current.AsSpan(0,common).SequenceEqual(head.AsSpan(0,common));
            if(replaced) Reset();
        }
        if(head.Length<HeadSize && stream.Length>head.Length) head=ReadHead(stream);
        if(stream.Length<=offset) return [];
        stream.Seek(offset,SeekOrigin.Begin);
        byte[] bytes=new byte[(int)Math.Min(262144,stream.Length-offset)]; int read=stream.Read(bytes); offset+=read;
        char[] chars=new char[System.Text.Encoding.UTF8.GetMaxCharCount(read)];int charCount=decoder.GetChars(bytes,0,read,chars,0,false);
        var text=partial+new string(chars,0,charCount); var lines=text.Split('\n'); partial=lines[^1];
        if(partial.Length>16384) partial="";
        if(stream.Length>offset) dirty=true;
        return lines[..^1].Select(x=>x.TrimEnd('\r')).ToArray();
    }
    public void Dispose()=>watcher?.Dispose();
}
