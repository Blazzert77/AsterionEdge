# 0.3.2 — Commandes de vol

Autodestruction et SCM/NAV avec les durées attendues, allocation d’énergie actuelle, séparation ouverture/verrouillage des portes, ordre des modificateurs et protection des durées personnalisées. Tests du résolveur réel ajoutés à la compilation. Voir [les notes](docs/release-0.3.2.md).

# 0.3.1 — Interface design.1 sur base stable

Reprise du cockpit de v0.3.0-design.1 sur le moteur v0.3.0 stable. Correctifs récents conservés, rendu différé après maintien, sept palettes, personnalisation et commandes supplémentaires configurables. Voir [les notes de version](docs/release-0.3.1.md).

# Asterion Edge — changelog de développement

## v0.3.0 — stable

- Interface graphique classique restaurée depuis la dernière base dev.2 (`632b307`) ; les correctifs techniques restent intégrés.

- Widget : un maintien Éjection / Autodestruction n'est plus annulé quand un événement Game.log ou une mise à jour d'état arrive pendant le maintien.
- Widget : les pages Combat, Systèmes et Réglages avaient des classes CSS sans style (boutons en ligne, page Réglages qui débordait) ; elles reprennent le style des panneaux MFD. Le cockpit principal est inchangé.
- Widget : le réglage « Taille du texte » est réellement appliqué ; l'horloge s'affiche immédiatement.
- Companion : une touche explicitement déliée dans le profil (`kb1_ `) n'est plus remplacée par la touche par défaut 4.10.
- Companion : les boutons « Thème Obsidienne » et « Thème Nuit bleue » envoyaient des thèmes inexistants (erreur « Invalid theme »).
- Companion : l'éditeur de commandes n'attribue plus les overrides à la mauvaise action après un tri de colonne.
- Companion : nom du mutex d'instance unique corrigé (`Local\AsterionEdgeCompanion`).
- Companion : une erreur imprévue n'arrête plus définitivement la surveillance du jeu / Game.log.
- Companion : un `config.json` illisible ou contenant des `null` n'empêche plus le démarrage (copie en `config.json.corrupt`).
- Companion : détection d'un nouveau Game.log par son en-tête (plus fiable que la date de création) ; un Game.log créé après le démarrage est lu depuis le début.
- Companion : la vérification des mises à jour compare correctement les préversions (0.3.0 > 0.3.0-dev.3).
- Companion : réponse `pong` au heartbeat du widget ; contexte manuel invalide refusé.

## v0.3.0 — base MFD-first

- Refonte visuelle basée sur une logique de MFD tactile : gros boutons, groupes Power / Flight / Ship Systems / Target.
- Les commandes principales sont visibles immédiatement au lieu d'être enfouies dans de petites tuiles.
- Ajout d'un panneau Boucliers dédié : avant, arrière, gauche, droite, haut, bas et reset. Les action IDs sont maintenant reliés aux commandes `v_shield_raise_level_*` / `v_shield_reset_level`; le binding réel reste celui du joueur.
- Éjection et autodestruction isolées dans une colonne danger avec maintien obligatoire.
- Nouvelle disposition À pied avec Suit & View, Actions, Emotes et Event Feed.
- Les commandes non liées restent visibles mais désactivées avec leur statut, au lieu de disparaître.
- Ajout d'un emplacement Refroidisseurs en binding manuel tant qu'aucun action ID actuel n'est suffisamment vérifié.
- Barre de session persistante : vaisseau si connu, localisation, shard, branche/build.
- Version affichée : `v0.3.0`.
- Personnalisation avancée conservée.
- Aucun faux état de bouclier/puissance n'est affiché : Asterion envoie des commandes, il ne prétend pas connaître leur état sans télémétrie fiable.
