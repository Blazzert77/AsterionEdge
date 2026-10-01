using Asterion.Companion;
using Asterion.Core;
using System.Text.Json;

// Exercises the actual Companion resolver, without starting monitoring or sending Windows input.
string temp=Path.Combine(Path.GetTempPath(),"asterion-controls-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temp);Config.DataDir=temp;
string profile=Path.Combine(temp,"profile.xml");File.WriteAllText(profile,"<ActionMaps/>");
var config=new Config{StarCitizenPath=temp,BindingProfile=profile,CheckForUpdates=false};
using var engine=new Engine(config);
int passed=0,failed=0;
void Check(bool v,string message="Assertion failed"){if(!v)throw new Exception(message);}
void Test(string name,Action action){try{action();passed++;Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
(string Input,int Press,string Source) Resolve(string id)=>engine.Resolve(engine.Actions.Single(a=>a.Id==id));
void Profile(string xml){File.WriteAllText(profile,xml);engine.Rescan();}
Test("Installed defaults: destruct passes 0.5s activation; SCM/NAV uses long B",()=>{
 var d=Resolve("destruct");Check(d.Input=="kb1_backspace"&&d.Press>=650);
 var m=Resolve("master");Check(m.Input=="kb1_b"&&m.Press>=400);Check(engine.Actions.Single(a=>a.Id=="master").Action=="v_master_mode_cycle_long");
});
Test("Point allocation: taps, modifier-first decreases, holds and reset",()=>{
 foreach(var (id,key,scan) in new[]{("weapons","f5",63),("engines","f6",64),("shields","f7",65)}){
  var up=Resolve(id+"Up");Check(up.Input=="kb1_"+key&&up.Press<250);
  var down=Resolve(id+"Down");Check(down.Press<250&&KeyChord.TryParse(down.Input,out var c)&&c.ScanCodes.SequenceEqual(new ushort[]{56,(ushort)scan}));
  Check(Resolve(id+"Max").Press>250&&Resolve(id+"Min").Press>250);
 }
 Check(Resolve("balance").Input=="kb1_f8");foreach(var id in new[]{"pw","pt","ps"})Check(Resolve(id).Press>250);
 Check(Resolve("coolersUp").Input==""&&Resolve("coolersDown").Input=="");
});
Test("Startup and thrusters use actual map and default",()=>{
 Check(Resolve("startup").Input=="kb1_ralt+r");Check(Resolve("engines").Input=="kb1_i");
 Check(engine.Actions.Single(a=>a.Id=="engines").Action=="v_power_toggle_thrusters");
});
Test("Game-format f6+lalt sends modifier first, case-insensitive mouse retained",()=>{
 Check(KeyChord.TryParse("kb1_f6+lalt",out var c)&&c.ScanCodes.SequenceEqual(new ushort[]{56,64}));
 Check(KeyChord.TryParse("KB1_MOUSE4",out c)&&c.MouseButton==1);
});
Test("Unlock shortcut cannot be resolved as opening; doors need their own binding",()=>{
 Profile("<ActionMaps><actionmap name='spaceship_general'><action name='v_unlock_all_doors'><rebind input='kb1_mouse4'/></action></actionmap></ActionMaps>");
 Check(Resolve("doorunlock").Input=="kb1_mouse4");foreach(var id in new[]{"doors","doorsOpen","doorsClose"})Check(Resolve(id).Input=="");
 Profile("<ActionMaps><actionmap name='spaceship_general'><action name='v_toggle_all_doors'><rebind input='kb1_rctrl+o'/></action><action name='v_open_all_doors'><rebind input='kb1_rctrl+i'/></action></actionmap></ActionMaps>");
 Check(Resolve("doors").Input=="kb1_rctrl+o"&&Resolve("doorsOpen").Input=="kb1_rctrl+i"&&Resolve("doorsClose").Input=="");
});
Test("Custom master-mode, allocation and self-destruct bindings take precedence",()=>{
 Profile("<ActionMaps><actionmap name='spaceship_movement'><action name='v_master_mode_cycle_long'><rebind input='kb1_m'/></action></actionmap><actionmap name='spaceship_power'><action name='v_engineering_assignment_engine_decrease'><rebind input='kb1_f9+lalt'/></action></actionmap><actionmap name='spaceship_general'><action name='v_self_destruct'><rebind input='kb1_rctrl+backspace'/></action></actionmap></ActionMaps>");
 Check(Resolve("master").Input=="kb1_m"&&Resolve("master").Press>=400);
 Check(Resolve("enginesDown").Input=="kb1_f9+lalt");Check(Resolve("destruct").Input=="kb1_rctrl+backspace"&&Resolve("destruct").Press>=650);
});
Test("Explicit keyboard unbind and unsupported activation never fall back",()=>{
 foreach(string attributes in new[]{"input=''","input='kb1_ '","input='kb1_f7' multiTap='2'"}){
  Profile("<ActionMaps><actionmap name='spaceship_power'><action name='v_engineering_assignment_shields_increase'><rebind "+attributes+"/></action></actionmap></ActionMaps>");Check(Resolve("shieldsUp").Input=="");
 }
 Profile("<ActionMaps><actionmap name='spaceship_power'><action name='v_engineering_assignment_shields_increase'><rebind input='js1_ '/></action></actionmap></ActionMaps>");Check(Resolve("shieldsUp").Input=="kb1_f7");
});
Test("Old manual durations cannot turn holds into taps, or point taps into MAX",()=>{
 config.Overrides["destruct"]=new("kb1_backspace",90);Check(Resolve("destruct").Press>=650);
 config.Overrides["master"]=new("kb1_b",90);Check(Resolve("master").Press>=400);
 config.Overrides["weaponsUp"]=new("kb1_f5",900);Check(Resolve("weaponsUp").Press<=150);
 config.Overrides.Clear();
});
Test("Schema migration preserves old unlocking override without reusing it for opening",()=>{
 File.WriteAllText(Config.FilePath,"{\"overrides\":{\"doors\":{\"input\":\"kb1_mouse4\",\"pressMs\":90}}}");var c=Config.Load();
 Check(c.CommandSchema==2&&!c.Overrides.ContainsKey("doors")&&c.Overrides["doorunlock"].Input=="kb1_mouse4");
 c.Overrides["doors"]=new("kb1_rctrl+o");c.Save();c=Config.Load();Check(c.Overrides["doors"].Input=="kb1_rctrl+o");
});
Test("Stable null-config and SemVer protections remain intact",()=>{
 File.WriteAllText(Config.FilePath,"{\"overrides\":null,\"logRules\":null,\"theme\":null}");var c=Config.Load();Check(c.Overrides!=null&&c.LogRules!=null&&c.Theme=="nebula");
 Check(UpdateChecker.IsNewer("0.3.2","0.3.1"));Check(UpdateChecker.IsNewer("0.3.0","0.3.0-design.2"));Check(!UpdateChecker.IsNewer("0.3.0-design.2","0.3.0"));
});
Test("Catalog durations are internally consistent",()=>{foreach(var a in engine.Actions)Check(a.MinimumPressMs>=30&&a.MinimumPressMs<=a.PressMs&&a.PressMs<=a.MaximumPressMs&&a.MaximumPressMs<=1500,a.Id);});
Test("Game mouse-device profile and port default resolve independently",()=>{
 Profile("<ActionMaps><actionmap name='spaceship_general'><action name='v_toggle_all_doors'><rebind input='mo1_mouse5'/></action><action name='v_toggle_all_doorlocks'><rebind input='mo1_mouse4'/></action></actionmap></ActionMaps>");
 Check(Resolve("doors").Input=="mo1_mouse5");Check(Resolve("doorlocks").Input=="mo1_mouse4");Check(Resolve("portlocks").Input=="kb1_ralt+k");
});
JsonElement Snapshot()=>JsonSerializer.SerializeToElement(engine.Snapshot(),Config.Json);
Test("Session context restores boarding history without replaying rewards",()=>{
 string dir=Path.GetDirectoryName(engine.LogPath)!;Directory.CreateDirectory(dir);
 File.WriteAllText(engine.LogPath,"[CSessionManager::OnClientSpawned] Spawned!\n<SHUDEvent_OnNotification> Added notification \"Vous avez rejoint canal 'RSI Perseus : Tester'.\n<SHUDEvent_OnNotification> Awarded 100 aUEC\n");
 engine.Context("AUTO");engine.RestoreContextFromLog();var snap=Snapshot();Check(snap.GetProperty("context").GetString()=="FLIGHT");Check(snap.GetProperty("contextSource").GetString()=="SHIP CHANNEL");Check(engine.SessionEarnings==null);
 engine.ApplyContextEvent(new("seat_exit","",Context.ON_FOOT));Check(Snapshot().GetProperty("context").GetString()=="FLIGHT");
 engine.ApplyContextEvent(new("ship_channel_leave","",Context.ON_FOOT,Ship:"Origin M80"));Check(Snapshot().GetProperty("context").GetString()=="FLIGHT");
 engine.ApplyContextEvent(new("ship_channel_leave","",Context.ON_FOOT,Ship:"RSI Perseus"));Check(Snapshot().GetProperty("context").GetString()=="ON_FOOT"&&engine.Ship==null);
});
Test("Feedback belongs to Companion, survives snapshots, and failed commands cannot light it",()=>{
 engine.SetIndicator("{\"id\":\"doors\",\"value\":false}");
 try{engine.Execute("doors").GetAwaiter().GetResult();throw new Exception("Non-simulation input should be rejected without game running");}catch(InvalidOperationException){}
 var door=Snapshot().GetProperty("actions").EnumerateArray().Single(a=>a.GetProperty("id").GetString()=="doors");Check(!door.GetProperty("indicator").GetProperty("active").GetBoolean());
 engine.Simulate("BOARD_SHIP");engine.SetIndicator("{\"id\":\"doors\",\"value\":false}");engine.Execute("doors").GetAwaiter().GetResult();engine.Execute("ping").GetAwaiter().GetResult();
 door=Snapshot().GetProperty("actions").EnumerateArray().Single(a=>a.GetProperty("id").GetString()=="doors");Check(door.GetProperty("indicator").GetProperty("active").GetBoolean());Check(!door.GetProperty("stateKnown").GetBoolean());
 engine.Simulate("STOP");door=Snapshot().GetProperty("actions").EnumerateArray().Single(a=>a.GetProperty("id").GetString()=="doors");Check(door.GetProperty("indicator").GetProperty("active").ValueKind==JsonValueKind.Null);
});
Console.WriteLine($"{passed} passed, {failed} failed");return failed==0?0:1;
