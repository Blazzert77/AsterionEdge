# Vérification technique — 20 septembre 2026

## Sources officielles consultées

1. CORSAIR, [création de widgets XENEON EDGE](https://www.corsair.com/us/en/explorer/diy-builder/accessories/how-to-create-a-custom-widget-for-the-xeneon-edge/), mise à jour 16 juillet 2026. HTML/CSS/JS, QtWebEngine, CLI de packaging. Cet article mentionne encore iCUE 5.44 ; la spécification canonique plus récente exige 5.47.
2. Elgato, [Getting Started](https://docs.elgato.com/icue/widgets/) et [Specification](https://docs.elgato.com/icue/widgets/specification/), API **1.4.0**, iCUE **5.47**, QtWebEngine 6.9.3 / Chromium 130. Le manifest doit déclarer `interactive:true` et `dashboard_lcd`.
3. Références CORSAIR, [tailles et validation](https://github.com/Corsair-Labs/icue-widget-builder/blob/main/skills/icue-widget-builder/references/security-and-testing-checklists.md). Large horizontal 1688×696, XL 2536×696. Pas de champ manifest inventé pour imposer XL : l’utilisateur choisit la disposition dans iCUE. L’interface répond à la taille réelle du viewport.
4. Elgato, [cycle de vie](https://docs.elgato.com/icue/widgets/specification/#icue-events). `onICUEInitialized`, `onDataUpdated` et `iCUE_initialized`. Reconnexion indépendante du cycle de vie ; nettoyage sur pagehide et annulation de maintien sur perte de visibilité.
5. RSI, [patch notes](https://robertsspaceindustries.com/en/patch-notes) et [problèmes connus 4.10.1](https://support.robertsspaceindustries.com/hc/en-us/articles/360056254754-Star-Citizen-Alpha-4-10-1-Known-Issues). Version publique affichée au moment de la recherche : **Alpha 4.10.1**.
6. RSI, [profils de commandes](https://support.robertsspaceindustries.com/hc/en-us/articles/360000183328-Create-export-and-import-custom-profiles), mise à jour 17 février 2026. XML exporté sous `USER\Client\0\Controls\Mappings`, nom `layout_<nom>_exported.xml`, export console `pp_rebindkeys export all <nom>`.
7. RSI, [fichiers de diagnostic](https://support.robertsspaceindustries.com/hc/en-us/articles/360000065688-Send-In-Game-Files-for-RSI-Support). Game.log décrit la dernière exécution et des échanges avec les serveurs ; ce n’est pas une API de télémétrie contractuelle.
8. Microsoft, [.NET sur Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows). .NET 10 choisi ; build autonome x64. Limites de support Windows 10 précisées dans le guide d’installation.
9. Microsoft, [SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput). Entrée synthétique soumise au niveau d’intégrité/UIPI. Aucun contournement de ces restrictions.

## Résultats locaux (lecture seule)

- iCUE installé : **5.51.42.0**.
- Une installation LIVE trouvée ; `build_manifest.id` indique **4.10.193.11644**, branche interne **sc-alpha-4.10.0**, build daté du 15 septembre 2026. Cette valeur interne est affichée telle quelle ; elle n’est pas transformée artificiellement en numéro marketing 4.10.1.
- Le journal examiné date du 9 septembre ; il n’est donc pas une preuve des événements produits par le build installé du 15 septembre.
- Le profil `actionmaps.xml` contient trois rebinds : portes sur `kb1_mouse4`, inventaire et looting sur `kb1_tab`. Il ne décrit pas la totalité des touches actives.
- Signatures observées : `<Join PU>`, `[CSessionManager::OnClientSpawned] Spawned!`, `<SystemQuit>`. Elles servent à des événements de session, jamais à inventer une position ou à conclure ON_FOOT.
- Aucune signature fiable d’entrée/sortie de siège du joueur local dans cet échantillon. Aucun adaptateur de boarding activé par défaut.
- Aucun log réel, identifiant joueur, adresse serveur ou clé d’association n’est livré dans le projet.

## Sandbox / permissions : distinction entre établi et à valider

La spécification publique consultée n’expose pas de permission générique permettant à un widget de lire les fichiers Star Citizen ou d’envoyer des touches. Ces tâches restent dans le companion. Les plugins iCUE listés ne constituent pas une API Star Citizen.

La documentation consultée ne fournit pas de garantie explicite pour les WebSockets `ws://127.0.0.1` depuis toutes les origines de widgets iCUE, ni de procédure officielle de permission localhost. Le choix WebSocket est donc une **hypothèse d’intégration à tester dans l’iCUE installé**, pas une compatibilité certifiée. Aucun champ de permissions non documenté ajouté au manifest, aucun lancement de navigateur avec sandbox désactivée. Le serveur et le widget sont fonctionnels ensemble dans Chromium ; le paquet passe le CLI officiel.

## Décisions V1

HTML/CSS/JS sans dépendance distante ; C# .NET 10 WinForms + Kestrel ; liaison WebSocket automatique en loopback ; HTTP de diagnostic ; état explicite UNKNOWN/Unavailable ; override manuel ; catalogue de candidats séparé des bindings réellement prouvés. Les faces de bouclier, refroidisseurs, anciennes fonctions ou raccourcis non vérifiés ne reçoivent aucun binding supposé.

## Notes 0.3.0-dev — données locales ajoutées

- `Game.log` expose le shard courant sur les signatures `<Join PU> ... shard[...]` et `<Update Shard Id> New Shard Id: ...`. Asterion parse les deux et n'invente pas un serveur si aucune signature n'est vue.
- La ligne `<Join PU>` peut contenir `locationId[...]`. Asterion expose cette valeur brute comme localisation initiale, sans la convertir en nom marketing tant qu'un mapping fiable n'est pas fourni.
- Les achats/ventes de marchandises peuvent apparaître via `SendCommodityBuyRequest` / `SendCommoditySellRequest` avec un total. Asterion additionne seulement ces flux observés avec les récompenses `Awarded ... aUEC`. Le résultat est nommé « flux observé », jamais « bénéfice net » ni « solde ».
- `USER/Client/0/Profiles/default/actionmaps.xml` ne contient pas nécessairement les touches qui sont restées aux valeurs par défaut. La V0.3 ajoute donc une petite table de fallback clavier 4.10, volontairement limitée à des commandes courantes documentées. Un binding réellement présent dans le profil utilisateur garde toujours la priorité.
- Le fallback n'est pas une télémétrie de jeu : il décrit une touche à envoyer. Les états réels des boucliers, armes ou systèmes restent inconnus tant qu'une source fiable ne les expose pas.
