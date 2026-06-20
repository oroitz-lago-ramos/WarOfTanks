using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Editor automation that turns a sliced sci-fi sheet into a varied tilemap
/// (RandomFloorTile) and swaps it into a target tilemap, preserving the layout.
/// Shared by the walkable floor and the unwalkable terrain via <see cref="TileSet"/>.
///
/// Pipeline per set:
///   1. Slice    - grid-slices the sheet into sprites (skip if you sliced manually).
///   2. Tile     - (re)creates the RandomFloorTile from whatever sprites the sheet has.
///   3. Apply    - replaces the old tile in the named tilemap and refreshes colliders.
///
/// Targets Unity 2020.3 (uses TextureImporter.spritesheet for slicing).
/// </summary>
public static class SciFiFloorTool
{
    /// <summary>Describes one art set and where it gets applied.</summary>
    private struct TileSet
    {
        public string Label;            // for menu logs
        public string SpritePrefix;     // sliced sprite name prefix
        public string TexturePath;
        public string TilePath;         // generated RandomFloorTile asset
        public string TilemapObject;    // scene GameObject holding the Tilemap
        public string OldTilePath;      // tile to replace within that tilemap
        public Tile.ColliderType Collider;
        public bool FitToCell;          // scale uneven sprites to one cell (scales collider too!)
        public bool NormalizePpu;       // set PPU = largest sprite px so sprites fit a cell w/o transform
        public int Cols, Rows;
    }

    private static readonly TileSet Floor = new TileSet
    {
        Label = "Floor",
        SpritePrefix = "Floor",
        TexturePath = "Assets/Images/Ground_Walkable.png",
        TilePath = "Assets/Tilemaps/Tiles/SciFiFloor_Random.asset",
        TilemapObject = "Ground_Walkable",
        OldTilePath = "Assets/Tilemaps/Tiles/Tile_Ground.asset",
        Collider = Tile.ColliderType.Grid, // full-cell walkable region for NavigationGrid
        FitToCell = false,                 // floor sprites are already exactly one cell
        NormalizePpu = false,              // floor PPU is already correct
        Cols = 6, Rows = 6,
    };

    private static readonly TileSet Terrain = new TileSet
    {
        Label = "Terrain",
        SpritePrefix = "Terrain",
        TexturePath = "Assets/Images/Terrain_Unwalkable.png",
        TilePath = "Assets/Tilemaps/Tiles/SciFiTerrain_Random.asset",
        TilemapObject = "Terrain_Unwalkable",
        OldTilePath = "Assets/Tilemaps/Tiles/Tile_Wall.asset",
        Collider = Tile.ColliderType.Grid, // full-cell block on the Unwalkable layer
        FitToCell = false,                 // don't scale via transform (would shrink the collider)
        NormalizePpu = true,               // fit your manual tight slices into a cell via PPU instead
        Cols = 5, Rows = 5,
    };

    private static readonly TileSet Obstacle = new TileSet
    {
        Label = "Obstacle",
        SpritePrefix = "Obstacle",
        TexturePath = "Assets/Images/Obstacles.png",
        TilePath = "Assets/Tilemaps/Tiles/SciFiObstacle_Random.asset",
        TilemapObject = "Obstacles",
        OldTilePath = "Assets/Tilemaps/Tiles/Tile_Obstacle.asset",
        Collider = Tile.ColliderType.Grid, // full-cell block (Obstacle layer)
        FitToCell = false,                 // don't transform-scale (it scales the collider too)
        NormalizePpu = true,               // fill the cell via PPU so the Grid collider stays full-cell
        Cols = 4, Rows = 4,
    };

    private static readonly TileSet Cover = new TileSet
    {
        Label = "Cover",
        SpritePrefix = "Cover",
        TexturePath = "Assets/Images/Cover.png",
        TilePath = "Assets/Tilemaps/Tiles/SciFiCover_Random.asset",
        TilemapObject = "Cover",
        OldTilePath = "Assets/Tilemaps/Tiles/Tile_Cover.asset",
        Collider = Tile.ColliderType.Grid, // full-cell block (Cover layer)
        FitToCell = false,                 // don't transform-scale (it scales the collider too)
        NormalizePpu = true,               // fill the cell via PPU so the Grid collider stays full-cell
        Cols = 4, Rows = 4,
    };

    private static readonly TileSet Hazard = new TileSet
    {
        Label = "Hazard",
        SpritePrefix = "Hazard",
        TexturePath = "Assets/Images/Hazard.png",
        TilePath = "Assets/Tilemaps/Tiles/SciFiHazard_Random.asset",
        TilemapObject = "Ground_Hazard",
        OldTilePath = "Assets/Tilemaps/Tiles/Tile_Hazard.asset",
        Collider = Tile.ColliderType.Grid, // full-cell collider so NavigationGrid detects the Hazard layer
        FitToCell = false,                 // don't transform-scale (it scales the collider too)
        NormalizePpu = true,               // fill the cell via PPU so the Grid collider stays full-cell
        Cols = 5, Rows = 5,
    };

