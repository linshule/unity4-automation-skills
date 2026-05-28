// Unity 4.6.8 Editor Script: Batch Texture Format Processor
// Place under Assets/Editor/ in your project.
//
// Usage from menu: Tools > Batch > Set Texture Max Size...
// Usage from batchmode: Unity.exe -batchmode -quit -projectPath . -executeMethod BatchTextureProcessor.SetAllTextures256

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BatchTextureProcessor
{
    [MenuItem("Tools/Batch/Set All Textures Max Size 256")]
    static void SetAllTextures256()
    {
        BatchSetTextureMaxSize(256);
    }

    [MenuItem("Tools/Batch/Set All Textures Max Size 512")]
    static void SetAllTextures512()
    {
        BatchSetTextureMaxSize(512);
    }

    [MenuItem("Tools/Batch/Disable Mipmaps on All Textures")]
    static void DisableAllMipmaps() { BatchSetTextureMipmaps(false); }

    [MenuItem("Tools/Batch/Set All Textures to TrueColor")]
    static void SetAllToTrueColor() { BatchSetTextureFormat(TextureImporterFormat.RGBA32); }

    [MenuItem("Tools/Batch/Set All Textures to Compressed")]
    static void SetAllToCompressed() { BatchSetTextureFormat(TextureImporterFormat.AutomaticCompressed); }

    static void BatchSetTextureMaxSize(int maxSize)
    {
        string[] guids = AssetDatabase.FindAssets("t:texture2D");
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && ti.maxTextureSize != maxSize)
            {
                ti.maxTextureSize = maxSize;
                ti.textureType = TextureImporterType.Advanced;
                AssetDatabase.ImportAsset(path);
                changed++;
            }
        }

        Debug.Log(string.Format("BatchTextureProcessor: Set maxSize={0} on {1}/{2} textures.", maxSize, changed, guids.Length));
    }

    static void BatchSetTextureMipmaps(bool enabled)
    {
        string[] guids = AssetDatabase.FindAssets("t:texture2D");
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && ti.mipmapEnabled != enabled)
            {
                ti.mipmapEnabled = enabled;
                AssetDatabase.ImportAsset(path);
                changed++;
            }
        }

        Debug.Log(string.Format("BatchTextureProcessor: mipmaps={0} on {1}/{2} textures.", enabled, changed, guids.Length));
    }

    static void BatchSetTextureFormat(TextureImporterFormat format)
    {
        string[] guids = AssetDatabase.FindAssets("t:texture2D");
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && ti.textureFormat != format)
            {
                ti.textureFormat = format;
                AssetDatabase.ImportAsset(path);
                changed++;
            }
        }

        Debug.Log(string.Format("BatchTextureProcessor: format={0} on {1}/{2} textures.", format.ToString(), changed, guids.Length));
    }
}
