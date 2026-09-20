# Roadmap — Asterion Edge

> Roadmap publique du projet. Les numéros de version suivent une logique SemVer : correctif = patch, ajout fonctionnel important = minor, première version stable = v1.0.0.

## Version actuelle

**v0.2.1 — phase de test**

## En développement — v0.3.0

### Refonte complète de l’interface
- Nouvelle interface plus moderne, plus lisible et plus directe
- Organisation centrée en priorité sur le pilotage et les systèmes du vaisseau
- Suppression des doublons entre menus et actions
- Écran principal contextuel avec actions rapides
- Mode FPS / à pied volontairement plus léger
- Affichage systématique de la version complète : `v0.3.0`, `v0.3.1`, etc.

### Personnalisation
- Interface française par défaut
- Sélection de langue
- Couleur principale et couleur secondaire
- Couleur d’accent
- Thèmes / presets visuels
- Couleurs automatiques selon le constructeur du vaisseau lorsque pertinent
- Densité de l’interface
- Taille / lisibilité des éléments
- Animations activables / désactivables
- Personnalisation des actions rapides affichées
- Préparation de layouts plus personnalisables à terme

### Contexte de jeu
- Détection améliorée de Star Citizen lancé / arrêté
- Détection LIVE / PTU / EPTU et build
- Détection améliorée de `ON FOOT` / `FLIGHT`
- Mode manuel de secours si la détection automatique échoue
- Détection du vaisseau uniquement lorsqu’une source fiable le permet

### Informations de session
- Statut Companion
- Statut du jeu
- État de `Game.log`
- Shard / serveur lorsqu’il est exposé dans les logs
- Localisation lorsqu’elle peut être déterminée proprement
- Session feed avec événements réellement observés
- Aucun faux état de jeu ou fausse télémétrie

### Contrôles et keybinds
- Refonte du système de keybinds
- Prise en compte des bindings personnalisés du joueur
- Fallback documenté pour les bindings par défaut lorsque possible
- Réduction des faux `NOT BOUND`
- Actions principales du cockpit :
  - Landing Gear
  - VTOL
  - Request Landing
  - SCM / NAV
  - Cruise Control
  - Decoupled
  - Lights
  - Scan / Ping
  - Countermeasures
  - MobiGlas
  - Starmap
- Pages secondaires pour les commandes avancées réellement utiles
- Maintien obligatoire pour les actions dangereuses

### Mise à jour
- Vérification des nouvelles versions via GitHub Releases
- Notification de mise à jour disponible
- Préparation du téléchargement / remplacement simplifié
- Même numéro de version partout : widget, Companion, installateur, GitHub et patch notes

## Stabilisation — v0.3.x

Les versions `v0.3.1`, `v0.3.2`, etc. seront réservées aux :
- correctifs
- améliorations UX mineures
- corrections de bindings
- compatibilité avec les patchs Star Citizen
- optimisation des performances
- corrections de détection de contexte

## Prévu ensuite — v0.4.0

### Session, missions et économie
Ces fonctions seront ajoutées uniquement si les données sont récupérables de manière fiable.

- Missions en cours / terminées lorsque détectables
- Historique de session
- Récompenses observées
- Transactions commerciales observées
- Achats / ventes issus des logs lorsque disponibles
- Gains / pertes observés pendant la session
- Bilan économique de session sans présenter une estimation comme une valeur officielle
- Intégrations API optionnelles si une API publique ou une clé utilisateur permet d’obtenir des données fiables

### Informations enrichies
- Données de vaisseau statiques depuis une source fiable
- Informations de constructeur / rôle / capacité lorsque disponibles
- Écrans spécialisés Mining / Salvage si leur usage justifie une page dédiée
- Informations de cible / scan uniquement si Star Citizen ou une API expose réellement ces données

## Plus tard

- Système de thèmes exportables / importables
- Profils de disposition personnalisés
- Choix et réorganisation plus poussée des blocs
- Extension à d’autres jeux si l’architecture le permet
- Installation et mise à jour toujours plus transparentes

## Règle du projet

Asterion Edge distingue toujours :
- **commande disponible**
- **état réel confirmé par le jeu**

Si une donnée n’est pas exposée de manière fiable, Asterion ne l’invente pas.
