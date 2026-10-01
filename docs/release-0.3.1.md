# Asterion Edge v0.3.1

Interface de **v0.3.0-design.1**, raccordée à la dernière base stable **v0.3.0** (28 septembre 2026).

## Interface

- Cockpit panoramique POWER / FLIGHT / SHIP SYSTEMS / MFD, icônes vectorielles, contours cyan, textures et rail rouge des commandes sensibles de design.1.
- Mode à pied avec équipement, armes, emotes et journal ; onglets TARGET / MINE / SALVAGE ; panneau POWER / SHIELDS de la référence demandée.
- Sept palettes, couleurs libres et constructeur, taille du texte, angles, densité et effets réglables.
- Éditeur de raccourcis intégré. Les commandes complémentaires de design.1 sont ajoutées sans remplacer les actions existantes de la version stable ; les commandes non liées restent à configurer.

## Correctifs stables conservés

Gestion des profils déliés, comparaison SemVer, résistance aux configurations invalides/nulles, surveillance du jeu, rotation et reprise du journal, validation des contextes, boutons de thèmes du Companion et sélection correcte des lignes de l’éditeur Windows.

Les maintiens d’éjection et d’autodestruction restent protégés côté serveur. Les mises à jour du journal n’interrompent pas un maintien ; un rendu différé est appliqué après son achèvement ou son annulation. L’horloge est initialisée immédiatement et les réglages de taille du texte sont appliqués dans le cockpit.

## Installation

Quittez le Companion en cours, puis utilisez l’installateur Windows ou décompressez l’archive portable et lancez **Ouvrir-Asterion.cmd**. Le runtime .NET est inclus. Le widget `.icuewidget` est disponible séparément.

Les réglages existants sont conservés. Pour retrouver la palette cyan de la référence, choisissez **Réglages → RSI · Cyan**.

## Limites

Cette version reprend volontairement la présentation de design.1, dont l’onglet de boucliers directionnels. Elle ne rétablit pas de mécanique supprimée du jeu : seules les commandes liées et prises en charge dans le profil sont utilisables. L’état réel des équipements n’est pas télémétré. La détection automatique dépend toujours des événements locaux reconnaissables dans Game.log.

La validation locale couvre la compilation, les 23 tests du moteur et les essais de communication et d’interface en simulation. Un essai réel dans Star Citizen et sur iCUE/XENEON EDGE reste nécessaire.
