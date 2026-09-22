# Asterion Edge 0.3.0-design.2

Refonte du cockpit basée sur v0.3.0-dev.3 et les références visuelles fournies. Voir [les changements et la validation](docs/design-reference.md).

Asterion Edge est un cockpit tactile local pour **CORSAIR XENEON EDGE** et **Star Citizen**.

Cette préversion organise les points d’énergie, le vol, les systèmes et le ciblage en contrôles tactiles utilisables en jeu. Le contexte suit les événements reconnus du journal ; l’entrée automatique dans le siège reste à valider. Voir [les notes de cette version](docs/release-design.2.md).

## Principes

- gros boutons lisibles sur le XENEON EDGE ;
- commandes de vaisseau prioritaires ;
- changement de layout selon le contexte ;
- éjection / autodestruction protégées par maintien ;
- distribution d’énergie aux armes, moteurs et boucliers ;
- aucune fausse télémétrie : commande disponible ≠ état réel confirmé ;
- Companion local uniquement sur `127.0.0.1`.

Voir `CHANGELOG.md` pour les changements de cette itération.
