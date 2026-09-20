[Setup]
AppId={{A2F76126-939A-469D-B42B-EE817F8B4210}
AppName=Asterion Edge Companion
AppVersion=0.3.0-dev.3
AppPublisher=Asterion Project
DefaultDirName={localappdata}\Programs\AsterionEdge
DefaultGroupName=Asterion Edge
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\release
OutputBaseFilename=AsterionEdge-Setup-v0.3.0-dev.3
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\SC-Xeneon-Companion.exe
CloseApplications=yes

[Files]
Source: "..\release\Companion\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\asterion-star-citizen-cockpit-v0.3.0-dev.3.icuewidget"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\setup.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Asterion Edge Companion"; Filename: "{app}\SC-Xeneon-Companion.exe"
Name: "{group}\Asterion Edge — Simulation"; Filename: "{app}\SC-Xeneon-Companion.exe"; Parameters: "--simulate"
Name: "{group}\Importer le widget iCUE"; Filename: "{app}\asterion-star-citizen-cockpit-v0.3.0-dev.3.icuewidget"
Name: "{group}\Désinstaller Asterion Edge"; Filename: "{uninstallexe}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "AsterionEdge"; ValueData: """{app}\SC-Xeneon-Companion.exe"" --background"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\SC-Xeneon-Companion.exe"; Parameters: "--background"; Description: "Démarrer Asterion en arrière-plan"; Flags: nowait postinstall skipifsilent
Filename: "{app}\asterion-star-citizen-cockpit-v0.3.0-dev.3.icuewidget"; Description: "Importer le widget Asterion dans iCUE"; Flags: shellexec postinstall skipifsilent
