# Asterion Edge — 0.3.0-design.1

Base : tag v0.3.0-dev.3, commit 0af62e3. Préversion de refonte visuelle.

## Interface

Reconstruction du cockpit à partir des six références fournies : bandeau de session, quatre blocs POWER / FLIGHT / SHIP SYSTEMS / MFD, rail rouge, journal inférieur, motifs de lignes, pictogrammes vectoriels et disposition panoramique. Mode à pied avec commandes vaisseau et emotes, équipement, armes et journal. Les images promotionnelles et la coque physique de l’écran ne font pas partie de l’interface.

Énergie et boucliers partagent le premier panneau. Le MFD comprend Target / Mine / Salvage. Sept palettes, couleurs libres et couleurs de constructeur lorsque le vaisseau est identifié. Réglages de texte, opacité, coins, effets et densité sauvegardés par le Companion. Les petits écrans défilent sans supprimer de panneau.

## Commandes

Les commandes existantes et leurs identifiants sont conservés. 42 entrées configurables complètent les boutons des références. Elles n’ont PAS de raccourci par défaut inventé : utiliser Réglages → Commandes & raccourcis et saisir le raccourci affecté dans Star Citizen, par exemple `kb1_lalt+n`. Le bouton Démarrage doit correspondre à la commande de préparation au vol configurée dans le jeu ; ce n’est pas une séquence de bascules approximative.

Les commandes non liées sont désactivées en jeu. En simulation elles sont exécutées sans envoyer de saisie, et l’interface affiche SIMULATION. Les commandes de minage et récupération sont disponibles après affectation des raccourcis. Le changement automatique de contexte et la reconnaissance du vaisseau restent tributaires des événements et des règles de Game.log de la version existante. Aucune jauge de bouclier ou position ON/OFF n’est présentée comme de la télémétrie réelle.

Éjection, autodestruction et alimentation conservent le maintien validé côté serveur. Relâchement, perte de focus et déconnexion annulent le maintien.

## Corrections

- Nom du mutex Windows corrigé pour permettre le lancement des sources.
- Réponse au heartbeat pour éviter une reconnexion toutes les douze secondes.
- Boutons de thèmes du Companion corrigés.
- Journal étendu à 80 événements ; mission observée affichée dans le bandeau.
- Éditeur de raccourcis avec validation côté serveur et réinitialisation.

## Validation

Compilation Windows .NET 10 ; 21 tests du cœur et 8 tests de protocole réussis. Tests navigateur sur le vrai Companion en simulation : maintien / annulation, modes, couleurs, persistance des raccourcis, heartbeat et rendu aux formats 2560×720, 1920×550, 1280×400, 1024×768 et 390×844. Captures issues de la simulation. Pas de validation des touches dans une session réelle de Star Citizen ni sur un écran XENEON EDGE physique ; import iCUE à vérifier sur le poste utilisateur.
