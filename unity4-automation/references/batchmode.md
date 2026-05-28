# Unity 4.x Batchmode Reference

## Command-line syntax

```bash
Unity.exe -batchmode [options]
```

## Core flags

| Flag | Purpose |
|------|---------|
| `-batchmode` | Run without GUI; required for headless automation |
| `-quit` | Exit Unity after command completes (always include) |
| `-projectPath "C:\path\to\project"` | Target project directory |
| `-executeMethod ClassName.Method` | Invoke static method in Editor assembly |
| `-logFile "C:\path\to\output.log"` | Redirect log output to file |
| `-buildTarget <target>` | Override build target: `standalone`, `standalonewindows`, `standaloneosxintel`, `ios`, `android`, `webplayer`, `webplayerstreamed` |
| `-nographics` | Disable GPU usage during batchmode (useful on headless servers) |
| `-force-free` / `-force-pro` | Force use of specific license tier if both installed |
| `-username` / `-password` | License activation credentials |
| `-serial` | License serial number for activation |
| `-accept-apiupdate` | Auto-accept API updater dialogs |

## Build via executeMethod

Create `Assets/Editor/<ClassName>.cs`:

```csharp
using UnityEditor;

class AutoBuilder
{
    static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        BuildPipeline.BuildPlayer(
            scenes,
            "Builds/MyGame.exe",
            BuildTarget.StandaloneWindows,
            BuildOptions.None
        );
    }
}
```

Invoke:
```bash
Unity.exe -batchmode -quit -projectPath . -executeMethod AutoBuilder.PerformBuild -logFile build.log
```

## BuildOptions flags (Unity 4.x)

- `None` — default
- `Development` — dev build with profiler
- `AutoRunPlayer` — auto-launch after build
- `ShowBuiltPlayer` — reveal in file manager
- `BuildAdditionalStreamedScenes` — include streamed scenes
- `AcceptExternalModificationsToPlayer` — allow external modifications
- `ConnectToHost` — connect to Unity profiler on host
- `ConnectWithProfiler` — enable profiler connection
- `AllowDebugging` — script debugging enabled
- `SymlinkLibraries` — symlink instead of copy DLLs (OSX)
- `UncompressedAssetBundle` — don't compress asset bundles
- `Development` — include development features

## Common failure patterns

1. **Missing scenes in Build Settings** — `BuildPipeline.BuildPlayer` requires scenes listed explicitly; they must exist in the array passed, not just in Build Settings
2. **Compile errors block executeMethod** — batchmode won't invoke methods if scripts don't compile. Always test locally first.
3. **License activation** — headless machines need license file at `%PROGRAMDATA%\Unity\Unity_*.ulf` or use `-username -password -serial`
4. **Exit codes** — Unity 4.x batchmode always returns 0 even on failure; parse `logFile` for `"Exiting batchmode with fatal error"` or `"exiting with 1..."`
5. **Path with spaces** — wrap `-projectPath` in quotes

## Exit code workaround

Unity 4.x batchmode ignores internal errors on exit. Wrap with a script that scans logs:

```powershell
$log = "build.log"
& Unity.exe -batchmode -quit -projectPath . -executeMethod Builder.DoBuild -logFile $log

if (Select-String -Path $log -Pattern "fatal error|exception|error CS" -CaseSensitive -Quiet) {
    exit 1
}
```

## AssetBundle building (Unity 4.x)

```csharp
// Single asset to bundle
BuildPipeline.BuildAssetBundle(
    Selection.activeObject,
    null,
    "Bundles/myasset.unity3d",
    BuildAssetBundleOptions.CollectDependencies,
    BuildTarget.StandaloneWindows
);

// Multiple assets
Object[] assets = Selection.objects;
BuildPipeline.BuildAssetBundle(
    null,
    assets,
    "Bundles/multi.unity3d",
    BuildAssetBundleOptions.CollectDependencies,
    BuildTarget.StandaloneWindows
);
```

Note: Unity 4.x AssetBundle format is incompatible with Unity 5+.
