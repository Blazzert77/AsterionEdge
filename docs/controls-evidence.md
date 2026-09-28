# Commandes vérifiées dans le jeu installé

Source primaire : `Data/Libs/Config/defaultProfile.xml` extrait en lecture seule de l’archive LIVE `Data.p4k`, build `4.10.193.11644`, vérifié le 29 septembre 2026. Le profil complet et les journaux privés ne sont pas inclus dans le dépôt.

| Fonction | Groupe / action | Clavier par défaut | Activation |
|---|---|---|---|
| Autodestruction | spaceship_general / v_self_destruct | backspace | delayed_press_medium, seuil 0,5 s |
| SCM/NAV | spaceship_movement / v_master_mode_cycle_long | b | delayed_press, seuil 0,25 s |
| Démarrage | spaceship_general / v_flightready | ralt+r | press |
| Moteurs | spaceship_power / v_power_toggle_thrusters | i | press |
| Armes | spaceship_power / v_engineering_assignment_weapons_* | f5, f5+lalt | increase/decrease : tap ; max/min : hold 0,25 s |
| Moteurs | spaceship_power / v_engineering_assignment_engine_* | f6, f6+lalt | idem |
| Boucliers | spaceship_power / v_engineering_assignment_shields_* | f7, f7+lalt | idem |
| Réinitialisation | spaceship_power / v_engineering_assignment_reset | f8 | press |
| Portes | spaceship_general / v_toggle_all_doors, v_open_all_doors, v_close_all_doors | aucune | press |
| Verrouillage | spaceship_general / v_lock_all_doors, v_unlock_all_doors | aucune | press |
| Débit refroidissement | spaceship_general / v_cooler_throttle_up, v_cooler_throttle_down | aucune | onPress + onHold |

Les délais envoyés (400 et 650 ms) ajoutent une marge aux seuils. Les taps d’allocation sont bornés à 150 ms pour rester sous 250 ms. Un profil joueur peut remplacer les touches, pas automatiquement les règles d’activation de l’action ; les activations complexes exportées restent refusées plutôt que réduites à un appui simple.

Les entrées non vérifiées du catalogue restent identifiées comme candidates ou manuelles. La nouvelle suite `tests-controls` instancie le vrai Engine, avec des profils synthétiques, et vérifie les touches et durées résolues sans démarrer la surveillance ni appeler WindowsInput.
