# Validation — Asterion Edge 0.3.0-dev

## Correctif import iCUE conservé

Le correctif qui avait résolu l’erreur d’import « Élément title manquant » est conservé : `<!DOCTYPE html>` en majuscules, `<title>Asterion Edge</title>` littéral placé en premier dans `<head>`, `translation.json` au format attendu et fichiers du widget à la racine de l’archive `.icuewidget`.

## Contrôles effectués sur la V0.2

- `app.js`, `protocol.test.cjs` et `ui.test.cjs` passent la vérification syntaxique Node.
- Les JSON principaux sont valides.
- L’archive `.icuewidget` passe `unzip -t` sans erreur et contient directement `index.html`, `style.css`, `app.js`, `icon.svg`, `manifest.json`, `translation.json`.
- Les anciennes propriétés iCUE `pairToken` / `companionPort` et le dialogue CONNECT / SETUP ont été retirés de l’interface.
- Le protocole V2 établit le lien automatiquement sur loopback.
- L’installateur source configure le démarrage Windows avec `--background` et propose l’import du widget à la fin.

## Ce qui n’a pas pu être exécuté dans cet environnement

Cette session tourne sous Linux et ne contient pas le SDK .NET Windows ni Inno Setup. Le companion WinForms et l’installateur Windows n’ont donc pas été compilés ici. `BUILD_AND_INSTALL.cmd` automatise cette compilation sur un PC Windows, puis lance l’installateur généré.

Le test final à faire sur le PC cible reste : installation, présence de l’icône dans la zone de notification, import iCUE, passage automatique à `COMPANION CONNECTED`, puis détection de `StarCitizen.exe`.
