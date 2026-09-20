# Asterion protocol v2

Transport principal : WebSocket `ws://127.0.0.1:32147/events`.

Le serveur Kestrel écoute **uniquement sur IPv4 loopback**. Les hôtes acceptés sont `127.0.0.1` et `localhost`. Aucun accès LAN/Internet n’est ouvert et aucun asset distant n’est requis au runtime.

## Auto-link

La V0.2 n’exige plus de première trame d’authentification. Dès qu’un client WebSocket local se connecte, le companion envoie un événement `state`. Le widget officiel réessaie automatiquement avec backoff si le companion n’est pas encore lancé.

`requirePairing` existe uniquement comme mécanisme avancé/compatibilité dans `config.json`; il vaut `false` par défaut. L’ancien message `auth` de la V0.1 reste accepté en mode auto-link mais n’est pas nécessaire.

## Événements serveur

- `{"type":"state","state":{"protocol":2,...}}`
- `{"type":"ack","requestId":"...","ok":true}`
- `{"type":"ack","requestId":"...","ok":false,"error":"..."}`

Toutes les actions exposent `stateKnown:false` tant que le jeu ne fournit pas réellement l’état correspondant.

## Commandes client

| type | id | rôle |
|---|---|---|
| action | catalog id | exécuter une commande liée ou feedback simulation |
| context | AUTO / ON_FOOT / FLIGHT / ... | override manuel / retour auto |
| test | vide | événement de test local |
| holdBegin | action sensible | commencer la confirmation |
| holdPulse | vide | maintenir la confirmation |
| holdCancel | vide | annuler immédiatement |
| ping | vide | heartbeat |

Les maintiens sensibles sont liés à une seule connexion WebSocket, vérifient l’identité de l’action, expirent si les pulses s’arrêtent et sont consommés une seule fois.

## HTTP local

- `GET /health`
- `GET /state`
- `POST /action` pour les actions non dangereuses uniquement.

En mode auto-link ces routes ne demandent pas de bearer token, mais restent inaccessibles depuis le réseau car le serveur n’écoute que sur loopback. L’API n’accepte ni commande shell arbitraire ni scancode arbitraire.

## Modèle de sécurité

La suppression du pairing manuel est un choix UX. Un processus local malveillant pourrait contacter le service loopback ; Asterion ne prétend pas protéger contre un programme déjà exécuté sur le PC de l’utilisateur. Les garde-fous importants restent : catalogue d’actions fermé, maintien côté serveur pour les actions dangereuses, Star Citizen requis au premier plan pour l’envoi réel, aucun shell/lecture mémoire/injection.
