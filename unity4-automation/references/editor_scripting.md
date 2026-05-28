# Unity 4.x Editor Scripting Reference

## Entry points

### MenuItem

```csharp
using UnityEditor;

public class MyTools
{
    [MenuItem("Tools/My Tool %t")]  // Ctrl+T shortcut
    static void DoSomething()
    {
        Debug.Log("Manual trigger from menu");
    }

    [MenuItem("Assets/Process Selected")]
    static void ProcessSelected()
    {
        foreach (Object obj in Selection.objects)
        {
            Debug.Log("Selected: " + AssetDatabase.GetAssetPath(obj));
        }
    }
}
```

### AssetPostprocessor

Hook into asset import pipeline:

```csharp
public class MyPostprocessor : AssetPostprocessor
{
    // Called before any texture import
    void OnPreprocessTexture()
    {
        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Advanced;
        ti.textureFormat = TextureImporterFormat.AutomaticCompressed;
        ti.maxTextureSize = 512;
        ti.mipmapEnabled = false;
    }

    void OnPreprocessModel()
    {
        ModelImporter mi = (ModelImporter)assetImporter;
        mi.importMaterials = false;
        mi.globalScale = 1.0f;
    }

    void OnPreprocessAudio()
    {
        AudioImporter ai = (AudioImporter)assetImporter;
        ai.threeD = false;
        ai.forceToMono = true;
        ai.format = AudioImporterFormat.Compressed;
    }

    // Called after asset import completes
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            Debug.Log("Imported: " + path);
        }
    }
}
```

### EditorWindow

```csharp
using UnityEditor;
using UnityEngine;

public class BatchProcessor : EditorWindow
{
    [MenuItem("Window/Batch Processor")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<BatchProcessor>("Batch Process");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Run Batch"))
        {
            BatchOperation();
        }
    }

    void BatchOperation()
    {
        // Your automation logic here
    }
}
```

## AssetDatabase APIs

### Finding assets

```csharp
// By type filter (Unity 4.x supported)
string[] guids = AssetDatabase.FindAssets("t:texture2D");
foreach (string guid in guids)
{
    string path = AssetDatabase.GUIDToAssetPath(guid);
    Debug.Log(path);
}

// All paths in folder
string[] allAssets = AssetDatabase.FindAssets("", new[] { "Assets/Textures" });
```

### Import / Refresh

```csharp
AssetDatabase.Refresh();                  // Full refresh
AssetDatabase.ImportAsset(path);          // Single asset
AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
AssetDatabase.SaveAssets();               // Save all dirty assets
```

### Create / Copy / Move / Delete

```csharp
// Copy
AssetDatabase.CopyAsset(sourcePath, destPath);

// Move
AssetDatabase.MoveAsset(oldPath, newPath);

// Delete
AssetDatabase.DeleteAsset(path);

// Create folder
AssetDatabase.CreateFolder("Assets", "NewFolder");

// Create material (when calling CreateAsset, asset must exist in memory)
Material mat = new Material(Shader.Find("Diffuse"));
AssetDatabase.CreateAsset(mat, "Assets/MyMaterial.mat");
```

### Asset modification (use SerializedObject)

```csharp
GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
SerializedObject so = new SerializedObject(go);
so.FindProperty("m_IsActive").boolValue = false;
so.ApplyModifiedProperties();
EditorUtility.SetDirty(go);
AssetDatabase.SaveAssets();
```

## TextureImporter settings

```csharp
string path = AssetDatabase.GetAssetPath(texture);
TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;

ti.textureType = TextureImporterType.Advanced;
ti.textureFormat = TextureImporterFormat.ARGB32;
ti.maxTextureSize = 256;
ti.mipmapEnabled = false;
ti.isReadable = false;
ti.wrapMode = TextureWrapMode.Clamp;
ti.filterMode = FilterMode.Bilinear;
ti.anisoLevel = 1;

AssetDatabase.ImportAsset(path);
```

## Bulk operations pattern

```csharp
[MenuItem("Tools/Batch Set Texture Max Size 512")]
static void BatchSetTextureSize()
{
    string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { "Assets/Textures" });

    foreach (string guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;

        if (ti.maxTextureSize > 512)
        {
            ti.maxTextureSize = 512;
            AssetDatabase.ImportAsset(path);
        }
    }

    AssetDatabase.Refresh();
    Debug.Log("Done processing " + guids.Length + " textures.");
}
```

## EditorUserBuildSettings (Unity 4.x)

```csharp
// Scene list management
EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
{
    new EditorBuildSettingsScene("Assets/Scenes/Start.unity", true),
    new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
};

// Platform switch
EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

// Player settings (partial)
PlayerSettings.productName = "My Game";
PlayerSettings.bundleIdentifier = "com.example.mygame";
```

## executeMethod pattern for batchmode

Methods invoked via `-executeMethod` must:
- Be `public static` (or just `static` in a non-public class)
- Take no parameters
- Be in a file under `Assets/Editor/`

```csharp
using UnityEditor;

public static class BuildScripts
{
    static void BuildWindows()
    {
        BuildPlayer("Builds/Windows/Game.exe", BuildTarget.StandaloneWindows);
    }

    static void BuildAndroid()
    {
        BuildPlayer("Builds/Android/Game.apk", BuildTarget.Android);
    }

    static void BuildAll()
    {
        BuildWindows();
        BuildAndroid();
    }

    static void BuildPlayer(string outputPath, BuildTarget target)
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        BuildPipeline.BuildPlayer(scenes, outputPath, target, BuildOptions.None);
    }
}
```



## Creating .cs files from external tools

When generating `.cs` files outside Unity (e.g. from PowerShell, Python, or CI scripts), **always use CRLF line endings and UTF-8 without BOM**. Unity 4.x will reject files with mixed line endings:

> There are inconsistent line endings in the '...' script. Some are Mac OS X (UNIX) and some are Windows.

### Correct PowerShell pattern

```powershell
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$writer = New-Object System.IO.StreamWriter("Assets/Scripts/MyScript.cs", $false, $utf8NoBom)
$writer.NewLine = "`r`n"
$writer.WriteLine("using UnityEngine;")
$writer.WriteLine("")
$writer.WriteLine("public class MyScript : MonoBehaviour { }")
$writer.Close()
```

Do **not** use `Set-Content -Encoding UTF8` (adds BOM) or `@'...'@ | Set-Content` (may produce mixed LF/CRLF in here-strings).

### Fixing existing files

```powershell
$content = [System.IO.File]::ReadAllText($file)
$content = $content -replace "`r`n", "`n" -replace "`n", "`r`n"
[System.IO.File]::WriteAllText($file, $content)
```

Also delete the corresponding `.meta` file so Unity re-imports cleanly.

## Procedural texture assets

When creating textures programmatically in an Editor script for use at runtime, they must be saved as `.asset` files:

```csharp
Texture2D tex = new Texture2D(256, 256);
for (int y = 0; y < 256; y++)
    for (int x = 0; x < 256; x++)
        tex.SetPixel(x, y, Color.gray);
tex.Apply();

// Save to disk
string path = "Assets/Textures/GeneratedTexture.asset";
AssetDatabase.CreateAsset(tex, path);
AssetDatabase.SaveAssets();

// Reload for a persistent reference
tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
```

Then assign to materials via `material.mainTexture = tex;`. The texture will persist through Play mode and domain reloads.
