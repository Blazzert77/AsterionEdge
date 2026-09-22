namespace Asterion.Core;

// These are dashboard estimates, never game telemetry. A toggle needs a user-supplied baseline.
public sealed class CommandFeedback
{
    public static readonly string[] Toggles = ["lights","doors","engines","weapons","shields","coolers","power","gear","vtol","decoupled","cruise","gsafe","esp","limiter","proximity","headtrack","lightamp"];
    readonly Dictionary<string,bool> estimates = [];
    readonly HashSet<string> touched = [];
    readonly Dictionary<string,int> deltas = [];
    public bool? Estimate(string id) => estimates.TryGetValue(id,out var value)?value:null;
    public bool Touched(string id) => touched.Contains(id);
    public int Delta(string id) => deltas.GetValueOrDefault(id);
    public void Calibrate(string id,bool? value)
    {
        if(!Toggles.Contains(id))throw new ArgumentException("Indicateur inconnu");
        if(value.HasValue)estimates[id]=value.Value;else estimates.Remove(id);
        touched.Remove(id);
    }
    public void Sent(string id)
    {
        if(Toggles.Contains(id)){if(estimates.TryGetValue(id,out var value))estimates[id]=!value;touched.Add(id);}
        if(id is "doorsOpen" or "doorsClose"){estimates["doors"]=id=="doorsOpen";touched.Add("doors");}
        foreach(string group in new[]{"weapons","engines","shields"})
        {
            if(id==group+"Up")deltas[group]=Delta(group)+1;
            if(id==group+"Down")deltas[group]=Delta(group)-1;
            if(id==group+"Max"||id==group+"Min"||id=="balance")deltas.Remove(group);
        }
        if(id=="startup"||id=="power")Reset();
    }
    public void Reset(){estimates.Clear();touched.Clear();deltas.Clear();}
}
