# Food damage feedback

Food with health now shares feedback across projectile and enemy-contact damage. A red flash and brief recoil affect only a visual child; the source transform, collider, combat values, and status tints remain under the existing gameplay code. Contact hits pulse at most once every 0.32 seconds.

At two thirds health, food gains chipped edges, cracks, and slight discoloration. At one third health, the missing chunks and wear increase. These sprites are generated once per source sprite and stage, cached, and keep the original pivot and pixel density. No new imported artwork or prefab changes are needed.

A world-space bar above damaged food tracks its exact remaining health, changes from green to amber to red, and fades during the final 0.4 seconds of its 2.2-second visibility window. Ongoing damage refreshes that window. Pie and tomato also support the existing integer-lives path; their default one-hit behavior and impact effects are unchanged. Pancakes retain their existing one-hit behavior.

## Verification

The verifier runs against isolated Unity preview scenes, calling the actual prefab damage methods and explicitly exercising lifecycle and render updates. It checks both damage stages, cached sprites, sprite size/pivot, unchanged physics-root geometry, status tints, hit recoil/flash, health-bar fading, and disable/re-enable restoration. It also checks pies and tomatoes with three lives. See validation.txt for the result and damage-stages.png for the Unity-rendered comparison. This is not a full gameplay or player-build test.

Run in the open Unity project:

```sh
unity command script run --file ArtRefresh/FoodDamage/verify-editor.cs --entry FoodDamageVerification.Run --caller plugin --skill ui-ugui --format json
```

The projectile regression harness also passes 39 assertions against the actual Shooter, animation bridge, and AlienShot source using lightweight Unity API stubs. Run with `dotnet run --project ArtRefresh/PantryProwlerShoot/Tests/Check.csproj`.
