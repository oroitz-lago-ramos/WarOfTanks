using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A tile that deterministically picks one sprite from a set based on the cell
/// position, giving a varied sci-fi floor from a single sliced sheet without
/// hand-placing each panel. The choice is stable across frames/sessions because
/// it is hashed from the cell coordinates (not random per draw).
/// </summary>
[CreateAssetMenu(fileName = "RandomFloorTile", menuName = "WarOfTanks/Random Floor Tile")]
public class RandomFloorTile : TileBase
{
    [Tooltip("Candidate sprites (the 36 sliced floor panels). One is chosen per cell.")]
    public Sprite[] sprites;

    [Tooltip("Tint applied to every panel. Keep white to show the art unmodified.")]
    public Color color = Color.white;

    [Tooltip("Collider generated for each cell. Ground_Walkable must use Grid so the " +
             "Tilemap Collider 2D produces a walkable region for NavigationGrid's OverlapCircle check.")]
    public Tile.ColliderType colliderType = Tile.ColliderType.Grid;

    [Tooltip("Scale each sprite so it fits within one cell. Enable when sprites have " +
             "uneven sizes (e.g. tightly-sliced shapes) to stop tiles overlapping neighbours.")]
    public bool fitToCell = false;

    [Tooltip("Cell size in world units the sprites are fitted to (Grid cell size; usually 1).")]
    public float cellSize = 1f;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.color = color;
        tileData.colliderType = colliderType;
        tileData.flags = TileFlags.LockColor | TileFlags.LockTransform;
        tileData.transform = Matrix4x4.identity;

        if (sprites == null || sprites.Length == 0)
            return;

        var sprite = sprites[IndexFor(position)];
        tileData.sprite = sprite;

        if (fitToCell && sprite != null)
        {
            Vector2 size = sprite.bounds.size; // world units at the sprite's PPU
            float maxDim = Mathf.Max(size.x, size.y);
            if (maxDim > 0f)
            {
                float s = cellSize / maxDim; // largest dimension fills exactly one cell
                tileData.transform = Matrix4x4.Scale(new Vector3(s, s, 1f));
            }
        }
    }

    private int IndexFor(Vector3Int position)
    {
        // Stable spatial hash (no per-frame randomness).
        unchecked
        {
            uint h = (uint)(position.x * 73856093) ^ (uint)(position.y * 19349663);
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;
            return (int)(h % (uint)sprites.Length);
        }
    }
}
