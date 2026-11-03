# CVA salad-style artwork refresh

Nine replacement assets for [wwelden/CVA](https://github.com/wwelden/CVA), based on commit acaa6d23bc69df392fd15a2c03a8d70ef838bb1e.

The visual reference is the existing salad character with big eyes. All nine replacements use dark contours, vivid colors, rounded forms, and simple cel shading. Original compositions are followed so existing overlays retain their space. Props and backgrounds do not acquire new faces.

## Replacements

| Asset | Original pixels | Replacement pixels | Scenes |
|---|---:|---:|---|
| fork2.jpg | 480 × 320 | 1440 × 960 | MainMenu.unity |
| Screenshot 2024-12-16 115522.png | 412 × 278 | 1648 × 1112 | MainMenu.unity |
| pieOven.jpg | 470 × 368 | 1410 × 1104 | OvenStart.unity |
| Oven.jpg | 612 × 459 | 1224 × 918 | OvenMinigame.unity |
| Screenshot 2024-11-13 100538.png | 518 × 511 | 1036 × 1022 | LCProject.unity, LasagnaMiniGame.unity |
| veggie.jpg | 700 × 547 | 1400 × 1094 | VeggieStart.unity |
| Screenshot 2024-11-27 092111.png | 156 × 185 | 1092 × 1295 | LasagnaStartGame.unity, LasagnaMiniGame.unity |
| RedCookbook.jpg | 600 × 400 | 1800 × 1200 | MainMenu.unity |
| Screenshot 2024-12-16 115435.png | 612 × 585 | 1224 × 1170 | MainMenu.unity |

## Install

Close Unity, back up or commit your project, and use this branch or copy the packaged unity-patch/ over the project root, retaining the Assets/Sprites paths. Include each .meta file. Open with Unity 2022.3.43f1 and allow textures to reimport. The branch/PR provides the same changes without manual copying.

Existing filenames, GUIDs, sprite IDs, pivots, wrap/filter modes, sprite mesh settings and scene references are preserved. Each texture is resized to an integer multiple of its old dimensions; pixels-per-unit increases by exactly the same factor, preserving world-space size. Maximum texture sizes cover the replacements, and compression quality is raised to 100 while keeping existing platform compression modes. Generated masters are saved under artwork/; their native sizes are listed separately in manifest.json. Modest resampling to an exact integer scale is documented there; replacement dimensions are not claims about native generation resolution.

## Validation and limits

Verified final image dimensions and aspect ratios, unchanged asset GUIDs, unchanged world-space sprite bounds, and empty custom sprite geometry for each modified asset. No scenes, scripts, animation sheets or colliders were modified. Reviewed all nine generated masters visually against the salad reference. Unity Editor is not installed at the standard location, so these assets have not been play-tested or rebuilt for WebGL. Check MainMenu, LasagnaMiniGame, LasagnaStartGame, OvenStart, OvenMinigame and VeggieStart in Unity, especially text visibility, the lasagna ingredient example and oven placement. The itch.io build has not been updated.

## Audit and scope

Inspected 105 image assets under Sprites (including the TextMesh Pro sprite directory); 79 are referenced by serialized project files. This count includes indirect prefab references and disabled/development scenes, not only assets reached by the active build. The audit records dimensions, metadata and references in asset-audit.json.

This is a focused pass on nine low-resolution background, decorative food and instruction assets. It does not make every existing asset in the game stylistically uniform. The chef's sliced pixel-art animation sheet, small gameplay sprites, planet prefabs, and the existing salad style reference were preserved. Tiny menu labels and rule text baked into images are better migrated to live TextMesh Pro text in a UI pass; enlarging the screenshots would not restore sharp lettering. Full replacement of character animation and gameplay sprites would need a coordinated separate pass for animations, silhouettes and collision shapes.

## Generation provenance

Used the built-in image_gen tool with each original asset as the edit target and the salad character as a separate style reference. Full final prompts and native generated dimensions are in manifest.json. Earlier photographic drafts were superseded and are not installed.
