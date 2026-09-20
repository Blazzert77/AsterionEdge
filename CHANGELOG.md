# 0.3.0-design.1

Refonte du cockpit selon les références visuelles, sept palettes, boucliers intégrés, modes à pied / minage / récupération, 42 commandes configurables et éditeur de raccourcis. Corrections du démarrage Windows et du heartbeat. Voir [les notes de version](docs/release-design.1.md).

# Asterion Edge — changelog de développement

## v0.3.0-dev.3 — MFD-first redesign

- Refonte visuelle basée sur une logique de MFD tactile : gros boutons, groupes Power / Flight / Ship Systems / Target.
- Les commandes principales sont visibles immédiatement au lieu d'être enfouies dans de petites tuiles.
- Ajout d'un panneau Boucliers dédié : avant, arrière, gauche, droite, haut, bas et reset. Les action IDs sont maintenant reliés aux commandes `v_shield_raise_level_*` / `v_shield_reset_level`; le binding réel reste celui du joueur.
- Éjection et autodestruction isolées dans une colonne danger avec maintien obligatoire.
- Nouvelle disposition À pied avec Suit & View, Actions, Emotes et Event Feed.
- Les commandes non liées restent visibles mais désactivées avec leur statut, au lieu de disparaître.
- Ajout d'un emplacement Refroidisseurs en binding manuel tant qu'aucun action ID actuel n'est suffisamment vérifié.
- Barre de session persistante : vaisseau si connu, localisation, shard, branche/build.
- Version affichée : `v0.3.0 · DEV.3`.
- Personnalisation avancée conservée.
- Aucun faux état de bouclier/puissance n'est affiché : Asterion envoie des commandes, il ne prétend pas connaître leur état sans télémétrie fiable.
