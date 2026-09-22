using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Asterion.Core;
namespace Asterion.Companion;

public sealed class Api(Engine engine)
{
    WebApplication? app;
    static bool TokenEquals(string a,string b)=>CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a),Encoding.UTF8.GetBytes(b));
    public async Task Start(CancellationToken cancel)
    {
        var builder=WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k=>{k.Listen(IPAddress.Loopback,engine.Config.Port);k.Limits.MaxRequestBodySize=16384;});
        app=builder.Build();
        app.Use(async(ctx,next)=>{
            if(ctx.Request.Host.Host!="127.0.0.1" && ctx.Request.Host.Host!="localhost"){ctx.Response.StatusCode=403;return;}
            ctx.Response.Headers.CacheControl="no-store";
            ctx.Response.Headers.XContentTypeOptions="nosniff";
            await next(ctx);
        });
        app.UseWebSockets(new(){KeepAliveInterval=TimeSpan.FromSeconds(15)});
        app.MapGet("/health",()=>Results.Json(new{protocol=2,status="ok",name="Asterion Edge",autoLink=!engine.Config.RequirePairing}));
        app.MapGet("/state",(HttpContext c)=>Authorized(c)?Results.Json(engine.Snapshot(),Config.Json):Results.Unauthorized());
        app.MapPost("/action",async(HttpContext c)=>{
            if(!Authorized(c))return Results.Unauthorized();
            try {
                var j=await JsonDocument.ParseAsync(c.Request.Body,cancellationToken:c.RequestAborted);
                string id=j.RootElement.GetProperty("id").GetString()??"";
                if(engine.Actions.Any(a=>a.Id==id&&a.Dangerous))return Results.Json(new{error="Hold confirmation requires WebSocket"},statusCode:409);
                await engine.Execute(id);return Results.Json(new{ok=true});
            }catch(Exception e) when(e is JsonException or KeyNotFoundException or ArgumentException or InvalidOperationException){return Results.Json(new{error=e.Message},statusCode:400);}
        });
        app.Map("/events",Socket);
        foreach(var file in new[]{"index.html","style.css","app.js","icon.svg"})
        {
            string captured=file;
            app.MapGet(file=="index.html"?"/":"/"+file,()=>Results.File(Path.Combine(AppContext.BaseDirectory,"widget",captured),captured.EndsWith(".css")?"text/css":captured.EndsWith(".js")?"text/javascript":captured.EndsWith(".svg")?"image/svg+xml":"text/html"));
        }
        await app.StartAsync(cancel);
        engine.Log.Write($"Local API started on 127.0.0.1:{engine.Config.Port}; auto-link={!engine.Config.RequirePairing}");
    }
    bool Authorized(HttpContext c)=>!engine.Config.RequirePairing || TokenEquals(c.Request.Headers.Authorization.ToString(),"Bearer "+engine.Config.Token);
    async Task Socket(HttpContext ctx)
    {
        if(!ctx.WebSockets.IsWebSocketRequest){ctx.Response.StatusCode=400;return;}
        using var ws=await ctx.WebSockets.AcceptWebSocketAsync();
        using var life=CancellationTokenSource.CreateLinkedTokenSource(ctx.RequestAborted);
        string client=Guid.NewGuid().ToString("N");var hold=new HoldGate();
        bool registered=false;Action? handler=null;Task? send=null;
        var queue=Channel.CreateBounded<object>(new BoundedChannelOptions(16){FullMode=BoundedChannelFullMode.DropOldest,SingleReader=true});
        try
        {
            if(engine.Config.RequirePairing)
            {
                life.CancelAfter(TimeSpan.FromSeconds(5));
                using var hello=await Receive(ws,life.Token);
                if(hello==null||hello.RootElement.GetProperty("type").GetString()!="auth"||!TokenEquals(hello.RootElement.GetProperty("token").GetString()??"",engine.Config.Token))
                {await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation,"Pairing token required",CancellationToken.None);return;}
                life.CancelAfter(Timeout.Infinite);
            }

            Interlocked.Increment(ref engine.Clients);registered=true;
            send=Task.Run(async()=>{await foreach(var item in queue.Reader.ReadAllAsync(life.Token))await ws.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item,Config.Json)),WebSocketMessageType.Text,true,life.Token);},life.Token);
            handler=()=>queue.Writer.TryWrite(new{type="state",state=engine.Snapshot()}); engine.Changed+=handler;handler();
            long last=0;
            while(ws.State==WebSocketState.Open&&!life.IsCancellationRequested)
            {
                using var document=await Receive(ws,life.Token);if(document==null)break;
                string requestId="";
                try
                {
                    var j=document.RootElement;string type=j.GetProperty("type").GetString()??"";
                    requestId=j.TryGetProperty("requestId",out var rid)?rid.GetString()??"":"";
                    string id=j.TryGetProperty("id",out var value)?value.GetString()??"":"";
                    switch(type)
                    {
                        case "auth":
                            if(engine.Config.RequirePairing&&!TokenEquals(j.TryGetProperty("token",out var tok)?tok.GetString()??"":"",engine.Config.Token))throw new InvalidOperationException("Pairing token rejected");
                            break;
                        case "holdBegin": if(!engine.Actions.Any(a=>a.Id==id&&a.Dangerous))throw new ArgumentException("Unknown guarded action");hold.Begin(client,id);break;
                        case "holdPulse": hold.Pulse(client);break;
                        case "holdCancel": hold.Cancel(client);break;
                        case "action":
                            if(Environment.TickCount64-last<200)throw new InvalidOperationException("Please wait");
                            last=Environment.TickCount64;
                            if(engine.Actions.Any(a=>a.Id==id&&a.Dangerous)&&!hold.Commit(client,id,engine.Config.HoldDuration))throw new InvalidOperationException("Hold interrupted or too short");
                            await engine.Execute(id);break;
                        case "indicator": engine.SetIndicator(id);break;
                        case "resetIndicators": engine.ResetIndicators();break;
                        case "binding": engine.SetBinding(id);break;
                        case "context": engine.Context(id);break;
                        case "accent": engine.SetAccent(id);break;
                        case "accent2": engine.SetAccent2(id);break;
                        case "background": engine.SetBackground(id);break;
                        case "panel": engine.SetPanel(id);break;
                        case "panelOpacity": engine.SetPanelOpacity(id);break;
                        case "fontScale": engine.SetFontScale(id);break;
                        case "radius": engine.SetRadius(id);break;
                        case "glow": engine.SetGlow(id);break;
                        case "quickColumns": engine.SetQuickColumns(id);break;
                        case "density": engine.SetDensity(id);break;
                        case "animations": engine.SetAnimations(id=="1"||id.Equals("true",StringComparison.OrdinalIgnoreCase));break;
                        case "theme": engine.SetTheme(id);break;
                        case "manufacturerColors": engine.SetManufacturerColors(id=="1"||id.Equals("true",StringComparison.OrdinalIgnoreCase));break;
                        case "openUpdate": engine.OpenUpdate();break;
                        case "test": engine.TestSignal();break;
                        case "ping":queue.Writer.TryWrite(new{type="pong"});break;
                        default:throw new ArgumentException("Unknown message type");
                    }
                    if(requestId!="")queue.Writer.TryWrite(new{type="ack",requestId,ok=true});
                }
                catch(Exception e) when(e is ArgumentException or InvalidOperationException or KeyNotFoundException or JsonException)
                {if(requestId!="")queue.Writer.TryWrite(new{type="ack",requestId,ok=false,error=e.Message});}
            }
        }
        catch(Exception e) when(e is WebSocketException or OperationCanceledException or JsonException or KeyNotFoundException or InvalidOperationException) {engine.Log.Write("Widget connection closed: "+e.GetType().Name);}
        finally
        {
            hold.Cancel(client);if(handler!=null)engine.Changed-=handler;
            if(registered)Interlocked.Decrement(ref engine.Clients);
            life.Cancel();queue.Writer.TryComplete();if(send!=null)try{await send;}catch{ }
        }
    }
    static async Task<JsonDocument?> Receive(WebSocket ws,CancellationToken cancel)
    {
        byte[] buffer=new byte[8192];int used=0;
        while(true)
        {
            var r=await ws.ReceiveAsync(buffer.AsMemory(used),cancel);
            if(r.MessageType==WebSocketMessageType.Close)return null;
            if(r.MessageType!=WebSocketMessageType.Text)throw new JsonException("Text only");
            used+=r.Count;if(used>=buffer.Length)throw new JsonException("Message too large");
            if(r.EndOfMessage)return JsonDocument.Parse(buffer.AsMemory(0,used));
        }
    }
    public async Task Stop(){if(app!=null)await app.StopAsync();}
}
