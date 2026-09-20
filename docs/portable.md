# Utiliser Asterion Edge portable

1. Décompressez entièrement l’archive dans un dossier accessible en écriture.
2. Quittez tout Companion Asterion déjà ouvert depuis son icône de notification.
3. Lancez **Ouvrir-Asterion.cmd**. Le Companion démarre et le cockpit s’ouvre dans le navigateur.
4. Importez le fichier `.icuewidget` fourni dans iCUE pour le XENEON EDGE.
5. Vérifiez le dossier Star Citizen dans le Companion si la détection automatique ne le trouve pas.

Le runtime .NET est inclus. Les paramètres portables sont sauvegardés dans `data/config.json`. La configuration de votre version installée reste inchangée. Pour réutiliser vos réglages, copiez votre fichier de configuration existant dans ce sous-dossier avant le premier lancement.

**Simulation.cmd** permet un essai sans envoyer de touches au jeu. Quittez le Companion avant de passer de la simulation au mode normal.

Dans **Réglages → Commandes & raccourcis**, associez les commandes marquées À CONFIGURER aux raccourcis affectés dans Star Citizen, par exemple `kb1_lalt+n`. Les 42 commandes supplémentaires n’ont pas de touches inventées par défaut.

Le contexte automatique et les couleurs constructeur dépendent des événements reconnus dans Game.log. Les tests automatisés utilisent la simulation ; la validation en jeu et l’import iCUE restent à effectuer sur votre matériel.
