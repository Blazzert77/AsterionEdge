# Asterion Edge v0.3.2 — Commandes de vol corrigées

Correctifs sur la base stable et le cockpit design.1 de v0.3.1. Les identifiants, touches et modes d’activation ci-dessous ont été vérifiés dans le profil de commandes de l’installation LIVE **4.10.193.11644**, le 29 septembre 2026.

- **Autodestruction** : Retour arrière envoyé pendant 650 ms, au-delà du seuil de 500 ms attendu par le jeu. Le maintien de confirmation du dashboard reste obligatoire et distinct de l’appui clavier.
- **SCM / NAV** : utilisation de `v_master_mode_cycle_long`, touche B maintenue 400 ms. L’ancienne action sans touche par défaut était incorrecte pour ce bouton.
- **Énergie** : commandes d’allocation actuelles pour les armes, moteurs et boucliers. F5/F6/F7 ajoutent un point ; Alt + F5/F6/F7 en retirent un. MAX/MIN envoient un appui long ; F8 réinitialise. Les modificateurs sont envoyés avant les autres touches, même si le profil exporté les écrit dans l’ordre inverse.
- **Portes** : ouverture/fermeture, ouverture seule, fermeture seule, verrouillage et déverrouillage sont séparés. Un ancien raccourci manuel du bouton Portes est conservé comme déverrouillage, sans être réutilisé silencieusement pour ouvrir.
- **Démarrage et moteurs** : identifiants corrigés, respectivement Alt droit + R et I par défaut. L’action d’éjection utilise le bon groupe de commandes du siège.
- Les durées des raccourcis manuels sont bornées selon le mode d’activation : un ancien appui court ne casse plus l’autodestruction ou SCM/NAV, et un appui long ne transforme plus un +1 point en MAX.
- Une perte de focus pendant un appui retourne une interruption plutôt qu’un faux succès ; les touches sont toujours relâchées.

## Portes : configuration nécessaire

Star Citizen ne fournit **aucun raccourci par défaut** pour ouvrir/fermer toutes les portes. Le bouton du cockpit affiche CONFIGURER et ouvre directement les commandes concernées si aucune touche n’est liée.

1. Dans les commandes clavier de Star Citizen, attribuez une touche à l’ouverture/fermeture de toutes les portes.
2. Exportez votre profil pour sa détection par le Companion, ou renseignez exactement la même touche dans **Réglages → Commandes & raccourcis** pour « Ouvrir / fermer les portes ».

Un raccourci de déverrouillage seul, par exemple le bouton latéral de souris du profil examiné, n’ouvre pas les portes. Le débit des refroidisseurs reste également sans touche par défaut : il se configure séparément et ne représente pas une allocation de points.

## Interface et validation

Le cockpit design.1 est conservé. Dans le panneau gauche, l’onglet **POINTS** remplace le schéma de boucliers directionnels par les réglages ±1 / MIN / MAX des trois systèmes. Aucun total réel de points ni état matériel n’est inventé.

Validation locale : compilation réussie, **23 tests du moteur + 11 tests du résolveur réel de commandes + 6 tests de communication**, ainsi que trois suites navigateur (maintien pendant les mises à jour, personnalisation, commandes, portes non liées et affichage responsive). Ces essais n’envoient aucune touche à Star Citizen. Le fonctionnement final doit être essayé dans le jeu.

Quittez le Companion ouvert avant d’installer cette version. Installateur Windows, archive portable et widget iCUE fournis.
