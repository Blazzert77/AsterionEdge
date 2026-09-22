# Asterion Edge v0.3.0-design.2

Commandes actualisées et distribution d’énergie, en conservant le design du cockpit.

- Remplacement du panneau de boucliers directionnels par les points ARMES / MOTEURS / BOUCLIERS : +1, −1, MIN, MAX et réinitialisation. Le débit de refroidissement reste une commande distincte.
- Correction des identifiants de commandes et des raccourcis à partir du profil par défaut de la version locale Star Citizen 4.10.193.11644. Éjection : action du siège. Autodestruction : maintien clavier de 650 ms après le maintien de confirmation du cockpit.
- Séparation des commandes ouvrir, fermer, basculer, verrouiller et déverrouiller les portes. L’ancien raccourci de déverrouillage est conservé sous sa bonne fonction.
- Surbrillance persistante des systèmes, avec synchronisation manuelle dans Réglages → Indicateurs / Synchro. L’état initial inconnu est affiché comme tel ; seuls les ordres envoyés avec succès modifient les estimations.
- Prise en compte de la sortie du siège lorsque le journal fournit la signature locale observée. L’entrée automatique dans le siège reste à valider avec un nouvel essai en jeu ; le sélecteur manuel reste disponible.

## À configurer avant l’essai

Les actions d’ouverture et de fermeture des portes n’ont pas de touche par défaut dans le profil du jeu examiné. Assignez-leur des touches dans Star Citizen puis importez le profil, ou renseignez les mêmes touches dans Réglages → Commandes & raccourcis. Déverrouiller les portes ne les ouvre pas.

Les surbrillances sont des **estimations**, pas une lecture des équipements du jeu. Synchronisez leur état initial dans les réglages ; une action au clavier ou en jeu peut ensuite les désynchroniser. Les traits du panneau d’énergie représentent les ajustements envoyés, pas le nombre réel de points disponibles ou alloués.

## Installation et validation

Quittez le Companion ouvert avant d’installer cette version. Utilisez l’installateur Windows ou l’archive portable (Ouvrir-Asterion.cmd). Le widget iCUE est fourni séparément.

27 tests du moteur, 8 tests de protocole et les essais navigateur ont réussi localement en simulation, notamment les points, les états des portes et l’annulation des commandes sensibles. La validation des commandes dans une session réelle de Star Citizen reste nécessaire.
