# Manual Vercel deployment

Pushes and pull requests do not deploy the game. To deploy, manually run
[Deploy WebGL to Vercel](https://github.com/wwelden/CVA/actions/workflows/deploy-webgl.yml)
using **Run workflow** with `main` selected.
GitHub Actions builds the game with the Unity version in `ProjectSettings/ProjectVersion.txt`,
then uses Bun and Vercel CLI to upload `Builds/WebGL` to production at
[cva-psi.vercel.app](https://cva-psi.vercel.app).

## One-time credentials

Add these under [repository Actions secrets](https://github.com/wwelden/CVA/settings/secrets/actions):

| Secret | Value |
| --- | --- |
| `UNITY_LICENSE` | The complete Unity `.ulf` license file for the GameCI build environment. |
| `UNITY_EMAIL` | The Unity account email used with that license. |
| `UNITY_PASSWORD` | That Unity account's password. |
| `VERCEL_TOKEN` | A [Vercel access token](https://vercel.com/account/tokens) with access to the `wweldens-projects` team and `cva` project. |

See [GameCI activation](https://game.ci/docs/github/activation/) for license setup.
Keep credentials out of source control. The workflow checks for missing secrets before
downloading Unity or building the game.

The workflow targets Vercel project `prj_uizfR8NZJUbhbLFyb67qBJoCWBrp` in team
`team_gKkroO4XiWdxmxF0cBs6LLl5`. These identifiers are not secrets.
If the project moves to another team, update both identifiers in the workflow.

## Verify a deployment

After configuring the secrets, choose **Run workflow** on the
Actions page with `main` selected. Confirm both the Unity build and Vercel deployment
steps succeed, then open the production URL. The first Unity import can take longer;
later runs reuse the cached `Library` folder.

Vercel's own Git builds are intentionally disabled in the root `vercel.json`:
Vercel cannot build the Unity project. The Actions workflow uploads the compiled
game with its generated hosting configuration from `Assets/Editor/WebGLBuild.cs`.