    // ----------------------------------------------------------------- Floor menu
    [MenuItem("WarOfTanks/Sci-Fi Floor/Do Everything (Slice + Tile + Apply)")]
    public static void FloorAll() { if (Slice(Floor)) { CreateTile(Floor); Apply(Floor); } }

    [MenuItem("WarOfTanks/Sci-Fi Floor/1. Slice Sheet (6x6)")]
    public static void FloorSlice() => Slice(Floor);

    [MenuItem("WarOfTanks/Sci-Fi Floor/2. Create or Update Random Tile")]
    public static void FloorTile() => CreateTile(Floor);

    [MenuItem("WarOfTanks/Sci-Fi Floor/3. Apply To Ground_Walkable (open scene)")]
    public static void FloorApply() => Apply(Floor);

    // --------------------------------------------------------------- Terrain menu
    [MenuItem("WarOfTanks/Sci-Fi Terrain/Do Everything (Slice + Tile + Apply)")]
    public static void TerrainAll() { if (Slice(Terrain)) { CreateTile(Terrain); Apply(Terrain); } }

    [MenuItem("WarOfTanks/Sci-Fi Terrain/1. Slice Sheet (5x5)")]
    public static void TerrainSlice() => Slice(Terrain);

    [MenuItem("WarOfTanks/Sci-Fi Terrain/2. Create or Update Random Tile")]
    public static void TerrainTile() => CreateTile(Terrain);

    [MenuItem("WarOfTanks/Sci-Fi Terrain/3. Apply To Terrain_Unwalkable (open scene)")]
    public static void TerrainApply() => Apply(Terrain);

    // -------------------------------------------------------------- Obstacle menu
    [MenuItem("WarOfTanks/Sci-Fi Obstacle/Do Everything (Slice + Tile + Apply)")]
    public static void ObstacleAll() { if (Slice(Obstacle)) { CreateTile(Obstacle); Apply(Obstacle); } }

    [MenuItem("WarOfTanks/Sci-Fi Obstacle/1. Slice Sheet (4x4)")]
    public static void ObstacleSlice() => Slice(Obstacle);

    [MenuItem("WarOfTanks/Sci-Fi Obstacle/2. Create or Update Random Tile")]
    public static void ObstacleTile() => CreateTile(Obstacle);

    [MenuItem("WarOfTanks/Sci-Fi Obstacle/3. Apply To Obstacles (open scene)")]
    public static void ObstacleApply() => Apply(Obstacle);

    // ----------------------------------------------------------------- Cover menu
    [MenuItem("WarOfTanks/Sci-Fi Cover/Do Everything (Slice + Tile + Apply)")]
    public static void CoverAll() { if (Slice(Cover)) { CreateTile(Cover); Apply(Cover); } }

    [MenuItem("WarOfTanks/Sci-Fi Cover/1. Slice Sheet (4x4)")]
    public static void CoverSlice() => Slice(Cover);

    [MenuItem("WarOfTanks/Sci-Fi Cover/2. Create or Update Random Tile")]
    public static void CoverTile() => CreateTile(Cover);

    [MenuItem("WarOfTanks/Sci-Fi Cover/3. Apply To Cover (open scene)")]
    public static void CoverApply() => Apply(Cover);

    // ---------------------------------------------------------------- Hazard menu
    [MenuItem("WarOfTanks/Sci-Fi Hazard/Do Everything (Slice + Tile + Apply)")]
    public static void HazardAll() { if (Slice(Hazard)) { CreateTile(Hazard); Apply(Hazard); } }

    [MenuItem("WarOfTanks/Sci-Fi Hazard/1. Slice Sheet (5x5)")]
    public static void HazardSlice() => Slice(Hazard);

    [MenuItem("WarOfTanks/Sci-Fi Hazard/2. Create or Update Random Tile")]
    public static void HazardTile() => CreateTile(Hazard);

    [MenuItem("WarOfTanks/Sci-Fi Hazard/3. Apply To Ground_Hazard (open scene)")]
    public static void HazardApply() => Apply(Hazard);

    // -------------------------------------------------------------------- core
    private static bool Slice(TileSet set)
    {
        var importer = AssetImporter.GetAtPath(set.TexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[SciFi/{set.Label}] Texture not found at '{set.TexturePath}'.");
            return false;
        }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(set.TexturePath);
        int texW = tex.width, texH = tex.height;
        int cw = texW / set.Cols, ch = texH / set.Rows;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = cw; // one cell == one world unit
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;

        var metas = new List<SpriteMetaData>(set.Cols * set.Rows);
        for (int r = 0; r < set.Rows; r++)
        {
            for (int c = 0; c < set.Cols; c++)
            {
                metas.Add(new SpriteMetaData
                {
                    name = $"{set.SpritePrefix}_{r}_{c}",
                    rect = new Rect(c * cw, texH - (r + 1) * ch, cw, ch), // flip rows: 0 = top
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero,
                });
            }
        }

#pragma warning disable 618 // SpriteMetaData/spritesheet is the supported path on 2020.3
        importer.spritesheet = metas.ToArray();
#pragma warning restore 618

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        Debug.Log($"[SciFi/{set.Label}] Sliced into {set.Cols * set.Rows} sprites ({cw}x{ch}px, PPU {cw}).");
        return true;
    }

