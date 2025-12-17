using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor utility to create and configure Runner UI.
/// Menu: Tools > Runner > Setup UI
/// </summary>
public static class RunnerUISetup
{
    [MenuItem("Tools/Runner/Setup UI")]
    public static void SetupUI()
    {
        // Find or create Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        }
        
        // Ensure EventSystem exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
        }
        
        // Create Start Menu
        CreateStartMenu(canvas.transform);
        
        // Create Game Over UI
        CreateGameOverUI(canvas.transform);
        
        // Create HUD (score display)
        CreateHUD(canvas.transform);
        
        Debug.Log("[RunnerUISetup] UI setup complete! Check Canvas in hierarchy.");
    }
    
    [MenuItem("Tools/Runner/Setup Player Effects")]
    public static void SetupPlayerEffects()
    {
        // Find Player
        PlayerRunner player = Object.FindFirstObjectByType<PlayerRunner>();
        if (player == null)
        {
            Debug.LogError("[RunnerUISetup] No PlayerRunner found in scene!");
            return;
        }
        
        // Add PowerUpOrbitEffect if not present
        if (player.GetComponent<PowerUpOrbitEffect>() == null)
        {
            player.gameObject.AddComponent<PowerUpOrbitEffect>();
            Debug.Log("[RunnerUISetup] Added PowerUpOrbitEffect to Player.");
        }
        
        // Enable test mode on PowerUpManager
        PowerUpManager powerUpManager = Object.FindFirstObjectByType<PowerUpManager>();
        if (powerUpManager != null)
        {
            SerializedObject so = new SerializedObject(powerUpManager);
            so.FindProperty("_testMode").boolValue = true;
            so.ApplyModifiedProperties();
            Debug.Log("[RunnerUISetup] Enabled Test Mode on PowerUpManager.");
        }
        
        EditorUtility.SetDirty(player.gameObject);
        Debug.Log("[RunnerUISetup] Player effects setup complete!");
    }
    
    private static void CreateStartMenu(Transform parent)
    {
        // Delete old if exists
        Transform existing = parent.Find("StartMenuPanel");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log("[RunnerUISetup] Deleted old StartMenuPanel.");
        }
        
        // Panel
        GameObject panel = CreatePanel(parent, "StartMenuPanel", new Color(0, 0, 0, 0.8f));
        
        // Title
        GameObject titleGO = CreateText(panel.transform, "TitleText", "CITY RUNNER", 48, TextAnchor.MiddleCenter);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(400, 60);
        
        // Instructions
        GameObject instructionsGO = CreateText(panel.transform, "InstructionsText", "A/D - Move\nSpace - Jump\nCtrl - Slide", 24, TextAnchor.MiddleCenter);
        RectTransform instrRect = instructionsGO.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.5f, 0.5f);
        instrRect.anchorMax = new Vector2(0.5f, 0.5f);
        instrRect.sizeDelta = new Vector2(300, 100);
        
        // Start Button
        GameObject startBtnGO = CreateButton(panel.transform, "StartButton", "START");
        RectTransform startRect = startBtnGO.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(0.5f, 0.3f);
        startRect.anchorMax = new Vector2(0.5f, 0.3f);
        startRect.sizeDelta = new Vector2(200, 60);
        
        // Add StartMenuUI component
        StartMenuUI startMenuUI = panel.AddComponent<StartMenuUI>();
        
        // Use SerializedObject to set private fields
        SerializedObject so = new SerializedObject(startMenuUI);
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.FindProperty("_titleText").objectReferenceValue = titleGO.GetComponent<Text>();
        so.FindProperty("_instructionsText").objectReferenceValue = instructionsGO.GetComponent<Text>();
        so.FindProperty("_startButton").objectReferenceValue = startBtnGO.GetComponent<Button>();
        so.ApplyModifiedProperties();
        
        Undo.RegisterCreatedObjectUndo(panel, "Create StartMenuPanel");
    }
    
    private static void CreateGameOverUI(Transform parent)
    {
        // Delete old if exists
        Transform existing = parent.Find("GameOverController");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log("[RunnerUISetup] Deleted old GameOverController.");
        }
        existing = parent.Find("GameOverPanel");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log("[RunnerUISetup] Deleted old GameOverPanel.");
        }
        
        // Controller (always active, holds the script)
        GameObject controller = new GameObject("GameOverController");
        controller.transform.SetParent(parent, false);
        RectTransform ctrlRect = controller.AddComponent<RectTransform>();
        ctrlRect.anchorMin = Vector2.zero;
        ctrlRect.anchorMax = Vector2.one;
        ctrlRect.offsetMin = Vector2.zero;
        ctrlRect.offsetMax = Vector2.zero;
        
        // Panel (child of controller, starts hidden)
        GameObject panel = CreatePanel(controller.transform, "GameOverPanel", new Color(0.2f, 0, 0, 0.9f));
        panel.SetActive(false); // Start hidden
        
        // Game Over Title
        GameObject titleGO = CreateText(panel.transform, "GameOverTitle", "GAME OVER", 48, TextAnchor.MiddleCenter);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(400, 60);
        titleGO.GetComponent<Text>().color = Color.red;
        
        // Score Text
        GameObject scoreGO = CreateText(panel.transform, "ScoreText", "SCORE: 0", 32, TextAnchor.MiddleCenter);
        RectTransform scoreRect = scoreGO.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.6f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.6f);
        scoreRect.sizeDelta = new Vector2(300, 40);
        
        // High Score Text
        GameObject highScoreGO = CreateText(panel.transform, "HighScoreText", "HIGH SCORE: 0", 28, TextAnchor.MiddleCenter);
        RectTransform highScoreRect = highScoreGO.GetComponent<RectTransform>();
        highScoreRect.anchorMin = new Vector2(0.5f, 0.52f);
        highScoreRect.anchorMax = new Vector2(0.5f, 0.52f);
        highScoreRect.sizeDelta = new Vector2(300, 35);
        highScoreGO.GetComponent<Text>().color = Color.yellow;
        
        // Distance Text
        GameObject distanceGO = CreateText(panel.transform, "DistanceText", "DISTANCE: 0m", 24, TextAnchor.MiddleCenter);
        RectTransform distRect = distanceGO.GetComponent<RectTransform>();
        distRect.anchorMin = new Vector2(0.5f, 0.45f);
        distRect.anchorMax = new Vector2(0.5f, 0.45f);
        distRect.sizeDelta = new Vector2(300, 30);
        
        // Coins Text
        GameObject coinsGO = CreateText(panel.transform, "CoinsText", "COINS: 0", 24, TextAnchor.MiddleCenter);
        RectTransform coinsRect = coinsGO.GetComponent<RectTransform>();
        coinsRect.anchorMin = new Vector2(0.5f, 0.38f);
        coinsRect.anchorMax = new Vector2(0.5f, 0.38f);
        coinsRect.sizeDelta = new Vector2(300, 30);
        coinsGO.GetComponent<Text>().color = new Color(1f, 0.84f, 0f); // Gold
        
        // Restart Button
        GameObject restartBtnGO = CreateButton(panel.transform, "RestartButton", "RESTART");
        RectTransform restartRect = restartBtnGO.GetComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.5f, 0.2f);
        restartRect.anchorMax = new Vector2(0.5f, 0.2f);
        restartRect.sizeDelta = new Vector2(200, 60);
        
        // Add GameOverUI component to CONTROLLER (not panel!)
        GameOverUI gameOverUI = controller.AddComponent<GameOverUI>();
        
        // Use SerializedObject to set private fields
        SerializedObject so = new SerializedObject(gameOverUI);
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.FindProperty("_scoreText").objectReferenceValue = scoreGO.GetComponent<Text>();
        so.FindProperty("_highScoreText").objectReferenceValue = highScoreGO.GetComponent<Text>();
        so.FindProperty("_distanceText").objectReferenceValue = distanceGO.GetComponent<Text>();
        so.FindProperty("_coinsText").objectReferenceValue = coinsGO.GetComponent<Text>();
        so.FindProperty("_restartButton").objectReferenceValue = restartBtnGO.GetComponent<Button>();
        so.ApplyModifiedProperties();
        
        Undo.RegisterCreatedObjectUndo(controller, "Create GameOverController");
    }
    
    private static void CreateHUD(Transform parent)
    {
        // Delete old if exists
        Transform existing = parent.Find("HUD");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log("[RunnerUISetup] Deleted old HUD.");
        }
        
        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(parent, false);
        RectTransform hudRect = hud.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        
        // Score (top center)
        GameObject scoreGO = CreateText(hud.transform, "HUD_Score", "SCORE: 0", 36, TextAnchor.UpperCenter);
        RectTransform scoreRect = scoreGO.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0, -20);
        scoreRect.sizeDelta = new Vector2(300, 50);
        
        // Coins (top right)
        GameObject coinsGO = CreateText(hud.transform, "HUD_Coins", "x 0", 28, TextAnchor.UpperRight);
        RectTransform coinsRect = coinsGO.GetComponent<RectTransform>();
        coinsRect.anchorMin = new Vector2(1f, 1f);
        coinsRect.anchorMax = new Vector2(1f, 1f);
        coinsRect.pivot = new Vector2(1f, 1f);
        coinsRect.anchoredPosition = new Vector2(-20, -20);
        coinsRect.sizeDelta = new Vector2(150, 40);
        coinsGO.GetComponent<Text>().color = new Color(1f, 0.84f, 0f);
        
        // Distance (top left)
        GameObject distGO = CreateText(hud.transform, "HUD_Distance", "0m", 28, TextAnchor.UpperLeft);
        RectTransform distRect = distGO.GetComponent<RectTransform>();
        distRect.anchorMin = new Vector2(0f, 1f);
        distRect.anchorMax = new Vector2(0f, 1f);
        distRect.pivot = new Vector2(0f, 1f);
        distRect.anchoredPosition = new Vector2(20, -20);
        distRect.sizeDelta = new Vector2(150, 40);
        
        // Power-Up Timer (bottom left)
        GameObject powerUpGO = CreateText(hud.transform, "HUD_PowerUps", "", 20, TextAnchor.LowerLeft);
        RectTransform powerUpRect = powerUpGO.GetComponent<RectTransform>();
        powerUpRect.anchorMin = new Vector2(0f, 0f);
        powerUpRect.anchorMax = new Vector2(0f, 0f);
        powerUpRect.pivot = new Vector2(0f, 0f);
        powerUpRect.anchoredPosition = new Vector2(20, 20);
        powerUpRect.sizeDelta = new Vector2(250, 120);
        Text powerUpText = powerUpGO.GetComponent<Text>();
        powerUpText.supportRichText = true;
        powerUpText.fontSize = 18;
        
        // Add HUD component and bind fields
        HUD hudComponent = hud.AddComponent<HUD>();
        SerializedObject so = new SerializedObject(hudComponent);
        so.FindProperty("_scoreText").objectReferenceValue = scoreGO.GetComponent<Text>();
        so.FindProperty("_coinsText").objectReferenceValue = coinsGO.GetComponent<Text>();
        so.FindProperty("_distanceText").objectReferenceValue = distGO.GetComponent<Text>();
        so.FindProperty("_powerUpTimerText").objectReferenceValue = powerUpText;
        so.ApplyModifiedProperties();
        
        Undo.RegisterCreatedObjectUndo(hud, "Create HUD");
    }
    
    private static GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        
        return panel;
    }
    
    private static GameObject CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        
        Text text = textGO.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        return textGO;
    }
    
    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        
        Image img = buttonGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);
        
        Button button = buttonGO.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.8f);
        button.colors = colors;
        
        // Button text
        GameObject textGO = CreateText(buttonGO.transform, "Text", label, 24, TextAnchor.MiddleCenter);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonGO;
    }
}
