using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a real alpha channel to a flattened sheet whose "transparent" background
/// was baked in as a light checkerboard (no alpha). Flood-fills the bright,
/// near-grayscale background that is connected to the texture borders and sets
/// it to alpha = 0. Connectivity means interior frame highlights are never
/// touched, so only the surrounding background is removed.
///
/// Menu: WarOfTanks > Textures > Bake Background -> Transparent (selected).
/// Select the PNG in the Project window first. The original is backed up once
/// as &lt;name&gt;_original.png next to it.
/// </summary>
public static class TransparencyBaker
{
    // A pixel counts as background if it is bright AND nearly grayscale.
    // Raise BrightnessCutoff if gray metal frames get eaten; lower it if some
    // checkerboard survives.
    private const int BrightnessCutoff = 170; // max channel, 0..255
    private const int ChromaCutoff     = 45;  // (max - min) channel spread

    [MenuItem("WarOfTanks/Textures/Bake Background -> Transparent (selected)")]
    public static void BakeSelected()
    {
        var tex2d = Selection.activeObject as Texture2D;
        if (tex2d == null)
        {
            EditorUtility.DisplayDialog("Bake Transparency",
                "Select the PNG texture in the Project window first.", "OK");
            return;
        }
        Bake(AssetDatabase.GetAssetPath(tex2d));
    }

    public static void Bake(string relPath)
    {
        string absPath = Path.GetFullPath(relPath);
        byte[] bytes = File.ReadAllBytes(absPath);

        // Decode raw RGB(A) straight from disk (ignores importer compression/readable).
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(bytes))
        {
            Debug.LogError($"[Baker] Could not decode '{relPath}'.");
            return;
        }

        int w = tex.width, h = tex.height;
        var px = tex.GetPixels32();

        bool IsBg(int i)
        {
            var c = px[i];
            int mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            int mn = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            return mx >= BrightnessCutoff && (mx - mn) <= ChromaCutoff;
        }

        var visited = new bool[px.Length];
        var stack = new Stack<int>(1 << 16);

        // Seed from every border pixel that looks like background.
        for (int x = 0; x < w; x++)
        {
            Seed(x, stack, visited, IsBg);                 // bottom row
            Seed((h - 1) * w + x, stack, visited, IsBg);   // top row
        }
        for (int y = 0; y < h; y++)
        {
            Seed(y * w, stack, visited, IsBg);             // left col
            Seed(y * w + (w - 1), stack, visited, IsBg);   // right col
        }

        int cleared = 0;
        while (stack.Count > 0)
        {
            int i = stack.Pop();
            var c = px[i];
            c.a = 0;
            px[i] = c;
            cleared++;

            int x = i % w, y = i / w;
            if (x > 0)     TryPush(i - 1, stack, visited, IsBg);
            if (x < w - 1) TryPush(i + 1, stack, visited, IsBg);
            if (y > 0)     TryPush(i - w, stack, visited, IsBg);
            if (y < h - 1) TryPush(i + w, stack, visited, IsBg);
        }

        tex.SetPixels32(px);
        tex.Apply();

        // Back up the original once (outside re-runs won't overwrite it).
        string backup = Path.Combine(Path.GetDirectoryName(absPath),
                                     Path.GetFileNameWithoutExtension(absPath) + "_original.png");
        if (!File.Exists(backup)) File.Copy(absPath, backup);

        File.WriteAllBytes(absPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(relPath) as TextureImporter;
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log($"[Baker] '{relPath}': cleared {cleared} background px to transparent. " +
                  $"Backup saved as '{Path.GetFileName(backup)}'. Slicing is preserved.");
    }

    private static void Seed(int i, Stack<int> stack, bool[] visited, System.Func<int, bool> isBg)
    {
        if (!visited[i] && isBg(i)) { visited[i] = true; stack.Push(i); }
    }

    private static void TryPush(int i, Stack<int> stack, bool[] visited, System.Func<int, bool> isBg)
    {
        if (!visited[i] && isBg(i)) { visited[i] = true; stack.Push(i); }
    }
}
