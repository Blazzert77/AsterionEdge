# Asterion Edge v0.3.0 — Stable

Version stable d’Asterion Edge pour le CORSAIR XENEON EDGE, avec l’interface graphique classique de la dernière base dev.2 et les correctifs techniques récents.

## Correctifs importants

- Le maintien **Éjection / Autodestruction** n’est plus annulé par les mises à jour d’état ou les événements `Game.log`.
- Les pages **Combat**, **Systèmes** et **Réglages** retrouvent une mise en forme MFD cohérente et sans débordement.
- Le réglage **Taille du texte** est désormais réellement appliqué.
- L’horloge s’affiche immédiatement au démarrage.
- Une touche explicitement **déliée** dans le profil Star Citizen n’est plus remplacée par le raccourci clavier par défaut.
- Correction des boutons de thèmes **Obsidienne** et **Nuit bleue**.
- L’éditeur de commandes n’enregistre plus un override sur la mauvaise action après un tri.
- Le Companion résiste mieux aux erreurs de surveillance et aux `config.json` invalides ou contenant des valeurs `null`.
- La lecture de `Game.log` est plus fiable lors d’une nouvelle session ou d’une rotation du fichier.
- La comparaison des versions respecte maintenant correctement SemVer (`0.3.0` > `0.3.0-dev.3`).
- Le Companion répond désormais au heartbeat `ping` du widget avec `pong`.
- Les contextes manuels invalides sont refusés et `mouse4` / `mouse5` ne sont plus sensibles à la casse.

## Interface

- Retour à l’interface graphique classique de la dernière base dev.2.
- Les correctifs de stabilité, bindings, Game.log, configuration, heartbeat et sécurité du maintien sont conservés.

## Validation

- Suite de tests du projet : **23/23 réussis** dans l’environnement de validation des correctifs.
- La CI GitHub Windows compile ensuite le Companion, le widget `.icuewidget` et l’installateur `.exe` avant publication.

Pour le détail technique, voir `CHANGELOG.md`.
