# Asterion Edge — changelog de développement

## v0.3.0-dev.3 — MFD-first redesign

- Refonte visuelle vers une logique de véritable panneau MFD tactile : gros boutons et groupes **Power / Flight / Ship Systems / Target**.
- Les commandes importantes sont désormais visibles directement au lieu d'être réparties dans de petites tuiles difficiles à lire.
- Ajout d'un panneau **Boucliers** dédié : avant, arrière, gauche, droite, haut, bas et reset.
- Les commandes de répartition de bouclier utilisent les action IDs Star Citizen vérifiés `v_shield_raise_level_*` et `v_shield_reset_level`; la touche envoyée reste celle trouvée dans le profil du joueur.
- **Éjection** et **Autodestruction** sont isolées dans une zone danger et nécessitent un maintien.
- Nouvelle disposition **À pied** avec Suit & View, Actions, Emotes et Event Feed.
- Les commandes sans binding restent visibles mais désactivées avec leur statut au lieu de disparaître.
- Ajout d'un emplacement **Refroidisseurs** en binding manuel tant qu'un action ID actuel n'est pas suffisamment vérifié.
- Barre de session persistante : vaisseau si connu, localisation, shard et build.
- Version affichée proprement : **v0.3.0 · DEV.3**.
- Personnalisation des thèmes et couleurs conservée.
- Aucune fausse télémétrie : Asterion peut envoyer une commande sans prétendre connaître l'état réel du système.
