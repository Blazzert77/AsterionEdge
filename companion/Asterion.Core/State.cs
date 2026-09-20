using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Asterion.Core;

public enum Context { UNKNOWN, ON_FOOT, FLIGHT, GROUND_VEHICLE, MINING, SALVAGE }
public record LogEvent(string Kind, string Message, Context? Context = null, string? Ship = null, string? Location = null, string? Mission = null, long? Amount = null, string? Shard = null);
public record LogRule(string Pattern, string LocalActor, Context Context, string? Ship = null);
public static class GameLog
{
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
    public Context Detected { get; private set; } = Context.UNKNOWN;
    public Context? Manual { get; private set; }
    public Context Current => Manual ?? Detected;
    public string Source => Manual.HasValue ? "MANUAL" : Detected == Context.UNKNOWN ? "UNAVAILABLE" : "LOG RULE";
    public void Apply(LogEvent e) { if (e.Context.HasValue) Detected = e.Context.Value; }
    public void Force(Context? context) => Manual = context;
    public void Reset() { Detected = Context.UNKNOWN; Manual = null; }
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
    string path = ""; long offset; DateTime created; string partial = ""; bool attached;
    readonly System.Text.Decoder decoder=System.Text.Encoding.UTF8.GetDecoder();
    FileSystemWatcher? watcher; volatile bool dirty = true;
    public void SetPath(string value)
    {
        if(path == value) return;
        watcher?.Dispose(); watcher = null; path=value; offset=0; partial=""; created=default; dirty=true; attached=false; decoder.Reset();
        if(Directory.Exists(Path.GetDirectoryName(path)))
        {
            watcher=new(Path.GetDirectoryName(path)!, Path.GetFileName(path)) { NotifyFilter=NotifyFilters.LastWrite|NotifyFilters.Size|NotifyFilters.FileName|NotifyFilters.CreationTime };
            watcher.Changed+=(_,_)=>dirty=true; watcher.Created+=(_,_)=>dirty=true; watcher.Deleted+=(_,_)=>dirty=true; watcher.Renamed+=(_,_)=>dirty=true; watcher.Error+=(_,_)=>dirty=true; watcher.EnableRaisingEvents=true;
        }
    }
    public IEnumerable<string> Read(bool reconcile = false)
    {
        if(!dirty && !reconcile) return []; dirty=false;
        if(!File.Exists(path)) { offset=0; partial=""; attached=false; decoder.Reset(); return []; }
        var info=new FileInfo(path);
        using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
        if(attached && (created != info.CreationTimeUtc || stream.Length<offset)) { offset=0; partial=""; decoder.Reset(); }
        created=info.CreationTimeUtc;
        if(!attached) { offset=stream.Length; attached=true; return []; }
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
