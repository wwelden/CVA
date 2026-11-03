# Chefs vs Aliens: unified art refresh

40 generated replacement assets, two sliced UI graphics, and eight updated scenes for [wwelden/CVA](https://github.com/wwelden/CVA). The existing salad character and the supplied Battlegrounds screenshot set the visual style: bold dark contours, rounded forms, clear cel shading, bright food colors and warm neutral backgrounds.

## Screens covered

| Screenshot / scene | Changes |
|---|---|
| KitchenPrep | Isometric kitchen, sign and title, illustrated chef with walking poses, station labels and timer HUD. |
| OvenStart | Illustrated oven backdrop, crisp native instructions and matching controls. |
| OvenMinigame | Brick oven, pie, thermometer, chef life icons, coordinated timing-zone colors and controls. |
| VeggieStart | Illustrated vegetable board and knives, readable native title/rules, matching controls. |
| SaladGame | Illustrated kitchen, knife, whole/flying vegetables and chopped halves, timer and lives HUD. |
| LasagnaStartGame | Illustrated board, native title/rules and Play button, matching ingredient example. |
| LasagnaMiniGame | Illustrated kitchen, quiet chalkboard sidebar, timer board, plate and ingredients, readable HUD. |
| MainMenu | Cartoon tabletop, menu sheet, title and food illustrations; four menu actions use live TextMesh Pro labels. |

The two numeric lives-display expressions in OffScreen.cs now show LIVES plus the remaining count. The life decrement and game-over behavior are unchanged.

## Original alien replacement

Alien3.png now contains an original Pantry Prowler design: a rounded teal alien with orange feelers and feet, large purple eyes and leaf-shaped side fins. The previous film creature is no longer used by this texture. Its legacy filename and GUID are retained so prefabs keep their references. The previous creature and old chef sheet are omitted from the downloadable comparison originals; Git history retains the prior versions.

## Install and review

Use branch art/full-game-style-refresh from your fork, or copy the complete unity-patch/ directory from the downloadable pack over the project root. Include the scene files, the two-line OffScreen.cs presentation change, and every .meta file. Copying textures alone will miss the native UI changes. Open with Unity 2022.3.43f1 and allow assets to reimport.

Play through MainMenu → KitchenPrep → each instruction scene → each minigame. Check movement, front/back/side animation cycles, cooking-station triggers, Play/Exit controls, timing zones, lives and timers, plate/ingredient catches, vegetable chopping and halves, and the Pantry Prowler in Battlegrounds. Unity Editor was unavailable in this environment, so engine compilation, play mode, WebGL size/memory/performance and the itch.io build remain unverified. The existing itch.io upload has not been changed.

## Technical compatibility

Existing texture GUIDs, sprite IDs and pivots are preserved. Single-sprite textures retain their file aspect ratio and pixels-per-unit proportion so their world-space canvas sizes remain the same. Transparent foreground art is registered to the original alpha bounds. Import texture limits cover the installed dimensions. New controls use small uncompressed sliced UI cards.

The chef sheet uses a new 5×3 grid of padded cells and 15 illustrated poses. All 55 original slice names, internal IDs and sprite IDs are retained and mapped to their direction/pose; existing animation clips and keyframe timing are unchanged. Visible chef height is normalized to 0.53 world units with a common foot baseline. Cell bounds are standardized to 0.4×0.56 world units, so the atlas has an explicit exception to the single-sprite bounds rule. Existing player transforms and gameplay colliders are unchanged.

No physics components, world transforms, prefab files, gameplay component settings or button callbacks were modified. Native UI layout, text, decorative visibility, button colors, camera background colors and selected sprite tints were updated in the eight scenes.

## Validation and previews

validation.json records image dimension/alpha/import checks, preserved GUIDs, the chef slice migration, scene YAML parsing, local references, new UI hierarchy, physics and custom-component comparisons, and unchanged button callbacks and animation files. Static scene-data layout previews were reviewed for composition and estimated text fit. These previews approximate Unity fonts/materials and omit dynamically spawned gameplay; they are not evidence of an engine play-test.

[Preview gallery](preview.md) includes the clean scene layout reconstructions. The downloadable preview.html adds all before/after assets and the chef frame inspection. manifest.json contains full generation prompts, native generated dimensions, final installed dimensions, scaling, transparency registration and atlas mappings. Generated image dimensions and installed dimensions are recorded separately; normalization can include modest resampling. The asset audit records the original image inventory and serialized references, including indirect and development-scene references.

## Generation

Artwork was created with the built-in image_gen tool, using each source image as the edit target and the salad character / approved in-game screenshot as style references. The two simple sliced UI shapes were drawn in code. Generated masters are preserved in the downloadable artwork/ directory. Remaining scenes outside the supplied screenshots, such as the space cutscenes, have not received a complete art pass.
