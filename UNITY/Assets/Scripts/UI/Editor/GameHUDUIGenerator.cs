using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using F = MenuUIFactory;

/// <summary>
/// One-click generator that builds the styled in-game HUD (ally score / match timer / enemy score),
/// wires it to the existing <see cref="GameHUD"/> component, and saves it to
/// Assets/Prefabs/UI/GameHUD.prefab. If the active scene is "Game" it drops an instance in.
/// Run via the menu: <b>War of Tanks ▸ Generate Game HUD UI</b>.
/// </summary>
public static class GameHUDUIGenerator
{
    private const string PrefabPath = "Assets/Prefabs/UI/GameHUD.prefab";

    [MenuItem("War of Tanks/Generate Game HUD UI")]
    public static void Generate()
    {
        GameObject root = new GameObject("GameHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // above the game, below the game-over screen
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameHUD hud = root.AddComponent<GameHUD>();

        // Ally score (top-left)
        RectTransform ally = F.CreateAnchoredBox("AllyPanel", root.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -20), new Vector2(190, 76), TextAnchor.MiddleLeft);
        F.CreateLabel("Label", "ALLY", ally, 12, F.Accent, 160, 16, bold: true, upper: true, spacing: 2f, align: TextAlignmentOptions.Left);
        TextMeshProUGUI scoreA = F.CreateLabel("Score", "0", ally, 32, F.Accent, 160, 36, bold: true, upper: false, spacing: 0f, align: TextAlignmentOptions.Left);

        // Match timer (top-center)
        RectTransform timer = F.CreateAnchoredBox("TimerPanel", root.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(220, 76), TextAnchor.MiddleCenter);
        F.CreateLabel("Label", "MATCH TIME", timer, 12, F.Muted, 190, 16, bold: true, upper: true, spacing: 3f, align: TextAlignmentOptions.Center);
        TextMeshProUGUI timerText = F.CreateLabel("Timer", "03:00", timer, 32, F.Text, 190, 36, bold: true, upper: false, spacing: 1f, align: TextAlignmentOptions.Center);

        // Enemy score (top-right)
        RectTransform enemy = F.CreateAnchoredBox("EnemyPanel", root.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -20), new Vector2(190, 76), TextAnchor.MiddleRight);
        F.CreateLabel("Label", "ENEMY", enemy, 12, F.Loss, 160, 16, bold: true, upper: true, spacing: 2f, align: TextAlignmentOptions.Right);
        TextMeshProUGUI scoreB = F.CreateLabel("Score", "0", enemy, 32, F.Loss, 160, 36, bold: true, upper: false, spacing: 0f, align: TextAlignmentOptions.Right);

        // ---- Wire GameHUD references ----
        SerializedObject so = new SerializedObject(hud);
        F.SetRef(so, "_scoreTeamAText", scoreA);
        F.SetRef(so, "_scoreTeamBText", scoreB);
        F.SetRef(so, "_timerText", timerText);
        so.ApplyModifiedPropertiesWithoutUndo();

        // ---- Save prefab ----
        F.EnsureFolder();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab == null)
        {
            Debug.LogError("[GameHUDUIGenerator] Failed to save prefab at " + PrefabPath);
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.name == "Game")
        {
            // Only add if an instance of THIS prefab isn't already present (ignores the old HUD object).
            bool hasInstance = false;
            foreach (GameHUD existing in Object.FindObjectsOfType<GameHUD>())
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(existing.gameObject) as GameObject == prefab)
                {
                    hasInstance = true;
                    break;
                }
            }
            if (!hasInstance)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Game HUD");
            }
            EditorSceneManager.MarkSceneDirty(active);
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[GameHUDUIGenerator] Generated " + PrefabPath +
                  (active.name == "Game" ? ". Delete the old GameHUD object from the scene." : "."));
    }
}
