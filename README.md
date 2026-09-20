# Asterion Edge 0.3.0-design.1

Refonte du cockpit basée sur v0.3.0-dev.3 et les références visuelles fournies. Voir [les changements et la validation](docs/design-reference.md).

Asterion Edge est un cockpit tactile local pour **CORSAIR XENEON EDGE** et **Star Citizen**.

Cette préversion passe sur une logique **MFD-first** : Power, Flight, Ship Systems, Targeting et Shields sont organisés en gros contrôles tactiles utilisables en jeu. Le mode À pied change automatiquement de disposition.

## Principes

- gros boutons lisibles sur le XENEON EDGE ;
- commandes de vaisseau prioritaires ;
- changement de layout selon le contexte ;
- éjection / autodestruction protégées par maintien ;
- boucliers directionnels disponibles quand le profil joueur fournit les bindings ;
- aucune fausse télémétrie : commande disponible ≠ état réel confirmé ;
- Companion local uniquement sur `127.0.0.1`.

Voir `CHANGELOG.md` pour les changements de cette itération.
