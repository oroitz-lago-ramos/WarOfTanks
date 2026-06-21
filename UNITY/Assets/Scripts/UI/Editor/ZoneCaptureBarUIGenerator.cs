using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WarOfTanks.UI;
using F = MenuUIFactory;

/// <summary>
/// One-click generator that builds the styled zone capture bar and re-links the scene's
/// <see cref="ZoneUIController"/> <c>_captureBar</c> to it. The world zone sprite (<c>_zoneSprite</c>)
/// is left untouched. Saved to Assets/Prefabs/UI/ZoneCaptureBar.prefab.
/// Run via the menu: <b>War of Tanks ▸ Generate Zone Capture Bar UI</b>.
/// </summary>
public static class ZoneCaptureBarUIGenerator
{
    private const string PrefabPath = "Assets/Prefabs/UI/ZoneCaptureBar.prefab";
    private const string RootName = "ZoneCaptureBar";

    [MenuItem("War of Tanks/Generate Zone Capture Bar UI")]
    public static void Generate()
    {
        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Bottom-right corner: a circular radial capture gauge (kept out of the central play area).
        RectTransform box = F.CreateAnchoredBox("ZoneBox", root.transform, new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-24, 24), new Vector2(170, 150), TextAnchor.MiddleCenter, spacing: 8f);
        F.CreateLabel("Label", "ZONE CONTROL", box, 12, F.Muted, 140, 16,
            bold: true, upper: true, spacing: 3f, align: TextAlignmentOptions.Center);
        F.CreateRadialGauge(box, 88);

        // ---- Save prefab (purely visual; ZoneUIController stays in the scene) ----
        F.EnsureFolder();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab == null)
        {
            Debug.LogError("[ZoneCaptureBarUIGenerator] Failed to save prefab at " + PrefabPath);
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.name == "Game")
        {
            GameObject instance = GameObject.Find(RootName);
            if (instance == null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Zone Capture Bar");
            }

            Image fill = FindFill(instance);
            ZoneUIController zone = Object.FindObjectOfType<ZoneUIController>();
            if (zone != null && fill != null)
            {
                SerializedObject zs = new SerializedObject(zone);
                SerializedProperty prop = zs.FindProperty("_captureBar");
                if (prop != null)
                {
                    prop.objectReferenceValue = fill;
                    zs.ApplyModifiedProperties();
                }
            }
            else if (zone == null)
            {
                Debug.LogWarning("[ZoneCaptureBarUIGenerator] No ZoneUIController found in the scene; " +
                                 "wire its Capture Bar to the new Fill image manually.");
            }
            EditorSceneManager.MarkSceneDirty(active);
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[ZoneCaptureBarUIGenerator] Generated " + PrefabPath +
                  (active.name == "Game" ? " and re-linked ZoneUIController. Remove the old capture-bar object if separate." : "."));
    }

    private static Image FindFill(GameObject instance)
    {
        foreach (Image img in instance.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject.name == "Fill") return img;
        }
        return null;
    }
}