    private static void CreateTile(TileSet set)
    {
        var sprites = LoadSlicedSprites(set.TexturePath);
        if (sprites.Length == 0)
        {
            Debug.LogError($"[SciFi/{set.Label}] No sliced sprites at '{set.TexturePath}' nor on the " +
                           "selected Project texture. Slice the sheet (Sprite Mode = Multiple) first, " +
                           "or select the sliced texture in the Project window and re-run.");
            return;
        }

        // Make the biggest manual slice render at exactly one cell (others sit inside,
        // smaller). This avoids scaling the tile transform, so the Grid collider stays
        // full-cell. PPU is per-texture, so this normalises all slices uniformly.
        if (set.NormalizePpu)
        {
            float maxPx = sprites.Max(s => Mathf.Max(s.rect.width, s.rect.height));
            var importer = AssetImporter.GetAtPath(set.TexturePath) as TextureImporter;
            if (importer != null && maxPx > 0f &&
                Mathf.Abs(importer.spritePixelsPerUnit - maxPx) > 0.5f)
            {
                importer.spritePixelsPerUnit = maxPx;
                importer.SaveAndReimport();
                sprites = LoadSlicedSprites(set.TexturePath); // reload after reimport
                Debug.Log($"[SciFi/{set.Label}] Set PixelsPerUnit = {maxPx:0} (largest slice) so tiles fit one cell.");
            }
        }

        var tile = AssetDatabase.LoadAssetAtPath<RandomFloorTile>(set.TilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<RandomFloorTile>();
            AssetDatabase.CreateAsset(tile, set.TilePath);
        }

        tile.sprites = sprites;
        tile.color = Color.white;
        tile.colliderType = set.Collider;
        tile.fitToCell = set.FitToCell;
        tile.cellSize = 1f;

        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();
        Debug.Log($"[SciFi/{set.Label}] Tile updated with {sprites.Length} sprites at '{set.TilePath}'.");
    }

    private static void Apply(TileSet set)
    {
        var randomTile = AssetDatabase.LoadAssetAtPath<RandomFloorTile>(set.TilePath);
        if (randomTile == null)
        {
            Debug.LogError($"[SciFi/{set.Label}] Tile not found at '{set.TilePath}'. Run step 2 first.");
            return;
        }
        var oldTile = AssetDatabase.LoadAssetAtPath<TileBase>(set.OldTilePath);

        var maps = Object.FindObjectsOfType<Tilemap>()
            .Where(t => t.gameObject.name == set.TilemapObject)
            .ToArray();
        if (maps.Length == 0)
        {
            Debug.LogError($"[SciFi/{set.Label}] No active '{set.TilemapObject}' Tilemap in the open scene.");
            return;
        }

        int replaced = 0;
        foreach (var map in maps)
        {
            Undo.RegisterCompleteObjectUndo(map, $"Apply Sci-Fi {set.Label}");
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                var current = map.GetTile(pos);
                if (current == null) continue;
                if (oldTile == null || current == oldTile)
                {
                    map.SetTile(pos, randomTile);
                    replaced++;
                }
            }
            map.RefreshAllTiles();
            EditorUtility.SetDirty(map);
        }
        Debug.Log($"[SciFi/{set.Label}] Replaced {replaced} cell(s) in '{set.TilemapObject}'.");
    }

    // ---------------------------------------------------------------- helpers
    /// <summary>Loads sliced sprites from a texture; falls back to the selected asset if the path is empty.</summary>
    private static Sprite[] LoadSlicedSprites(string texturePath)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name, NaturalComparer.Instance)
            .ToArray();

        if (sprites.Length == 0 && Selection.activeObject != null)
        {
            string selPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            sprites = AssetDatabase.LoadAllAssetsAtPath(selPath)
                .OfType<Sprite>()
                .OrderBy(s => s.name, NaturalComparer.Instance)
                .ToArray();
            if (sprites.Length > 0)
                Debug.Log($"[SciFi] Using sprites from selected asset '{selPath}'.");
        }
        return sprites;
    }

    /// <summary>Sorts X_0_2 before X_0_10 (numeric-aware) so the spatial hash is stable.</summary>
    private class NaturalComparer : IComparer<string>
    {
        public static readonly NaturalComparer Instance = new NaturalComparer();
        public int Compare(string a, string b)
        {
            int ia = 0, ib = 0;
            while (ia < a.Length && ib < b.Length)
            {
                if (char.IsDigit(a[ia]) && char.IsDigit(b[ib]))
                {
                    int na = 0, nb = 0;
                    while (ia < a.Length && char.IsDigit(a[ia])) na = na * 10 + (a[ia++] - '0');
                    while (ib < b.Length && char.IsDigit(b[ib])) nb = nb * 10 + (b[ib++] - '0');
                    if (na != nb) return na - nb;
                }
                else
                {
                    if (a[ia] != b[ib]) return a[ia] - b[ib];
                    ia++; ib++;
                }
            }
            return (a.Length - ia) - (b.Length - ib);
        }
    }
}
