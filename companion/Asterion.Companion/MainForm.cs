using System.Diagnostics;
using System.Text.Json;
using Asterion.Core;
namespace Asterion.Companion;

public sealed class MainForm : Form
{
    readonly Engine engine; readonly Api api;readonly CancellationTokenSource life=new();
    readonly bool portable;
    readonly Label status=new(){AutoSize=false,Dock=DockStyle.Top,Height=175,Padding=new(16),Font=new("Consolas",11)};
    readonly System.Windows.Forms.Timer timer=new(){Interval=1000};
    readonly TextBox path=new(){Width=500};readonly ComboBox branch=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=100};
    readonly Label message=new(){AutoSize=true,MaximumSize=new(730,0)};
    readonly NotifyIcon tray;
    bool exitRequested;

    public MainForm(Engine e,bool simulate,bool background,bool portableMode)
    {
        engine=e;api=new(e);portable=portableMode;
        Text="ASTERION / EDGE — Companion 0.3";Size=new(840,720);MinimumSize=new(800,650);
        BackColor=Color.FromArgb(15,23,29);ForeColor=Color.FromArgb(223,233,239);Font=new("Segoe UI",10);

        var menu=new ContextMenuStrip();
        menu.Items.Add("Ouvrir Asterion",null,(_,_)=>ShowFromTray());
        menu.Items.Add("Réanalyser Star Citizen",null,(_,_)=>engine.Rescan());
        menu.Items.Add("Ouvrir les logs",null,(_,_)=>Open(Config.DataDir));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quitter",null,(_,_)=>{exitRequested=true;Close();});
        tray=new NotifyIcon(){Text="Asterion Edge Companion",Icon=SystemIcons.Application,ContextMenuStrip=menu,Visible=true};
        tray.DoubleClick+=(_,_)=>ShowFromTray();

        var body=new FlowLayoutPanel(){Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new(16)};
        Controls.Add(body);Controls.Add(status);
        body.Controls.Add(new Label(){Text="AUTO-DÉTECTION STAR CITIZEN · normalement aucune action n’est nécessaire",AutoSize=true});
        path.Text=e.Config.StarCitizenPath;branch.Items.AddRange(["LIVE","PTU","EPTU"]);branch.SelectedItem=e.Config.Branch;
        var row=new FlowLayoutPanel(){AutoSize=true};row.Controls.Add(path);row.Controls.Add(branch);body.Controls.Add(row);
        AddButtons(body,("Détecter les dossiers",Detect),("Parcourir…",Browse),("Appliquer",Apply));
        AddButtons(body,("Importer profil XML…",Import),("Réanalyser bindings",()=>e.Rescan()),("Configurer commandes…",BindingsEditor));
        AddButtons(body,("Ouvrir cockpit",()=>Open($"http://127.0.0.1:{e.Config.Port}/")),("Tester la liaison locale",()=>e.TestSignal()),("Ouvrir logs",()=>Open(Config.DataDir)));
        AddButtons(body,("Ouvrir config",()=>Open(Config.FilePath)),("Actualiser maintenant",()=>e.Rescan()));

        var startup=new CheckBox(){Text="Démarrer automatiquement avec Windows",AutoSize=true,Checked=!portable&&StartupManager.IsEnabled(),Enabled=!portable};
        startup.CheckedChanged+=(_,_)=>{
            if(portable)return;
            StartupManager.Set(startup.Checked);e.Config.StartWithWindows=startup.Checked;e.Config.Save();
            message.Text=startup.Checked?"Asterion démarrera désormais automatiquement et restera dans la zone de notification.":"Démarrage automatique désactivé.";
        };body.Controls.Add(startup);
        var auto=new CheckBox(){Text="Changement automatique de page selon le contexte disponible",AutoSize=true,Checked=e.Config.AutoContext};auto.CheckedChanged+=(_,_)=>{e.Config.AutoContext=auto.Checked;e.Config.Save();};body.Controls.Add(auto);
        var colors=new CheckBox(){Text="Accent selon le constructeur (si connu)",AutoSize=true,Checked=e.Config.ManufacturerColors};colors.CheckedChanged+=(_,_)=>e.SetManufacturerColors(colors.Checked);body.Controls.Add(colors);
        AddButtons(body,("Couleur personnalisée",()=>{using var d=new ColorDialog();if(d.ShowDialog()==DialogResult.OK){e.SetAccent(ColorTranslator.ToHtml(d.Color));}}));
        AddButtons(body,("Thème Obsidienne",()=>e.SetTheme("obsidian")),("Thème Graphite",()=>e.SetTheme("graphite")),("Thème Nuit bleue",()=>e.SetTheme("navy")));
        body.Controls.Add(new Label(){Text="SIMULATEUR · aucune commande clavier n’est envoyée",AutoSize=true,ForeColor=Color.FromArgb(242,192,110)});
        AddButtons(body,("À pied",()=>e.Simulate("ON_FOOT")),("Embarquer",()=>e.Simulate("BOARD_SHIP")),("Combat",()=>e.Simulate("COMBAT")),("Débarquer",()=>e.Simulate("EXIT_SHIP")),("Fin simulation",()=>e.Simulate("STOP")));
        body.Controls.Add(message);

        timer.Tick+=(_,_)=>RefreshStatus();
        Shown+=async(_,_)=>{
            try
            {
                await api.Start(life.Token);
                _=Task.Run(()=>engine.Monitor(life.Token));
                timer.Start();
                if(simulate)e.Simulate("ON_FOOT");
                RefreshStatus();
                message.Text="Liaison automatique active. Vous pouvez fermer cette fenêtre : Asterion continuera dans la zone de notification.";
                if(background)BeginInvoke((Action)HideToTray);
            }
            catch(Exception ex){message.Text="API indisponible : "+ex.Message;}
        };
        FormClosing+=OnClosing;
        FormClosed+=(_,_)=>{timer.Stop();life.Cancel();_ = api.Stop();tray.Visible=false;tray.Dispose();};
        Resize+=(_,_)=>{if(WindowState==FormWindowState.Minimized)HideToTray();};
    }

    void OnClosing(object? sender,FormClosingEventArgs e)
    {
        if(!exitRequested && e.CloseReason==CloseReason.UserClosing)
        {
            e.Cancel=true;HideToTray();
            tray.ShowBalloonTip(1200,"Asterion Edge","Asterion reste actif en arrière-plan et se connectera automatiquement au XENEON EDGE.",ToolTipIcon.Info);
        }
    }
    void HideToTray(){Hide();ShowInTaskbar=false;}
    void ShowFromTray(){ShowInTaskbar=true;Show();WindowState=FormWindowState.Normal;Activate();RefreshStatus();}

    void RefreshStatus()
    {
        using var j=JsonDocument.Parse(JsonSerializer.Serialize(engine.Snapshot(),Config.Json));var s=j.RootElement;
        status.Text=$"STAR CITIZEN  {(engine.Running?"RUNNING":"STOPPED")}     BUILD {engine.Config.Branch} / {engine.Build}\nGAME.LOG      {(s.GetProperty("logFound").GetBoolean()?"FOUND":"NOT FOUND")}\nXENEON EDGE   {(engine.Clients>0?"CONNECTED":"WAITING — AUTO LINK")}\nCONTEXT       {s.GetProperty("context")} / {s.GetProperty("contextSource")}\nSHIP          {engine.Ship??"Unavailable"}\nBINDINGS      {s.GetProperty("bindingCount")} XML / {s.GetProperty("actions").EnumerateArray().Count(a=>!a.GetProperty("bound").GetBoolean())} commandes sans binding";
        if(path.Text!=engine.Config.StarCitizenPath && !path.Focused)path.Text=engine.Config.StarCitizenPath;
        if(branch.SelectedItem?.ToString()!=engine.Config.Branch && !branch.Focused)branch.SelectedItem=engine.Config.Branch;
    }
    static void Open(string target){Process.Start(new ProcessStartInfo(target){UseShellExecute=true});}
    static void AddButtons(Control parent,params (string Text,Action Run)[] buttons)
    {var row=new FlowLayoutPanel(){AutoSize=true,MaximumSize=new(770,0)};foreach(var b in buttons){var button=new Button(){Text=b.Text,AutoSize=true,Height=36,FlatStyle=FlatStyle.Flat,Padding=new(6)};button.Click+=(_,_)=>{try{b.Run();}catch(Exception e){MessageBox.Show(e.Message);}};row.Controls.Add(button);}parent.Controls.Add(row);}
    void Detect(){var found=Engine.DetectFolders();if(found.Count==1)path.Text=found[0];else if(found.Count>1){using var f=new Form(){Text="Choisir l’installation",Size=new(700,300)};var list=new ListBox(){Dock=DockStyle.Fill};list.Items.AddRange(found.Cast<object>().ToArray());list.DoubleClick+=(_,_)=>{path.Text=list.SelectedItem?.ToString()??"";f.Close();};f.Controls.Add(list);f.ShowDialog();}else message.Text="Aucun dossier standard trouvé. Lancez Star Citizen : Asterion essaiera aussi de détecter son chemin depuis le processus.";}
    void Browse(){using var d=new FolderBrowserDialog(){Description="Dossier StarCitizen contenant LIVE / PTU / EPTU"};if(d.ShowDialog()==DialogResult.OK)path.Text=d.SelectedPath;}
    void Apply(){if(!Directory.Exists(Path.Combine(path.Text,branch.Text)))throw new InvalidOperationException("La branche sélectionnée n’existe pas dans ce dossier.");lock(engine.Gate){engine.Config.StarCitizenPath=path.Text;engine.Config.Branch=branch.Text;engine.Config.BindingProfile="";engine.Config.Save();}engine.Context("AUTO");engine.Rescan();}
    void Import(){using var d=new OpenFileDialog(){Filter="Star Citizen XML|*.xml"};if(d.ShowDialog()==DialogResult.OK){Bindings.Parse(File.ReadAllText(d.FileName));lock(engine.Gate){engine.Config.BindingProfile=d.FileName;engine.Config.Save();}engine.Rescan();}}
    void BindingsEditor()
    {
        using var form=new Form(){Text="Commandes · saisie explicite ou XML détecté",Size=new(1000,650)};
        var grid=new DataGridView(){Dock=DockStyle.Fill,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White};
        grid.Columns.Add("id","Action");grid.Columns[0].ReadOnly=true;grid.Columns.Add("xml","Binding XML / effectif");grid.Columns[1].ReadOnly=true;grid.Columns.Add("manual","Override (ex. kb1_lalt+n)");grid.Columns.Add("duration","Durée ms (30–1500)");
        foreach(var a in engine.Actions){engine.Config.Overrides.TryGetValue(a.Id,out var b);grid.Rows.Add(a.Id+" · "+a.Label,engine.Resolve(a).Input,b?.Input??"",b?.PressMs??90);}
        var help=new TextBox(){Dock=DockStyle.Top,Height=88,Multiline=true,ReadOnly=true,Text="Asterion recherche automatiquement un export layout_asterion_exported.xml puis le profil actionmaps.xml. Les noms de touches sont des positions physiques QWERTY (scancodes). Vérifiez AZERTY en jeu.\r\nExport optionnel : console du jeu → pp_rebindkeys export all asterion.\r\nUne case vide conserve le binding XML. Les actions complexes/double-tap doivent être reconfigurées en jeu."};
        var save=new Button(){Text="Enregistrer les overrides locaux",Dock=DockStyle.Bottom,Height=42};save.Click+=(_,_)=>{
            try{var edits=new Dictionary<string,ManualBinding>();for(int i=0;i<grid.Rows.Count;i++){var r=grid.Rows[i];string input=r.Cells[2].Value?.ToString()?.Trim()??"";if(input=="")continue;if(!KeyChord.TryParse(input,out _)||!int.TryParse(r.Cells[3].Value?.ToString(),out int duration)||duration<30||duration>1500)throw new InvalidOperationException("Binding ou durée invalide : "+engine.Actions[i].Label);edits[engine.Actions[i].Id]=new(input,duration);}lock(engine.Gate){engine.Config.Overrides=edits;engine.Config.Save();}form.Close();}catch(Exception ex){MessageBox.Show(ex.Message);}};
        form.Controls.Add(grid);form.Controls.Add(help);form.Controls.Add(save);form.ShowDialog();engine.Rescan();
    }
}
