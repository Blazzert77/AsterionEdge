# Installation et utilisation — Asterion Edge v0.3.0-dev.3

## Installation

1. Lancer `AsterionEdge-Setup-v0.3.0-dev.3.exe`.
2. Le Companion est installé dans `%LOCALAPPDATA%\Programs\AsterionEdge`.
3. Asterion démarre avec Windows et reste dans la zone de notification.
4. Importer le fichier `.icuewidget` dans iCUE puis l'ajouter au XENEON EDGE.
5. Lancer Star Citizen : la liaison locale se fait automatiquement.

## Bindings

Asterion cherche un export `layout_asterion_exported.xml`, puis le profil `actionmaps.xml`. Pour obtenir un export complet :

`pp_rebindkeys export all asterion`

Les actions sans binding restent visibles mais désactivées. Les boucliers directionnels utilisent les action IDs Star Citizen vérifiés ; Asterion n'invente jamais une touche absente du profil.

## Connexion locale

Le widget utilise `ws://127.0.0.1:32147/events`. Le Companion n'écoute que sur loopback, jamais sur le LAN ou Internet.

## Sécurité

Éjection et autodestruction exigent un maintien. En dehors du mode simulation, Asterion n'envoie des touches que lorsque Star Citizen est au premier plan.

## Dépannage

- **Companion déconnecté** : vérifier l'icône Asterion dans la zone de notification.
- **NON LIÉ** : exporter les bindings ou définir un override manuel.
- **Contexte inconnu** : utiliser temporairement À pied / Vol.
- **Windows refused input** : vérifier que Star Citizen et Asterion utilisent un niveau de privilèges compatible.
