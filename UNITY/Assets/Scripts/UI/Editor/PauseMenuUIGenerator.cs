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
/// One-click generator that builds the styled pause menu (Resume / Restart / Main Menu), wires it to
/// a <see cref="PauseMenu"/> component, and saves it to Assets/Prefabs/UI/PauseMenu.prefab. If the
/// active scene is "Game" it drops an instance in, ensures an EventSystem, and re-links
/// GameManager's pause-panel reference to the toggled panel.
/// Run via the menu: <b>War of Tanks ▸ Generate Pause Menu UI</b>.
/// </summary>
public static class PauseMenuUIGenerator
{
    private const string PrefabPath = "Assets/Prefabs/UI/PauseMenu.prefab";
    private const string RootName = "PauseMenu";

    [MenuItem("War of Tanks/Generate Pause Menu UI")]
    public static void Generate()
    {
        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // above HUD (10), below game-over (100)
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        PauseMenu pause = root.AddComponent<PauseMenu>();

        // Dark scrim overlay — this is the GameObject GameManager toggles.
        GameObject panel = F.CreatePanel("Panel", root.transform, new Color(F.Bg.r, F.Bg.g, F.Bg.b, 0.92f), 0f);

        RectTransform card = F.CreateCard("PauseCard", panel.transform, 480, 420, 14f);
        F.CreateBrandMark(card);
        F.CreateLabel("Title", "Paused", card, 40, F.Text, 420, 54,
            bold: true, upper: false, spacing: 0f, align: TextAlignmentOptions.Center);
        F.CreateLabel("Subtitle", "The match is frozen.", card, 16, F.Muted, 420, 22,
            bold: false, upper: false, spacing: 0f, align: TextAlignmentOptions.Center);
        F.Spacer(card, 8);
        Button resumeButton = F.CreateButton("ResumeButton", "Resume", card, primary: true);
        Button restartButton = F.CreateButton("RestartButton", "Restart", card, primary: false);
        Button menuButton = F.CreateButton("MainMenuButton", "Main Menu", card, primary: false);

        UnityEventTools.AddPersistentListener(resumeButton.onClick, new UnityAction(pause.Resume));
        UnityEventTools.AddPersistentListener(restartButton.onClick, new UnityAction(pause.Restart));
        UnityEventTools.AddPersistentListener(menuButton.onClick, new UnityAction(pause.ToMainMenu));

        panel.SetActive(false); // hidden until GameManager shows it on pause

        // ---- Save prefab ----
        F.EnsureFolder();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab == null)
        {
            Debug.LogError("[PauseMenuUIGenerator] Failed to save prefab at " + PrefabPath);
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.name == "Game")
        {
            F.EnsureEventSystem();
            GameObject instance = GameObject.Find(RootName);
            if (instance == null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Pause Menu");
            }

            // Re-link GameManager's pause panel to the toggled "Panel" child.
            Transform panelChild = instance.transform.Find("Panel");
            GameManager gm = Object.FindObjectOfType<GameManager>();
            if (gm != null && panelChild != null)
            {
                SerializedObject gmSo = new SerializedObject(gm);
                SerializedProperty prop = gmSo.FindProperty("_pausePanel");
                if (prop != null)
                {
                    prop.objectReferenceValue = panelChild.gameObject;
                    gmSo.ApplyModifiedProperties();
                }
            }
            EditorSceneManager.MarkSceneDirty(active);
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[PauseMenuUIGenerator] Generated " + PrefabPath +
                  (active.name == "Game" ? " and re-linked GameManager. Delete any old pause-panel object." : "."));
    }
}
