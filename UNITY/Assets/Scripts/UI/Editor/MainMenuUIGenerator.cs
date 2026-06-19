using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using F = MenuUIFactory;

/// <summary>
/// One-click generator that builds the styled main-menu UI (Play / Settings / Quit + an editable
/// match-specs panel), wires it to a <see cref="MainMenuUI"/> component, and saves it as a finished
/// prefab at Assets/Prefabs/UI/MainMenuUI.prefab. If the active scene is "MainMenu" it also drops an
/// instance in and ensures an EventSystem.
/// Run via the menu: <b>War of Tanks ▸ Generate Main Menu UI</b>.
/// </summary>
public static class MainMenuUIGenerator
{
    private const string PrefabPath = "Assets/Prefabs/UI/MainMenuUI.prefab";

    [MenuItem("War of Tanks/Generate Main Menu UI")]
    public static void Generate()
    {
        GameObject root = new GameObject("MainMenuUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        MainMenuUI menu = root.AddComponent<MainMenuUI>();

        // ---- Main panel ----
        GameObject mainPanel = F.CreatePanel("MainPanel", root.transform, F.Bg, 18f);
        F.CreateBrandMark(mainPanel.transform);
        F.CreateLabel("Title", "WAR OF TANKS", mainPanel.transform, 46, F.Text, 720, 60,
            bold: true, upper: true, spacing: 8f, align: TextAlignmentOptions.Center);
        F.CreateLabel("Subtitle", "Capture the centre. Hold the zone.", mainPanel.transform, 18, F.Muted, 720, 26,
            bold: false, upper: false, spacing: 0f, align: TextAlignmentOptions.Center);
        F.Spacer(mainPanel.transform, 10);
        Button playButton = F.CreateButton("PlayButton", "Play", mainPanel.transform, primary: true);
        Button settingsButton = F.CreateButton("SettingsButton", "Settings", mainPanel.transform, primary: false);
        Button quitButton = F.CreateButton("QuitButton", "Quit", mainPanel.transform, primary: false);

        // ---- Settings panel (centered bordered card) ----
        GameObject settingsPanel = F.CreatePanel("SettingsPanel", root.transform, F.Bg, 0f);
        RectTransform card = F.CreateCard("SettingsCard", settingsPanel.transform, 660, 600, 14f);
        F.CreateLabel("Header", "MATCH SETTINGS", card, 13, F.Muted, 600, 22,
            bold: true, upper: true, spacing: 4f, align: TextAlignmentOptions.Left);

        TMP_InputField tankHealth = F.CreateSpecRow("TankHealth", "Tank max health", card);
        TMP_InputField fireRate = F.CreateSpecRow("FireRate", "Fire rate (shots/s)", card);
        TMP_InputField respawnDelay = F.CreateSpecRow("RespawnDelay", "Respawn delay (s)", card);
        TMP_InputField bulletDamage = F.CreateSpecRow("BulletDamage", "Bullet damage", card);
        TMP_InputField explosionRadius = F.CreateSpecRow("ExplosionRadius", "Explosion radius", card);
        TMP_InputField matchDuration = F.CreateSpecRow("MatchDuration", "Match duration (s)", card);
        TMP_InputField scoreLimit = F.CreateSpecRow("ScoreLimit", "Score limit", card);
        Slider volume = F.CreateSliderRow("Volume", "Master volume", card);

        F.Spacer(card, 6);
        RectTransform buttonRow = F.NewRect("Buttons", card);
        F.AddLayoutElement(buttonRow.gameObject, 600, 52);
        F.ConfigureLayout(buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>(), 12f, TextAnchor.MiddleCenter);
        Button saveButton = F.CreateButton("SaveButton", "Save", buttonRow.transform, primary: true, width: 180);
        Button resetButton = F.CreateButton("ResetButton", "Reset", buttonRow.transform, primary: false, width: 180);
        Button backButton = F.CreateButton("BackButton", "Back", buttonRow.transform, primary: false, width: 180);

        settingsPanel.SetActive(false);

        // ---- Wire references onto the (private serialized) MainMenuUI fields ----
        SerializedObject so = new SerializedObject(menu);
        F.SetRef(so, "_mainPanel", mainPanel);
        F.SetRef(so, "_settingsPanel", settingsPanel);
        F.SetRef(so, "_playButton", playButton);
        F.SetRef(so, "_settingsButton", settingsButton);
        F.SetRef(so, "_quitButton", quitButton);
        F.SetRef(so, "_saveButton", saveButton);
        F.SetRef(so, "_resetButton", resetButton);
        F.SetRef(so, "_backButton", backButton);
        F.SetRef(so, "_tankHealthField", tankHealth);
        F.SetRef(so, "_fireRateField", fireRate);
        F.SetRef(so, "_respawnDelayField", respawnDelay);
        F.SetRef(so, "_bulletDamageField", bulletDamage);
        F.SetRef(so, "_explosionRadiusField", explosionRadius);
        F.SetRef(so, "_matchDurationField", matchDuration);
        F.SetRef(so, "_scoreLimitField", scoreLimit);
        F.SetRef(so, "_volumeSlider", volume);
        so.ApplyModifiedPropertiesWithoutUndo();

        // ---- Save prefab ----
        F.EnsureFolder();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab == null)
        {
            Debug.LogError("[MainMenuUIGenerator] Failed to save prefab at " + PrefabPath);
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.name == "MainMenu")
        {
            F.EnsureEventSystem();
            if (Object.FindObjectOfType<MainMenuUI>() == null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Main Menu UI");
            }
            EditorSceneManager.MarkSceneDirty(active);
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[MainMenuUIGenerator] Generated " + PrefabPath +
                  (active.name == "MainMenu" ? " and added it to the MainMenu scene." : "."));
    }
}
