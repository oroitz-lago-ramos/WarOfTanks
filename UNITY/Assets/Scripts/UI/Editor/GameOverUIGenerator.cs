using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using F = MenuUIFactory;

/// <summary>
/// One-click generator that builds the styled end-of-match UI, wires it to the existing
/// <see cref="GameOverScreen"/> component (panel + winner/score texts + Play Again / Main Menu
/// buttons) and saves it to Assets/Prefabs/UI/GameOverUI.prefab. If the active scene is "Game" it
/// drops an instance in, ensures an EventSystem, and re-links GameManager's reference to it.
/// Run via the menu: <b>War of Tanks ▸ Generate Game Over UI</b>.
/// </summary>
public static class GameOverUIGenerator
{
    private const string PrefabPath = "Assets/Prefabs/UI/GameOverUI.prefab";
    private const string RootName = "GameOverUI";

    [MenuItem("War of Tanks/Generate Game Over UI")]
    public static void Generate()
    {
        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // draw above the in-game HUD
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameOverScreen screen = root.AddComponent<GameOverScreen>();

        // Dark scrim overlay (this is the toggled _panel).
        GameObject panel = F.CreatePanel("Panel", root.transform, new Color(F.Bg.r, F.Bg.g, F.Bg.b, 0.92f), 0f);

        RectTransform card = F.CreateCard("GameOverCard", panel.transform, 560, 400, 16f);
        F.CreateLabel("Eyebrow", "MATCH OVER", card, 13, F.Muted, 480, 22,
            bold: true, upper: true, spacing: 4f, align: TextAlignmentOptions.Center);
        TextMeshProUGUI winnerText = F.CreateLabel("WinnerText", "Game Over", card, 40, F.Text, 480, 56,
            bold: true, upper: false, spacing: 0f, align: TextAlignmentOptions.Center);

        RectTransform scoreRow = F.NewRect("ScoreRow", card);
        F.AddLayoutElement(scoreRow.gameObject, 480, 40);
        F.ConfigureLayout(scoreRow.gameObject.AddComponent<HorizontalLayoutGroup>(), 36f, TextAnchor.MiddleCenter);
        TextMeshProUGUI scoreA = F.CreateLabel("ScoreTeamA", "Player: 0", scoreRow, 22, F.Accent, 200, 34,
            bold: true, upper: false, spacing: 0f, align: TextAlignmentOptions.Right);
        TextMeshProUGUI scoreB = F.CreateLabel("ScoreTeamB", "Enemy: 0", scoreRow, 22, F.Loss, 200, 34,
            bold: true, upper: false, spacing: 0f, align: TextAlignmentOptions.Left);

        F.Spacer(card, 8);
        Button playAgainButton = F.CreateButton("PlayAgainButton", "Play Again", card, primary: true);
        Button mainMenuButton = F.CreateButton("MainMenuButton", "Main Menu", card, primary: false);

        // ---- Wire the GameOverScreen references + button actions ----
        SerializedObject so = new SerializedObject(screen);
        F.SetRef(so, "_panel", panel);
        F.SetRef(so, "_winnerText", winnerText);
        F.SetRef(so, "_scoreTeamAText", scoreA);
        F.SetRef(so, "_scoreTeamBText", scoreB);
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(playAgainButton.onClick, new UnityAction(screen.PlayAgain));
        UnityEventTools.AddPersistentListener(mainMenuButton.onClick, new UnityAction(screen.GoToMainMenu));

        panel.SetActive(false); // hidden until GameManager calls Show()

        // ---- Save prefab ----
        F.EnsureFolder();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab == null)
        {
            Debug.LogError("[GameOverUIGenerator] Failed to save prefab at " + PrefabPath);
            return;
        }

        // ---- Drop into the Game scene and re-link GameManager ----
        Scene active = SceneManager.GetActiveScene();
        if (active.name == "Game")
        {
            F.EnsureEventSystem();
            GameObject instance = GameObject.Find(RootName);
            if (instance == null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Game Over UI");
            }

            GameManager gm = Object.FindObjectOfType<GameManager>();
            GameOverScreen newScreen = instance.GetComponent<GameOverScreen>();
            if (gm != null && newScreen != null)
            {
                SerializedObject gmSo = new SerializedObject(gm);
                SerializedProperty prop = gmSo.FindProperty("_gameOverScreen");
                if (prop != null)
                {
                    prop.objectReferenceValue = newScreen;
                    gmSo.ApplyModifiedProperties();
                }
            }
            EditorSceneManager.MarkSceneDirty(active);
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[GameOverUIGenerator] Generated " + PrefabPath +
                  (active.name == "Game" ? " and re-linked GameManager. Delete any old game-over UI object." : "."));
    }
}
