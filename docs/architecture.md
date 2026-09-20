# Architecture

```text
XENEON EDGE / iCUE QtWebEngine
  widget/index.html + style.css + app.js
          | automatic WebSocket, loopback only
          v
C# companion / Kestrel + WinForms
  API → action allowlist → HoldGate → foreground check → Windows SendInput
  Engine → process metadata, build_manifest.id, LogTail, XML profiles
  ContextMachine → detected source / manual override / simulation
```

## Répartition

- `Asterion.Core` : XML avec DTD désactivée, scancodes explicitement reconnus, tail borné du journal, événements et state machine, minuterie monotone de maintien. Testable sans jeu ni interface Windows.
- `Asterion.Companion` : configuration, auto-détection bornée des dossiers, sélection de branche, envoi clavier Windows, serveur local et petite UI. Aucune dépendance NuGet tierce.
- `widget` : pages, boutons tactiles, sous-pages, confirmation visuelle, traitement du protocole, reconnexion exponentielle 0,8–10 s. Assets locaux originaux ; pas de police ou script distant.
- `Mappings/4.10.json` : catalogue versionné d’actions. Les identifiants sont des candidats et n’impliquent aucun binding par défaut. Seuls les couples exacts actionmap/action trouvés dans le XML peuvent être résolus automatiquement ; sinon saisie explicite.
- `shared/protocol.md` : contrat v1. Les mises à jour futures de mapping seront séparées de l’exécutable. Aucun auto-update réseau en V1.

## Vérité des données

Trois notions indépendantes : **commande disponible**, **contexte choisi/détecté**, **état réel du système**. La V1 ne connaît pas l’état de puissance, boucliers, train ou armement : chaque action annonce `stateKnown:false`. Une entrée Windows réussie ne prouve pas que le jeu l’a acceptée. Le flux indique donc « Command sent », jamais « Gear down » ou « Engines on ».

Un seul contexte est effectif : override manuel s’il existe, sinon contexte détecté. Le redémarrage ou arrêt du jeu réinitialise les informations. L’identité et le lieu sont nullable. En simulation, aucun SendInput n’est appelé, même si le jeu tourne.

## Journal

FileSystemWatcher déclenche le tail. Réconciliation toutes les dix secondes ; mise à jour d’état/processus chaque seconde. Lecture bornée à 256 KiB par passage, conservation des lignes partielles et du décodeur UTF-8, partage ReadWrite/Delete. Historique ignoré à la première ouverture ; troncature et changement de date de création réinitialisent le curseur. Une rotation dont le fichier de remplacement conserve exactement la date de création et une taille supérieure à l’ancien offset n’est pas détectable par cette méthode ; il faut alors redémarrer/réattacher. Aucun polling de mémoire ou hook.

## Adaptateurs de contexte

Le tableau `logRules` est vide par défaut. Exemple de STRUCTURE, pas une signature Star Citizen réelle :

```json
{"pattern":"^EXAMPLE_ONLY SeatEntered.*actor=MY_LOCAL_ID", "localActor":"actor=MY_LOCAL_ID", "context":"FLIGHT", "ship":null}
```

Pour ajouter une règle : fournir un échantillon récent anonymisé, identifier une preuve explicite du joueur local, tester l’entrée et la sortie ainsi que les événements d’autres acteurs, ajouter des fixtures. Les regex ont un timeout de 40 ms et les lignes de plus de 16 KiB sont ignorées. Une règle doit contenir un discriminant local non vide. Ces contraintes limitent les erreurs mais ne certifient pas une règle choisie par l’utilisateur.

Mining et Salvage utilisent les contextes réservés. Ajouter ensuite leurs catalogues et panneaux spécifiques ; aucun code ne présume des jauges métier.

## Sécurité et focus

L’écoute est exclusivement `IPAddress.Loopback` IPv4 avec contrôle du Host. En V0.2, le widget officiel se connecte automatiquement sans pairing manuel. Ce choix réduit la friction mais signifie qu’un autre processus local peut contacter l’API ; il n’y a toujours aucun accès LAN/Internet, aucune exécution shell via API ni accès arbitraire aux fichiers. Les actions restent limitées au catalogue et l’envoi réel exige Star Citizen au premier plan.

Un appel dangereux exige un lease de maintien par WebSocket. Les messages API ne peuvent pas choisir directement les scancodes : ils choisissent un identifiant du catalogue configuré. Un unique envoi à la fois, temporisation minimale des actions WS, impulsions clavier bornées et relâchement en finally. Une perte de focus pendant une impulsion interrompt rapidement le maintien ; la vérification du premier plan et l’envoi ne sont pas atomiques au niveau Windows.

La clé n’isole pas les applications malveillantes exécutées sous le même compte Windows, qui peuvent lire la configuration utilisateur. Le périmètre est un outil personnel local. Aucun serveur distant, aucune télémétrie.

## Déploiement

Distribution autonome Windows x64 incluant .NET 10. Installateur Inno Setup par utilisateur avec désinstallation et raccourcis ; démarrage Windows désactivé par défaut. L’app ne modifie jamais un fichier de jeu. La configuration et les logs restent locaux et sont conservés à la désinstallation.
