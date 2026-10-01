namespace Asterion.Core;

public record IndicatorDefinition(string Id,string On,string Off);
public record Indicator(bool? Active,string Source,bool Touched);

// Command estimates are not telemetry. An unknown toggle needs an observed baseline.
public sealed class CommandFeedback
{
    public static readonly IndicatorDefinition[] Definitions = [
        new("doors","OUVERTES","FERMÉES"),new("doorlocks","VERROUILLÉES","DÉVERROUILLÉES"),new("portlocks","VERROUILLÉS","DÉVERROUILLÉS"),
        new("lights","ALLUMÉS","ÉTEINTS"),new("power","SOUS TENSION","HORS TENSION"),new("engines","MARCHE","ARRÊT"),new("weapons","ACTIVES","COUPÉES"),new("shields","ACTIFS","COUPÉS"),new("coolers","ACTIFS","COUPÉS"),
        new("master","NAV","SCM"),new("gear","SORTI","RENTRÉ"),new("vtol","ACTIF","INACTIF"),new("decoupled","DÉCOUPLÉ","COUPLÉ"),new("cruise","ACTIF","INACTIF"),new("gsafe","ACTIF","INACTIF"),new("esp","ACTIF","INACTIF"),new("limiter","ACTIF","INACTIF"),new("proximity","ACTIF","INACTIF"),new("headtrack","ACTIF","INACTIF"),new("lightamp","ACTIF","INACTIF"),new("flashlight","ALLUMÉE","ÉTEINTE"),new("helmet","ÉQUIPÉ","RETIRÉ")
    ];
    readonly Dictionary<string,Indicator> values=[];
    public Indicator Get(string id)=>values.GetValueOrDefault(id)??new(null,"UNAVAILABLE",false);
    public void Calibrate(string id,bool? active)
    {
        if(!Definitions.Any(x=>x.Id==id))throw new ArgumentException("Indicateur inconnu");
        values[id]=new(active,active.HasValue?"USER":"UNAVAILABLE",false);
    }
    public void Sent(string id)
    {
        if(id=="startup"){InvalidatePower();return;}
        if(Definitions.Any(x=>x.Id==id))
        {
            var old=Get(id);bool? next=old.Active.HasValue?!old.Active.Value:null;
            if(id=="power")InvalidatePower();
            values[id]=new(next,next.HasValue?"ESTIMATED":"UNAVAILABLE",true);
        }
        var explicitState=id switch {
            "doorsOpen"=>("doors",true),"doorsClose"=>("doors",false),
            "doorlock"=>("doorlocks",true),"doorunlock"=>("doorlocks",false),
            "portsLock"=>("portlocks",true),"portsUnlock"=>("portlocks",false),
            _=>("",false)
        };
        if(explicitState.Item1!="")values[explicitState.Item1]=new(explicitState.Item2,"ESTIMATED",true);
    }
    void InvalidatePower(){foreach(var id in new[]{"power","engines","weapons","shields","coolers"})values.Remove(id);}
    public void Reset()=>values.Clear();
}
