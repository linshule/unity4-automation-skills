# Unity 4.6.8 Limitations

## .NET / C# constraints

- **Runtime**: .NET 2.0 Subset (no `dynamic`, no `async/await`, no `Task`, no `Action<T>` >4 params)
- **LINQ**: Partial support via `System.Linq` but some methods unavailable. Prefer `foreach` over `.Select()`/`.Where()` chains when issues arise.
- **C# version**: ~C# 3.0 level. No named arguments, no optional parameters, no `var` in all contexts.
- **String interpolation**: Not available. Use `string.Format()` or `+` concatenation.
- **Generics**: Limited; avoid deeply nested generic types which can confuse Mono.
- **No `System.Collections.Generic.IReadOnlyList<T>` or newer collection interfaces**

## API gaps vs Unity 5+

| Feature | Unity 4.6.8 | Unity 5+ |
|---------|-------------|----------|
| Build system | `BuildPipeline.BuildPlayer` | `BuildPipeline.BuildPlayer` + BuildReport API |
| AssetBundle format | Legacy (`.unity3d`) | New format, incompatble with 4.x |
| IL2CPP | Not available | Available |
| SceneManager | Not available, use `Application.LoadLevel()` | Full SceneManager API |
| UGUI / uGUI | Introduced in 4.6, but limited vs 5.x | Full-featured |
| Editor Coroutines | Not built-in. Use EditorApplication.update | Native `EditorCoroutine` |
| AssetDatabase V2 | Not available | Available in 2017.3+ |
| Presets | Not available | Available in 2018.1+ |

## Platform support

| Platform | Status in 4.6.8 |
|----------|-----------------|
| Windows Standalone | x86 only (32-bit), XP+ |
| OS X Standalone | 32-bit + 64-bit Intel |
| Android | ARMv7, supports 2.3+ |
| iOS | 6.0+, 32-bit + 64-bit |
| Web Player | Supported (NPAPI, deprecated in modern browsers) |
| WebGL | Preview only, not production-ready |
| Windows Store / UWP | Not available |
| Linux | Not available |
| tvOS | Not available |

## Shader / Graphics

- **Shader target**: `#pragma target 2.0` to `3.0` depending on platform. No `target 4.0+`.
- **No PBR / Standard Shader**: Unity 5's Standard Shader does not exist. Use Legacy shaders.
- **No Enlighten**: Use Beast lightmapper (baked lightmaps).
- **Graphics API**: DirectX 9/11 (Windows), OpenGL ES 2.0 (mobile). No Vulkan, Metal.

## Editor

- **UI**: IMGUI only for Editor tools. uGUI (UnityEngine.UI) is for runtime only.
- **Layout**: Use `GUILayout` / `EditorGUILayout` APIs.
- **No UIElements / UI Toolkit** — that's Unity 2019+.

## Project format

- **Serialization**: Binary-text mixed. Set `Edit > Project Settings > Editor > Asset Serialization` to `Force Text` for proper VCS.
- **.meta files**: Must be visible (Text serialization + Visible Meta Files). Required for proper source control.
- **No Package Manager**: All assets are either in the project or imported via `.unitypackage`.

## VCS recommendations

- **Meta files**: `Edit > Project Settings > Editor > Version Control > Mode: Visible Meta Files`
- **Asset Serialization**: `Edit > Project Settings > Editor > Asset Serialization > Mode: Force Text`
- **Common practice for Unity 4.x era**: SVN or Perforce. Git also works but monitor `.asset` merge conflicts.

## Build size / Performance

- No code stripping by default. Use `BuildOptions.Development` or strip levels manually.
- Mono runtime size: ~5-8 MB overhead.
- No incremental build caching.

## License / Installation

- Unity 4.6.8 download archives: Built into Unity Hub? No — pre-Hub era. Use standalone installer.
- License activation: Manual via `-serial` flag or `%PROGRAMDATA%\Unity\Unity_*.ulf`.
- Free (Personal) vs Pro: Some features Pro-only: Render-to-texture, profiler, etc. (changes per patch version).



## Shader naming differences (Unity 4.x vs 5+)

| Unity 5+ name | Unity 4.x name |
|---------------|----------------|
| `Self-Illuminated/Diffuse` | `Self-Illumin/Diffuse` |
| `Self-Illuminated/Bumped Diffuse` | `Self-Illumin/Bumped Diffuse` |
| `Self-Illuminated/Specular` | `Self-Illumin/Specular` |

Also, the emission color property is `_Illum` in Unity 4.x (`SetColor("_Illum", color)`), not `_Emission` (Unity 5+).

Legacy shaders available: `Diffuse`, `Bumped Diffuse`, `Specular`, `Bumped Specular`, `Self-Illumin/Diffuse`, `Transparent/Diffuse`.

## CharacterController pitfalls

### isGrounded reliability
`cc.isGrounded` can be unreliable in Unity 4.x, especially with thin colliders. Use a **thick Cube** (e.g. scale 50×1×50) instead of a Plane for ground — Plane colliders are infinitely thin and often fail to register CC ground contact.

### Child colliders interfere with parent CharacterController
A child GameObject with a **non-trigger collider** (e.g. a gun model cube) will physically interact with the environment and can push/jitter the parent CharacterController. When a gun model tilts down (camera pitch), its collider can intersect the ground and launch the player upward.

**Fix**: Either remove the child collider entirely (`DestroyImmediate` in editor, `Destroy` at runtime), or mark it as a trigger (`col.isTrigger = true`).

### minMoveDistance
`CharacterController.minMoveDistance` does **not** exist in Unity 4.x. Remove any references to it.

### spawn height
Spawn the player at Y >= 1.5 to ensure they start above the ground collider. If the CC spawns partially inside geometry, it may behave erratically.

## Cursor locking (Unity 4.x API)

| Unity 5+ | Unity 4.x |
|----------|-----------|
| `Cursor.lockState = CursorLockMode.Locked` | `Screen.lockCursor = true` |
| `Cursor.visible = false` | `Screen.showCursor = false` |
| `Cursor.lockState = CursorLockMode.None` | `Screen.lockCursor = false; Screen.showCursor = true` |

## Renderer.material leak in Editor scripts

In Editor scripts (including `[MenuItem]` and batchmode helpers), always use `renderer.sharedMaterial` instead of `renderer.material`. The `material` getter clones the material at runtime, which leaks instances into the scene in edit mode and triggers the warning:

> Instantiating material due to calling renderer.material during edit mode. This will leak materials into the scene.

When multiple objects share a material and need different tiling/scale, prefer creating separate materials per object.

## Runtime vs Editor asset lifecycle

Textures created in Editor scripts via `new Texture2D()` **will not survive** into Play mode unless saved as persistent assets. Always use:

```csharp
Texture2D tex = new Texture2D(w, h);
// ... fill pixels ...
tex.Apply();
AssetDatabase.CreateAsset(tex, "Assets/Textures/MyTex.asset");
AssetDatabase.SaveAssets();
// Reload from disk to get a persistent reference
tex = AssetDatabase.LoadAssetAtPath("Assets/Textures/MyTex.asset", typeof(Texture2D)) as Texture2D;
```

Without this, Play mode will see null texture references and may crash with `NullReferenceException`.

## AssetDatabase API gaps

| Unity 5+ | Unity 4.x workaround |
|----------|----------------------|
| `AssetDatabase.IsValidFolder(path)` | `System.IO.Directory.Exists(path)` |
| `AssetDatabase.FindAssets("t:script")` | Works, but filter syntax may differ |

`System.IO.Directory.Exists()` uses the project-relative path (e.g. `"Assets/Textures"`), not the absolute filesystem path.
