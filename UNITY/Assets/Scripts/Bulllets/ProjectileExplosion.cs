using UnityEngine;

/// <summary>
/// Self-contained projectile impact VFX, generated entirely in code so it needs
/// no particle package and runs on WebGL. Spawns a bright additive flash, an
/// expanding shockwave ring, and a few flying shards, then destroys itself.
///
/// Reuses the additive material in Resources/TankGlowAdditive (same one the tank
/// glow uses), so the shader is always included in the build.
///
/// Drop this on an empty prefab and assign it to BulletController._explosionVfx;
/// BulletController.Explode() instantiates it at the impact point.
/// </summary>
[DisallowMultipleComponent]
public class ProjectileExplosion : MonoBehaviour
{
    [Header("Timing & Size")]
    [SerializeField] private float _duration = 0.4f;
    [Tooltip("Approximate blast diameter in world units.")]
    [SerializeField] private float _maxRadius = 1.6f;

    [Header("Colors (additive)")]
    [SerializeField] private Color _coreColor = new Color(1f, 0.85f, 0.45f);  // hot yellow-white
    [SerializeField] private Color _ringColor = new Color(1f, 0.45f, 0.15f);  // orange
    [SerializeField] private Color _shardColor = new Color(1f, 0.55f, 0.2f);

    [Header("Shards")]
    [SerializeField] private int _shardCount = 6;
    [SerializeField] private float _shardSpeed = 6f;

    [Header("Sorting")]
    [SerializeField] private string _sortingLayer = "Tank"; // topmost sprite layer
    [SerializeField] private int _sortingOrder = 50;

    private static Sprite _radialSprite;
    private static Sprite _ringSprite;
    private static Material _additiveMat;

    private float _elapsed;
    private SpriteRenderer _core;
    private SpriteRenderer _ring;
    private Transform[] _shardT;
    private SpriteRenderer[] _shardR;
    private Vector2[] _shardDir;
    private float[] _shardSpeeds;

    private void Awake()
    {
        EnsureAssets();

        _core = MakeRenderer("Core", _radialSprite);
        _ring = MakeRenderer("Ring", _ringSprite);

        int n = Mathf.Max(0, _shardCount);
        _shardT = new Transform[n];
        _shardR = new SpriteRenderer[n];
        _shardDir = new Vector2[n];
        _shardSpeeds = new float[n];
        for (int i = 0; i < n; i++)
        {
            var r = MakeRenderer("Shard" + i, _radialSprite);
            float ang = (360f / n) * i + Random.Range(-18f, 18f);
            float rad = ang * Mathf.Deg2Rad;
            _shardT[i] = r.transform;
            _shardR[i] = r;
            _shardDir[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            _shardSpeeds[i] = _shardSpeed * Random.Range(0.6f, 1.2f);
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;
        float fade = 1f - t;
        float easeOut = 1f - (1f - t) * (1f - t);

        // Core flash: snaps open, fades fast.
        SetRenderer(_core, Mathf.Lerp(0.4f, _maxRadius, easeOut), _coreColor, fade * fade);

        // Shockwave ring: expands wider, fades linearly.
        SetRenderer(_ring, Mathf.Lerp(0.3f, _maxRadius * 1.7f, easeOut), _ringColor, fade);

        // Shards: fly out, shrink and fade.
        for (int i = 0; i < _shardT.Length; i++)
        {
            _shardT[i].localPosition += (Vector3)(_shardDir[i] * (_shardSpeeds[i] * Time.deltaTime));
            SetRenderer(_shardR[i], Mathf.Lerp(0.5f, 0.06f, t), _shardColor, fade);
        }

        if (t >= 1f) Destroy(gameObject);
    }

    private static void SetRenderer(SpriteRenderer r, float worldDiameter, Color c, float alpha)
    {
        r.transform.localScale = Vector3.one * worldDiameter;
        c.a = Mathf.Clamp01(alpha);
        r.color = c;
    }

    private SpriteRenderer MakeRenderer(string n, Sprite sprite)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        if (_additiveMat != null) r.sharedMaterial = _additiveMat;
        if (!string.IsNullOrEmpty(_sortingLayer)) r.sortingLayerName = _sortingLayer;
        r.sortingOrder = _sortingOrder;
        return r;
    }

    private static void EnsureAssets()
    {
        if (_radialSprite == null) _radialSprite = BuildRadial(64);
        if (_ringSprite == null) _ringSprite = BuildRing(64);
        if (_additiveMat == null)
        {
            _additiveMat = Resources.Load<Material>("TankGlowAdditive");
            if (_additiveMat == null)
            {
                Shader s = Shader.Find("WarOfTanks/TankGlowAdditive");
                if (s != null) _additiveMat = new Material(s) { hideFlags = HideFlags.DontSave };
            }
        }
    }

    private static Sprite BuildRadial(int size)
    {
        var cols = new Color[size * size];
        float c = (size - 1) * 0.5f, r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a);
                cols[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        return MakeSprite(cols, size, "ExplosionRadial");
    }

    private static Sprite BuildRing(int size)
    {
        var cols = new Color[size * size];
        float c = (size - 1) * 0.5f, r = size * 0.5f;
        const float peak = 0.78f, band = 0.22f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
                float a = Mathf.Clamp01(1f - Mathf.Abs(d - peak) / band);
                a = a * a * (3f - 2f * a);
                cols[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        return MakeSprite(cols, size, "ExplosionRing");
    }

    private static Sprite MakeSprite(Color[] cols, int size, string name)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = name,
            hideFlags = HideFlags.DontSave,
        };
        tex.SetPixels(cols);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = name;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }
}
