# Pantry Prowler walking animation

Shooting extension: see [antenna charge and throw](../PantryProwlerShoot/README.md). The combined branch now adds a third Animator state and changes projectile timing; the original walk-only validation below describes the earlier commit.

Six transparent walking poses for the original teal Pantry Prowler, facing left to match the Shooter enemy. The loop runs at 8 fps (0.75 seconds) with alternating suction-cup steps, a small body bob and feeler sway.

![Walk loop](walk.gif)

## Unity integration

- `Assets/Sprites/PantryProwlerWalk.png`: 2700 × 1320 atlas, three columns by two rows. Six 900 × 660 cells; 100 pixels per unit and centered pivots. Each frame retains the existing 9 × 6.6 world canvas of Alien3.png.
- `Assets/PantryProwlerWalk.anim`: six poses plus a closing first-frame key at 0.75 seconds. Only SpriteRenderer.m_Sprite animates.
- `Assets/PantryProwlerIdle.anim`: original Alien3.png sprite.
- `Assets/PantryProwler.controller`: Idle (default) and Walk; immediate transitions controlled by the Walking boolean, without exit time.
- `PantryProwlerAnimation.cs`: follows Shooter's enabled state, nonzero speed, reloading and pancaked fields. Stops the walk during reloading or immobilization and clears Walking when disabled. Uses normal Animator time so game pause applies.
- `Assets/Resources/Prefabs/Shooter.prefab`: Animator and animation bridge added to the root. Existing components, movement, combat, physics, hitboxes and tint behavior remain unchanged. The WaveTransition cutscene keeps the original static sprite.

Checkout the PR branch to use it. Alternatively, copy the packaged unity-patch/Assets folder over the project after the full art refresh (PR #2) is merged. Keep all .meta files. No manual slicing or Animator setup is required.

## Validation

Static checks passed: all six atlas regions match their individual preview frames, GUIDs are unique, sprite key references resolve, the ground baseline is fixed, and all pre-existing prefab components are byte-for-byte unchanged. New clips contain no transform or tint curves.

C# compilation passed against the installed Unity 2022.3.62f3 engine assemblies using a minimal Shooter field contract. This is not a full project compilation. An attempted Unity batch import in a disposable project copy aborted while starting Package Manager before any animation import or Editor checks ran. Full Unity import and gameplay playtest remain pending. The project version and packages have not been upgraded.

In Unity 2022.3.43f1, verify a spawned Shooter walks, returns to idle when reloading or pancaked, preserves damage/freeze tint, and pauses with Time.timeScale. A reusable Editor verification source is included as verify-editor.cs (copy temporarily into Assets/Editor and run ProwlerWalkCheck.Run); it has not run successfully in this environment.

## Source and preview

Built-in imagegen created the six-pose sheet from the supplied original Pantry Prowler image. Sharp performed deterministic slicing, torso registration, baseline alignment and atlas packing. generated-master.png and prompt.txt preserve the generation source; manifest.json records the slices and normalization. All PNG sprites retain full alpha. walk.gif uses a cream preview background and alternating 120/130 ms frame delays because GIF timing is in 10 ms increments; the native Unity animation uses exactly 8 fps.
