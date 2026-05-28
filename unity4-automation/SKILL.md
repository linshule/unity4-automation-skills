---
name: unity4-automation
description: "Automate Unity 4.6.8 (and 4.x) projects via batchmode, editor scripting, and CI integration. Use when the user needs to: (1) run Unity 4.x from the command line for builds, (2) write or fix editor scripts (MenuItem, AssetPostprocessor, EditorWindow), (3) batch-process textures/models/assets, (4) set up CI/CD pipelines for Unity 4.x, (5) understand Unity 4.x API limitations vs modern Unity, or (6) troubleshoot batchmode errors or license activation on headless machines."
---

# Unity 4.x Automation

Automate Unity 4.6.8 projects: batchmode builds, editor scripting for bulk asset operations, external script wrappers, and CI integration patterns.

## Quick start: batchmode build

Minimal invocation:

```bash
Unity.exe -batchmode -quit -projectPath "C:\path\to\project" -executeMethod Builder.DoBuild -logFile build.log
```

Create `Assets/Editor/Builder.cs`:

```csharp
using UnityEditor;

public class Builder
{
    static void DoBuild()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            scenes[i] = EditorBuildSettings.scenes[i].path;

        BuildPipeline.BuildPlayer(scenes, "Builds/Game.exe",
            BuildTarget.StandaloneWindows, BuildOptions.None);
    }
}
```

**Critical**: Unity 4.x batchmode always returns exit code 0. Parse `-logFile` for `"fatal error"`, `"error CS"`, or `"exiting with 1"` to detect failures.

## Core workflows

### 1. Multi-platform batch build

Use `scripts/unity_batch_build.ps1` — a PowerShell wrapper that handles log scanning and multi-target builds:

```powershell
.\unity_batch_build.ps1 `
    -UnityPath "C:\Program Files (x86)\Unity\Editor\Unity.exe" `
    -ProjectPath "C:\MyProject" `
    -ExecuteMethod "Builder.DoBuild" `
    -Targets @("StandaloneWindows", "Android")
```

Read `references/batchmode.md` for full command-line reference, BuildOptions flags, and AssetBundle building.

### 2. Editor scripting (MenuItem, Postprocessor, EditorWindow)

Three entry points for automation inside the Editor:

- **MenuItem** — binds static methods to menu entries (including `Ctrl+` shortcuts)
- **AssetPostprocessor** — hooks asset import pipeline (auto-set texture format, model scale, audio settings on import)
- **EditorWindow** — builds custom tool windows for repeatable batch workflows

Read `references/editor_scripting.md` for complete API reference with code samples.

### 3. Batch asset processing patterns

Copy `scripts/batch_set_texture_format.cs` into `Assets/Editor/` for ready-to-use texture batch processing (max size, mipmap toggle, format conversion):

- `Tools > Batch > Set All Textures Max Size 256`
- `Tools > Batch > Set All Textures Max Size 512`
- `Tools > Batch > Disable Mipmaps on All Textures`
- `Tools > Batch > Set All Textures to TrueColor`
- `Tools > Batch > Set All Textures to Compressed`

Extend the pattern by modifying the `AssetDatabase.FindAssets("t:texture2D")` filter to target other asset types (`t:prefab`, `t:model`, `t:material`, `t:audioClip`).

### 4. CI/CD integration

Recommended pipeline for Unity 4.x + Jenkins (or similar):

1. **Source control setup**: `Force Text` serialization + `Visible Meta Files` (required for VCS)
2. **License**: Place `Unity_*.ulf` in `%PROGRAMDATA%\Unity\`, or pass `-serial` at build time
3. **Build** via `scripts/unity_batch_build.ps1` — handles multi-platform, log parsing, exit codes
4. **Artifacts**: Archive `Builds/` directory
5. **Notifications**: Parse log for error patterns; alert on failure

### 5. Selecting automation approach

| Need | Best approach |
|------|---------------|
| One-off or scheduled build | `-batchmode` + `-executeMethod` |
| Reusable editor tool for team | `MenuItem` + `EditorWindow` |
| Enforce import standards | `AssetPostprocessor` |
| Multi-platform CI pipeline | `scripts/unity_batch_build.ps1` |
| Bulk asset modification | Editor script with `AssetDatabase.FindAssets` |

## What not to try

- **Don't** use C# features newer than ~3.0: no `async/await`, no `dynamic`, no `?.`, no string interpolation
- **Don't** try IL2CPP builds — Mono-only in Unity 4.x
- **Don't** use SceneManager — `Application.LoadLevel()` is the 4.x API
- **Don't** mix AssetBundle formats — 4.x bundles are incompatible with Unity 5+
- **Don't** expect exit codes from `Unity.exe` — always scan logs

Read `references/limitations.md` for full API gaps, platform support, and .NET constraints.

## References

- `references/batchmode.md` — full batchmode CLI reference, BuildOptions, AssetBundle building, error patterns
- `references/editor_scripting.md` — MenuItem, AssetPostprocessor, EditorWindow, AssetDatabase, TextureImporter APIs
- `references/limitations.md` — C#/.NET constraints, API gaps vs Unity 5+, platform support matrix, VCS setup

## Scripts

- `scripts/unity_batch_build.ps1` — PowerShell multi-platform batch build wrapper with log scanning
- `scripts/batch_set_texture_format.cs` — ready-to-use editor script for batch texture processing (max size, mipmaps, format)
## Common pitfalls when building games

### Shader names
Use Unity 4.x naming: `"Self-Illumin/Diffuse"` (not `"Self-Illuminated/..."`), emission color via `SetColor("_Illum", ...)` (not `"_Emission"`). Full list in `references/limitations.md`.

### Procedural textures
Textures created with `new Texture2D()` in Editor scripts **must** be saved via `AssetDatabase.CreateAsset()` or they are lost on Play. See `references/editor_scripting.md`.

### CharacterController
- Use thick Cube (not Plane) for ground — Plane colliders cause unreliable `isGrounded`
- Remove colliders from child objects (e.g. gun models) — they push the CC around
- `cc.minMoveDistance` does not exist in 4.x
- Spawn player at Y >= 1.5 to avoid spawning inside geometry
- Use `Screen.lockCursor` / `Screen.showCursor` for cursor control (not `Cursor.lockState`)

### Creating .cs files externally
Always use **CRLF line endings + UTF-8 without BOM**. PowerShell `StreamWriter` with `NewLine = "\`r\`n"` and `UTF8Encoding($false)`. Never use `Set-Content -Encoding UTF8` (adds BOM). See `references/editor_scripting.md`.

### Material assignment in editor scripts
Use `renderer.sharedMaterial` instead of `renderer.material` to avoid leaking material instances and the associated warning. See `references/limitations.md`.
