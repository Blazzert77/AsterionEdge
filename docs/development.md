# Développement

## Build Windows

Prérequis développeur : SDK .NET 10 x64. Inno Setup est requis uniquement pour produire l’installateur final. Le runtime utilisateur final est inclus grâce au publish self-contained.

Depuis PowerShell :

```powershell
./scripts/build.ps1 -Installer
```

Le script :

1. restaure les projets ;
2. exécute les tests C# ;
3. publie `SC-Xeneon-Companion.exe` pour `win-x64 --self-contained true` ;
4. empaquette le widget ;
5. génère l’installateur Inno Setup si `-Installer` est utilisé.

## Mode développeur

```powershell
dotnet run --project companion/Asterion.Companion -- --simulate
```

Le navigateur de développement peut être ouvert depuis **Ouvrir cockpit**. Depuis la V0.2, aucun fragment/token n’est nécessaire : le lien WebSocket local est automatique.

## Configuration

`%LOCALAPPDATA%\AsterionEdge\config.json`.

Principaux champs : `starCitizenPath`, `branch`, `bindingProfile`, `port`, `accent`, `manufacturerColors`, `autoContext`, `holdDuration`, `startWithWindows`, `restoreFocusFromIcue`, `debugLogging`, `overrides`, `logRules`.

`token` et `requirePairing` restent présents pour compatibilité/usage avancé mais le widget V0.2 ne demande pas de clé. Ne changez pas le port sans modifier également le widget ou reconstruire celui-ci.
