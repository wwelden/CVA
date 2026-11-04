# Artwork and text alignment

Reviewed the Canvas/TMP layouts in all 23 scenes under `Assets/Scenes`. Corrected eight scenes, prioritizing the starting menu and Lasagna screens.

- CutScene: keep Skip at 160×56 pixels with a 28-point font cap and a 24-pixel bottom-right inset, independent of viewport size.
- MainMenu: keep the action buttons registered to the illustrated menu sheet as the viewport changes shape. Give rules headings, body text, and the Exit button separate bounds. Fit credits to the cookbook pages.
- LasagnaMiniGame: fit the timer and caption inside the cutting board and keep the ingredient order inside the visible sidebar. Normalize the win/lose panels so their messages fit without uneven text stretching.
- LasagnaStartGame and OvenStart: register instructions and Play controls to the artwork rather than the window width.
- VeggieStart: use the same height scaling as the artwork and inset the rules within the board.
- SaladGame: scale the lives Canvas consistently with the other HUD Canvas.
- OvenMinigame: keep the score inside the visible pie badge.

`ArtworkTextLayout` projects an explicitly assigned renderer's bounds through its scene camera, applies a normalized content region, clips it to the visible viewport, and insets the text. Four labels use it: the Lasagna timer/caption/rules and oven score. Its `LateUpdate` handles resizing during play. The existing project uses orthographic cameras and full-screen overlay canvases with these labels as direct children.

The scene changes preserve existing gameplay components, sprite renderers, cameras, colliders, and button callbacks, checked against snapshots taken before the layout edits. Existing local Unity migration changes are retained. `scope-validation.json` records the comparison.

## Verification

The saved JSON reports cover 12 views at each of four resolutions: 1024×768, 1280×800, 1920×1080, and 2560×1080. Each pass checks text overflow, text rectangles against viewport bounds, and artwork-bound labels against the visible graphics. The intro also checks the Skip button’s exact pixel dimensions and corner inset. The extra views exercise Rules, Credits, Lasagna win, and Lasagna lose panels; result messages use the actual runtime wording and reward values.

These are Unity-rendered layout checks in temporary preview scenes. The verifier reproduces the configured CanvasScaler geometry on camera-facing canvases because preview scenes do not initialize screen-space canvases. It calls the same artwork layout method used at runtime. The JPEGs show Unity's actual TMP meshes and scene graphics. This is not a full gameplay playthrough or a player build test.

Run against the open Unity project:

```sh
unity command script run --file ArtRefresh/UILayout/verify-layout.cs --entry LayoutVerification.Run --args '[1280,800]' --caller plugin --skill ui-ugui --format json
```

Repeat with the other resolution pairs. The verifier does not save preview scenes. The project also has pre-existing merge conflict markers in several `.meta` files; Unity reports GUID parsing warnings when opening those scenes.

## Previews

[Starting menu, 16:10](previews/MainMenu-1280x800.jpg) · [Rules](previews/MainMenu-rulesPanel-1280x800.jpg) · [Credits](previews/MainMenu-creditsPanel-1280x800.jpg)

[Lasagna, 16:10](previews/LasagnaMiniGame-1280x800.jpg) · [Lasagna, 4:3](previews/LasagnaMiniGame-1024x768.jpg) · [Win panel](previews/LasagnaMiniGame-WinPanel-1280x800.jpg) · [Lose panel](previews/LasagnaMiniGame-losePanel-1280x800.jpg)

[Intro Skip button, 16:10](previews/CutScene-CutScene-1280x800.jpg)
