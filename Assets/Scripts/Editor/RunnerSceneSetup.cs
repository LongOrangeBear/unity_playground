using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility to set up the Runner scene with all required components.
/// </summary>
public static class RunnerSceneSetup
{
    [MenuItem("Tools/Runner/Create Runner Scene")]
    public static void CreateRunnerScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Save it first
        string scenePath = "Assets/Scenes/RunnerScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        
        // Create RunnerSettings asset if needed
        CreateSettingsAsset();
        
        // Setup scene objects
        SetupManagers();
        SetupPlayer();
        SetupCamera();
        SetupLighting();
        SetupWorld();
        SetupSpawners();
        SetupUI();
        
        // Save scene
        EditorSceneManager.SaveScene(scene);
        
        Debug.Log("[RunnerSceneSetup] Runner scene created successfully!");
    }
    
    [MenuItem("Tools/Runner/Setup Current Scene")]
    public static void SetupCurrentScene()
    {
        CreateSettingsAsset();
        SetupManagers();
        SetupPlayer();
        SetupCamera();
        SetupLighting();
        SetupWorld();
        SetupSpawners();
        SetupUI();
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[RunnerSceneSetup] Current scene configured for Runner!");
    }
    
    private static void CreateSettingsAsset()
    {
        string path = "Assets/Data/RunnerSettings.asset";
        if (AssetDatabase.LoadAssetAtPath<RunnerSettings>(path) != null)
        {
            Debug.Log("[RunnerSceneSetup] RunnerSettings already exists.");
            return;
        }
        
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        
        var settings = ScriptableObject.CreateInstance<RunnerSettings>();
        AssetDatabase.CreateAsset(settings, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[RunnerSceneSetup] Created RunnerSettings asset.");
    }
    
    private static RunnerSettings GetSettings()
    {
        return AssetDatabase.LoadAssetAtPath<RunnerSettings>("Assets/Data/RunnerSettings.asset");
    }
    
    private static void SetupManagers()
    {
        var settings = GetSettings();
        
        // GameManager
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            var gmObj = new GameObject("GameManager");
            var gm = gmObj.AddComponent<GameManager>();
            
            // Set settings via serialized object
            SerializedObject so = new SerializedObject(gm);
            so.FindProperty("_settings").objectReferenceValue = settings;
            so.ApplyModifiedProperties();
        }
        
        // ScoreManager
        if (Object.FindFirstObjectByType<ScoreManager>() == null)
        {
            var smObj = new GameObject("ScoreManager");
            smObj.AddComponent<ScoreManager>();
        }
        
        // ObjectPool
        if (Object.FindFirstObjectByType<ObjectPool>() == null)
        {
            var poolObj = new GameObject("ObjectPool");
            poolObj.AddComponent<ObjectPool>();
        }
        
        Debug.Log("[RunnerSceneSetup] Managers created.");
    }
    
    private static void SetupPlayer()
    {
        var settings = GetSettings();
        
        if (Object.FindFirstObjectByType<PlayerRunner>() != null)
        {
            Debug.Log("[RunnerSceneSetup] Player already exists.");
            return;
        }
        
        // Create player capsule
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1.1f, 0); // Capsule height 2, so center at 1 = bottom at 0
        player.tag = "Player";
        
        // Remove default collider (CharacterController handles collision)
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        
        // Add CharacterController
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.zero; // Center at object origin
        
        // Add PlayerRunner
        var runner = player.AddComponent<PlayerRunner>();
        
        // Set settings via serialized object
        SerializedObject so = new SerializedObject(runner);
        so.FindProperty("_settings").objectReferenceValue = settings;
        so.ApplyModifiedProperties();
        
        // Color the player red
        var renderer = player.GetComponent<MeshRenderer>();
        Material playerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        playerMat.color = Color.red;
        renderer.material = playerMat;
        
        Debug.Log("[RunnerSceneSetup] Player created.");
    }
    
    private static void SetupCamera()
    {
        // Find or create main camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        
        mainCam.transform.position = new Vector3(0, 5, -10);
        mainCam.transform.rotation = Quaternion.Euler(15, 0, 0);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        
        // Add RunnerCamera if not present
        if (mainCam.GetComponent<RunnerCamera>() == null)
        {
            mainCam.gameObject.AddComponent<RunnerCamera>();
        }
        
        Debug.Log("[RunnerSceneSetup] Camera configured.");
    }
    
    private static void SetupLighting()
    {
        // Directional light
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
        
        Debug.Log("[RunnerSceneSetup] Lighting configured.");
    }
    
    private static void SetupWorld()
    {
        var settings = GetSettings();
        
        if (Object.FindFirstObjectByType<ChunkSpawner>() != null)
        {
            Debug.Log("[RunnerSceneSetup] ChunkSpawner already exists.");
            return;
        }
        
        var worldObj = new GameObject("World");
        var spawner = worldObj.AddComponent<ChunkSpawner>();
        
        // Set settings via serialized object
        SerializedObject so = new SerializedObject(spawner);
        so.FindProperty("_settings").objectReferenceValue = settings;
        so.ApplyModifiedProperties();
        
        Debug.Log("[RunnerSceneSetup] World spawner created.");
    }
    
    private static void SetupSpawners()
    {
        var settings = GetSettings();
        
        // PowerUpManager
        if (Object.FindFirstObjectByType<PowerUpManager>() == null)
        {
            var pumObj = new GameObject("PowerUpManager");
            pumObj.AddComponent<PowerUpManager>();
        }
        
        // ObstacleSpawner
        if (Object.FindFirstObjectByType<ObstacleSpawner>() == null)
        {
            var obsObj = new GameObject("ObstacleSpawner");
            var obs = obsObj.AddComponent<ObstacleSpawner>();
            
            SerializedObject so = new SerializedObject(obs);
            so.FindProperty("_settings").objectReferenceValue = settings;
            so.ApplyModifiedProperties();
        }
        
        // EnemySpawner
        if (Object.FindFirstObjectByType<EnemySpawner>() == null)
        {
            var enemyObj = new GameObject("EnemySpawner");
            var enemy = enemyObj.AddComponent<EnemySpawner>();
            
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("_settings").objectReferenceValue = settings;
            so.ApplyModifiedProperties();
        }
        
        // CollectibleSpawner
        if (Object.FindFirstObjectByType<CollectibleSpawner>() == null)
        {
            var colObj = new GameObject("CollectibleSpawner");
            var col = colObj.AddComponent<CollectibleSpawner>();
            
            SerializedObject so = new SerializedObject(col);
            so.FindProperty("_settings").objectReferenceValue = settings;
            so.ApplyModifiedProperties();
        }
        
        Debug.Log("[RunnerSceneSetup] Spawners created.");
    }
    
    [MenuItem("Tools/Runner/Fix Spawner Values")]
    public static void FixSpawnerValues()
    {
        // Fix ObstacleSpawner
        var obs = Object.FindFirstObjectByType<ObstacleSpawner>();
        if (obs != null)
        {
            SerializedObject so = new SerializedObject(obs);
            so.FindProperty("_minSpawnDistance").floatValue = 60f;
            so.FindProperty("_maxSpawnDistance").floatValue = 100f;
            so.FindProperty("_minGapBetweenObstacles").floatValue = 15f;
            so.ApplyModifiedProperties();
            Debug.Log("[RunnerSceneSetup] ObstacleSpawner values fixed.");
        }
        
        // Fix EnemySpawner  
        var enemy = Object.FindFirstObjectByType<EnemySpawner>();
        if (enemy != null)
        {
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("_minSpawnDistance").floatValue = 80f;
            so.FindProperty("_spawnAheadDistance").floatValue = 120f;
            so.ApplyModifiedProperties();
            Debug.Log("[RunnerSceneSetup] EnemySpawner values fixed.");
        }
        
        // Fix Player CharacterController
        var player = Object.FindFirstObjectByType<PlayerRunner>();
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.center = Vector3.zero;
                player.transform.position = new Vector3(
                    player.transform.position.x,
                    1.1f,
                    player.transform.position.z);
            }
            Debug.Log("[RunnerSceneSetup] Player position fixed.");
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[RunnerSceneSetup] All values fixed! Save the scene.");
    }
    
    [MenuItem("Tools/Runner/Add Required Tags")]
    public static void AddRequiredTags()
    {
        AddTag("Obstacle");
        AddTag("Enemy");
        AddTag("Coin");
        AddTag("PowerUp");
        AddTag("Ground");
        
        Debug.Log("[RunnerSceneSetup] Tags added.");
    }
    
    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        // Check if tag exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }
        
        // Add new tag
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
    
    [MenuItem("Tools/Runner/Setup UI Only")]
    public static void SetupUIOnly()
    {
        SetupUI();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[RunnerSceneSetup] UI setup complete!");
    }
    
    private static void SetupUI()
    {
        // Check if canvas already exists
        var existingCanvas = Object.FindFirstObjectByType<UnityEngine.Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("[RunnerSceneSetup] Canvas already exists, skipping UI setup.");
            return;
        }
        
        // Create Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<UnityEngine.Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Configure CanvasScaler
        var scaler = canvasObj.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        
        // Create HUD
        CreateHUD(canvasObj.transform);
        
        // Create GameOver Panel
        CreateGameOverPanel(canvasObj.transform);
        
        // Create Start Menu Panel
        CreateStartMenuPanel(canvasObj.transform);
        
        Debug.Log("[RunnerSceneSetup] UI created.");
    }
    
    private static void CreateHUD(Transform parent)
    {
        var hudObj = new GameObject("HUD");
        hudObj.transform.SetParent(parent, false);
        var hudRect = hudObj.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        
        // Score Text (top center)
        var scoreText = CreateText("ScoreText", hudObj.transform, "Score: 0", 
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50), 48);
        
        // Coins Text (top right)
        var coinsText = CreateText("CoinsText", hudObj.transform, "Coins: 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150, -50), 32);
        
        // Distance Text (top left)
        var distText = CreateText("DistanceText", hudObj.transform, "0m",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100, -50), 32);
        
        // Speed Text (below distance)
        var speedText = CreateText("SpeedText", hudObj.transform, "Speed: 10",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100, -90), 24);
        
        // Add HUD component
        var hud = hudObj.AddComponent<HUD>();
        
        // Link references via SerializedObject
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("_coinsText").objectReferenceValue = coinsText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("_distanceText").objectReferenceValue = distText.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("_speedText").objectReferenceValue = speedText.GetComponent<UnityEngine.UI.Text>();
        so.ApplyModifiedProperties();
    }
    
    private static void CreateGameOverPanel(Transform parent)
    {
        var panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(parent, false);
        
        // Full screen dark overlay
        var panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        var panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Game Over Title
        CreateText("TitleText", panelObj.transform, "GAME OVER",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, 72);
        
        // Final Score
        var finalScore = CreateText("FinalScoreText", panelObj.transform, "Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), 48);
        
        // High Score
        var highScore = CreateText("HighScoreText", panelObj.transform, "High Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), 36);
        
        // Restart Button
        var restartBtn = CreateButton("RestartButton", panelObj.transform, "RESTART",
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(200, 60));
        
        // Add GameOverUI component
        var gameOverUI = panelObj.AddComponent<GameOverUI>();
        
        SerializedObject so = new SerializedObject(gameOverUI);
        so.FindProperty("_finalScoreText").objectReferenceValue = finalScore.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("_highScoreText").objectReferenceValue = highScore.GetComponent<UnityEngine.UI.Text>();
        so.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<UnityEngine.UI.Button>();
        so.ApplyModifiedProperties();
        
        panelObj.SetActive(false); // Start hidden
    }
    
    private static void CreateStartMenuPanel(Transform parent)
    {
        var panelObj = new GameObject("StartMenuPanel");
        panelObj.transform.SetParent(parent, false);
        
        var panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        var panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        // Title
        CreateText("TitleText", panelObj.transform, "RUNNER",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, 96);
        
        // Instruction
        CreateText("InstructionText", panelObj.transform, "Press SPACE to Start",
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, 36);
        
        // High Score
        var highScore = CreateText("HighScoreText", panelObj.transform, "High Score: 0",
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, 32);
        
        // Add StartMenuUI component
        var startUI = panelObj.AddComponent<StartMenuUI>();
        
        SerializedObject so = new SerializedObject(startUI);
        so.FindProperty("_highScoreText").objectReferenceValue = highScore.GetComponent<UnityEngine.UI.Text>();
        so.ApplyModifiedProperties();
        
        panelObj.SetActive(false); // Will be shown by GameManager
    }
    
    private static GameObject CreateText(string name, Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, int fontSize)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        var rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(400, fontSize + 20);
        
        var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Add outline for readability
        var outline = textObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        return textObj;
    }
    
    private static GameObject CreateButton(string name, Transform parent, string text,
        Vector2 anchor, Vector2 pivot, Vector2 size)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        
        var image = btnObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.6f, 1f);
        
        var button = btnObj.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        
        // Button text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
        textComp.text = text;
        textComp.fontSize = 32;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        return btnObj;
    }
}
