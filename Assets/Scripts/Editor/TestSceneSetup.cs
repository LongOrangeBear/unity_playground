using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class TestSceneSetup
{
    [MenuItem("Tools/TestScene/Create PhysicMaterial")]
    public static void CreatePhysicMaterial()
    {
        var mat = new PhysicsMaterial("ZeroFriction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f
        };
        
        string path = "Assets/PhysicsMaterials/ZeroFriction.physicMaterial";
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"[TestSceneSetup] Created PhysicMaterial at {path}");
    }
    
    [MenuItem("Tools/TestScene/Setup Full Scene")]
    public static void SetupFullScene()
    {
        // Load materials
        var groundMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GroundMat.mat");
        var playerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PlayerMat.mat");
        var wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WallMat.mat");
        var zeroFriction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/PhysicsMaterials/ZeroFriction.physicMaterial");
        
        // Create Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2, 1, 2);
        if (groundMat) ground.GetComponent<MeshRenderer>().material = groundMat;
        
        // Create Walls
        CreateWall("Wall_North", new Vector3(0, 1, 10), new Vector3(20, 2, 0.5f), wallMat);
        CreateWall("Wall_South", new Vector3(0, 1, -10), new Vector3(20, 2, 0.5f), wallMat);
        CreateWall("Wall_East", new Vector3(10, 1, 0), new Vector3(0.5f, 2, 20), wallMat);
        CreateWall("Wall_West", new Vector3(-10, 1, 0), new Vector3(0.5f, 2, 20), wallMat);
        
        // Create Player
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1.1f, 0);
        if (playerMat) player.GetComponent<MeshRenderer>().material = playerMat;
        
        var rb = player.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        var capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (zeroFriction && capsuleCollider) capsuleCollider.material = zeroFriction;
        
        player.AddComponent<PlayerController>();
        
        // Setup Camera
        var mainCam = Camera.main;
        if (mainCam)
        {
            mainCam.transform.position = new Vector3(0, 10, -10);
            mainCam.transform.rotation = Quaternion.Euler(45, 0, 0);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.2f, 1f); // Dark Blue
        }
        
        Debug.Log("[TestSceneSetup] Scene setup complete! Now create UI manually or use SetupUI.");
    }
    
    [MenuItem("Tools/TestScene/Setup UI")]
    public static void SetupUI()
    {
        // Create Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if needed
        if (!Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create ButtonPanel
        var panel = new GameObject("ButtonPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 0);
        panelRect.pivot = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(20, 20);
        panelRect.sizeDelta = new Vector2(320, 320);
        
        // Create buttons
        CreateMoveButton(panel.transform, "BtnUp", new Vector2(110, 220), new Vector3(0, 0, 1));
        CreateMoveButton(panel.transform, "BtnDown", new Vector2(110, 0), new Vector3(0, 0, -1));
        CreateMoveButton(panel.transform, "BtnLeft", new Vector2(0, 110), new Vector3(-1, 0, 0));
        CreateMoveButton(panel.transform, "BtnRight", new Vector2(220, 110), new Vector3(1, 0, 0));
        
        Debug.Log("[TestSceneSetup] UI setup complete!");
    }
    
    private static void CreateWall(string name, Vector3 position, Vector3 scale, Material mat)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        if (mat) wall.GetComponent<MeshRenderer>().material = mat;
    }
    
    private static void CreateMoveButton(Transform parent, string name, Vector2 position, Vector3 direction)
    {
        var btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        
        var rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        rect.anchoredPosition = position;
        
        var image = btnGO.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        var button = btnGO.AddComponent<Button>();
        var colors = button.colors;
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        button.colors = colors;
        
        // var moveBtn = btnGO.AddComponent<MoveButton>();
        
        // // Set direction via serialized field - reflection needed in Editor
        // var dirField = typeof(MoveButton).GetField("_moveDirection", 
        //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // if (dirField != null)
        //     dirField.SetValue(moveBtn, direction);
        
        // Add label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(btnGO.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        var text = labelGO.AddComponent<UnityEngine.UI.Text>();
        text.text = name.Replace("Btn", "");
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }


    [MenuItem("Tools/TestScene/Fix Scene Issues")]
    public static void FixSceneIssues()
    {
        // Fix Camera
        var mainCam = Camera.main;
        if (mainCam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
        mainCam.transform.position = new Vector3(0, 10, -10);
        mainCam.transform.rotation = Quaternion.Euler(45, 0, 0);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.2f, 1f);
        Debug.Log("[TestSceneSetup] Camera configured.");
        
        // Fix EventSystem - replace StandaloneInputModule with InputSystemUIInputModule
        var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            var standaloneModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (standaloneModule != null)
            {
                Object.DestroyImmediate(standaloneModule);
                Debug.Log("[TestSceneSetup] Removed StandaloneInputModule.");
            }
            
            var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null)
            {
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("[TestSceneSetup] Added InputSystemUIInputModule.");
            }
        }
        
        Debug.Log("[TestSceneSetup] Scene issues fixed!");
    }


    [MenuItem("Tools/TestScene/Adjust Player Speed")]
    public static void AdjustPlayerSpeed()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("[TestSceneSetup] Player not found!");
            return;
        }
        
        // Adjust Rigidbody drag for faster acceleration
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping = 2f; // Reduced from 5 for faster movement
            Debug.Log("[TestSceneSetup] Rigidbody drag set to 2.");
        }
        
        // Adjust PlayerController speed
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_moveSpeed").floatValue = 50f;
            so.ApplyModifiedProperties();
            Debug.Log("[TestSceneSetup] PlayerController speed set to 50.");
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[TestSceneSetup] Player speed adjusted! Save the scene.");
    }
}