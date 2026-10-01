# Asterion Edge v0.3.3 — Voyants et contexte

Le cockpit design.1 et les correctifs de v0.3.2 sont conservés.

- **Commandes non liées** : mention « À configurer » sous les boutons concernés, y compris dans les panneaux compacts et les commandes protégées.
- **Portes et verrous** : trois boutons distincts dans Atterrissage & docking : Portes, Verrous et Ports. Les boutons latéraux souris 4/5 sont reconnus dans les profils `kb1_` et `mo1_`. L’affectation réellement enregistrée dans le profil du jeu est respectée ; aucune touche souris universelle n’est imposée.
- **Verrous des ports** : bascule Alt droit + K par défaut, remplaçable par le profil ou un raccourci manuel.
- **Voyants persistants** : 22 commandes à état disposent d’un libellé et d’une surbrillance indépendante. Ouvrir les portes puis utiliser un autre bouton conserve le voyant des portes. Les états restent disponibles lors des changements de page et des reconnexions au Companion.
- **Synchronisation** : Réglages → États & voyants permet de renseigner l’état observé dans le jeu. Un état inconnu reste inconnu après une bascule, au lieu d’inventer une position initiale. Les ordres explicites ouvrir/fermer donnent une estimation directement.
- **Contexte automatique** : reconnaissance des notifications françaises de connexion/déconnexion au canal d’un vaisseau connu. L’interface indique « À bord ≈ ». Le dernier contexte est également recherché dans la fin du journal au démarrage du Companion ou à la détection du jeu.

## Ce que signifient les voyants

Le journal examiné ne fournit pas l’état réel des portes, lumières ou systèmes. **✓** indique un état renseigné manuellement ; **≈** une estimation après une commande du dashboard. Une action exécutée directement dans le jeu peut désynchroniser cette estimation. Les états sont remis à inconnu au changement de contexte, de vaisseau ou de session et au redémarrage du Companion. Les actions momentanées, comme Ping, n’ont pas d’état marche/arrêt permanent. Les quantités réelles d’énergie distribuée ne sont pas inventées.

La détection par canal est un indice de présence à bord, pas une preuve d’installation au siège pilote. Elle dépend des notifications reconnues du journal ; la sélection manuelle reste disponible.

## Installation et validation

Quittez le Companion ouvert avant d’installer cette version. Importez ou sélectionnez le profil contenant vos affectations de portes si nécessaire.

Compilation locale réussie ; 27 tests du moteur, 14 tests des commandes et 6 tests de communication passent. Les essais navigateur couvrent le design, les raccourcis, les maintiens, la personnalisation et la persistance des voyants. Les tests utilisent la simulation : les commandes finales restent à essayer dans Star Citizen.
