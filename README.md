# Lumber — bûcheron low-poly PS1

Jeu mobile à la première personne : vous incarnez un bûcheron qui coupe des
arbres pour gagner de l'argent et de l'expérience, dans une esthétique
low-poly façon PS1 (rendu pixelisé, ombrage plat, sans lissage).

C'est un **projet Unity (C#)**. Tout le monde de jeu (sol, arbres, joueur,
interface) est **généré automatiquement au lancement, par code** — il n'y a
aucune scène à monter à la main : ouvrez le projet, appuyez sur Play, tout
apparaît.

## Ouvrir le projet

1. Installez [Unity Hub](https://unity.com/download).
2. Dans Unity Hub, ajoutez ce dossier comme projet existant ("Open" /
   "Add project from disk").
3. Le projet cible Unity **2022.3 LTS**. Si vous n'avez pas exactement la
   version indiquée dans `ProjectSettings/ProjectVersion.txt`, Unity Hub
   proposera de l'installer — vous pouvez aussi accepter d'ouvrir avec
   n'importe quelle version 2022.3 LTS que vous avez déjà, ça fonctionnera.
4. Une fois l'éditeur ouvert, allez dans `Assets/Scenes/Game.unity` (double-clic)
   puis appuyez sur **Play**. Le jeu se construit tout seul.

Pipeline de rendu : **Built-in Render Pipeline** (volontairement, pas URP) —
plus simple, plus léger, et cohérent avec un rendu "caméra basse résolution"
façon PS1.

## Contrôles

**Sur mobile (build Android/iOS) :**
- Joystick en bas à gauche : déplacement
- Glisser n'importe où sur la moitié droite de l'écran : regarder autour de soi
- Bouton rouge "COUPER" en bas à droite : donner un coup de hache
- Bouton "BOUTIQUE" en haut : ouvrir la boutique de haches

**Dans l'éditeur Unity (pour tester sans appareil mobile) :**
- ZQSD / WASD (`Horizontal`/`Vertical`) : déplacement
- Clic droit maintenu + souris : regarder autour de soi
- Clic gauche ou Espace : donner un coup de hache
- Maj gauche : sprint

## Ce qui est déjà implémenté

- **Contrôleur FPS mobile** (`Assets/Scripts/Player/FirstPersonController.cs`) :
  `CharacterController`, joystick + glissement tactile, avec repli
  clavier/souris pour tester dans l'éditeur.
- **Hache** (`AxeTool.cs`) : animation de coup procédurale + raycast qui inflige
  des dégâts à l'arbre visé.
- **Arbres low-poly procéduraux** (`World/LowPolyTreeFactory.cs`,
  `MeshBuilder.cs`) : troncs et feuillages générés en mesh à facettes plates
  (aucun asset 3D importé nécessaire), dispersés sur la carte
  (`ForestGenerator.cs`) avec évitement de chevauchement.
- **Système d'arbre** (`Tree.cs`) : points de vie, réaction au coup, chute
  animée, repousse après un délai.
- **Économie & progression** (`Economy/EconomyManager.cs`,
  `ExperienceManager.cs`) : argent gagné par bûche coupée, XP avec courbe de
  niveaux.
- **Boutique de haches** (`Shop/ShopManager.cs`) : 6 paliers (bois → pierre →
  fer → acier → or → légendaire), chacun plus rapide et plus puissant.
- **Sauvegarde locale** (`Core/SaveManager.cs`) : argent, niveau, XP et hache
  équipée sont sauvegardés en JSON (`Application.persistentDataPath`) et
  rechargés au lancement, avec sauvegarde automatique périodique.
- **Rendu PS1** (`Rendering/PS1RenderEffects.cs` + shader
  `Shaders/PS1FlatLit.shader`) : la caméra est rendue dans une petite
  RenderTexture (240px de haut par défaut) puis ré-affichée en filtrage "point"
  → look pixelisé et anguleux ; le shader personnalisé fait un ombrage plat
  sans reflets, comme sur PS1.
- **HUD & UI** (`UI/UIManager.cs`) : argent, niveau, barre d'XP, joystick,
  pavé de visée, bouton de coupe, panneau boutique — tout construit par code.

## Pour builder sur mobile

1. `File > Build Settings`, choisissez Android ou iOS, cliquez sur
   "Switch Platform".
2. Ajoutez `Assets/Scenes/Game.unity` à la liste des scènes ("Add Open Scenes"
   après l'avoir ouverte).
3. Dans `Player Settings`, réglez l'orientation par défaut sur **Landscape**
   (paysage) — le script force déjà l'auto-rotation en paysage au lancement,
   mais le régler aussi dans Player Settings évite un flash en portrait au
   démarrage.
4. Build & Run.

## Idées d'évolution

- Ajouter un vrai modèle 3D de hache/bras plutôt que le cube placeholder.
- Sons de coup de hache / chute d'arbre / achat en boutique.
- Types d'arbres différents (valeur de bois variable).
- Mission/quêtes journalières, objets à collecter (champignons, pierres).
- Habillage de personnage acheté avec l'argent gagné.
- Multi-scènes (forêt → village avec la boutique physique).
