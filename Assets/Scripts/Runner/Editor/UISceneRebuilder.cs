using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Runner.Editor
{
    [InitializeOnLoad]
    public static class UISceneRebuilder
    {
        static UISceneRebuilder()
        {
            EditorApplication.delayCall += () =>
            {
                // Changed key to V5 to ensure it runs again after compilation fix
                if (!SessionState.GetBool("UIRebuildRan_V5", false))
                {
                    SessionState.SetBool("UIRebuildRan_V5", true);
                    RebuildUI();
                }
            };
        }

        [MenuItem("Tools/Runner/Rebuild Complete UI")]
        public static void RebuildUI()
        {
            // 1. Nuke existing UI
            GameObject existingCanvas = GameObject.Find("Canvas");
            if (existingCanvas != null) Undo.DestroyObjectImmediate(existingCanvas);
            
            GameObject existingEventSystem = GameObject.Find("EventSystem");
            if (existingEventSystem != null) Undo.DestroyObjectImmediate(existingEventSystem);
            
            // 2. Create Event System (Input System friendly)
            GameObject eventSystemGO = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            // 3. Create Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f; // Match width/height equally
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // 4. Create Screens
            CreateHUD(canvasGO.transform);
            CreateStartMenu(canvasGO.transform);
            CreateGameOver(canvasGO.transform); // Fixed Logic inside
            
            Debug.Log("UI Rebuild Complete (V5)! 🏗️");
        }

        private static void CreateHUD(Transform parent)
        {
            GameObject hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(parent, false);
            RectTransform rect = hudGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            HUD hud = hudGO.AddComponent<HUD>();
            
            // Score (Top Left) - Pivot (0,1)
            hud.SetPrivateField("_scoreText", CreateTMP(hudGO.transform, "ScoreText", "SCORE: 0", 
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), TextAlignmentOptions.Left));
            
            // Coins (Top Right) - Pivot (1,1)
            hud.SetPrivateField("_coinsText", CreateTMP(hudGO.transform, "CoinsText", "x 0", 
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), TextAlignmentOptions.Right));
                
            // Distance (Top Center) - Pivot (0.5, 1)
            hud.SetPrivateField("_distanceText", CreateTMP(hudGO.transform, "DistanceText", "0m", 
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), TextAlignmentOptions.Center));
            
            // Speed (Bottom Left) - Pivot (0,0)
            hud.SetPrivateField("_speedText", CreateTMP(hudGO.transform, "SpeedText", "0 m/s", 
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(30, 30), TextAlignmentOptions.Left));
            
            // Powerup Timer (Left Center) - Pivot (0, 0.5)
            hud.SetPrivateField("_powerUpTimerText", CreateTMP(hudGO.transform, "PowerUpTimer", "", 
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 50), TextAlignmentOptions.Left));
            
            // Icons Container (Left Center, below Timer)
            GameObject iconsContainer = new GameObject("Icons");
            iconsContainer.transform.SetParent(hudGO.transform, false);
            RectTransform iconsRect = iconsContainer.AddComponent<RectTransform>();
            iconsRect.anchorMin = new Vector2(0, 0.5f);
            iconsRect.anchorMax = new Vector2(0, 0.5f);
            iconsRect.pivot = new Vector2(0, 1);
            iconsRect.anchoredPosition = new Vector2(30, 0); // Below timer
            
            // Create Icons
            hud.SetPrivateField("_magnetIcon", CreateIcon(iconsContainer.transform, "Magnet", Color.blue, new Vector2(0, 0)));
            hud.SetPrivateField("_shieldIcon", CreateIcon(iconsContainer.transform, "Shield", Color.white, new Vector2(50, 0)));
            hud.SetPrivateField("_doubleScoreIcon", CreateIcon(iconsContainer.transform, "DoubleScore", Color.green, new Vector2(100, 0)));
            hud.SetPrivateField("_speedBoostIcon", CreateIcon(iconsContainer.transform, "SpeedBoost", Color.yellow, new Vector2(150, 0)));
        }

        private static void CreateStartMenu(Transform parent)
        {
            GameObject menuGO = new GameObject("StartMenu");
            menuGO.transform.SetParent(parent, false);
            RectTransform rect = menuGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            StartMenuUI menuUI = menuGO.AddComponent<StartMenuUI>();
            menuUI.SetPrivateField("_panel", menuGO);
            
            // Background Blur/Panel
            CreatePanelBackground(menuGO.transform);
            
            // Title
            TextMeshProUGUI title = CreateTMP(menuGO.transform, "Title", "NEON RUNNER", 
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
            title.fontSize = 80;
            title.color = Color.cyan;
            menuUI.SetPrivateField("_titleText", title);
            
            // Instructions
            TextMeshProUGUI instr = CreateTMP(menuGO.transform, "Instructions", "A/D to Move, SPACE to Jump, CTRL to Slide", 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
            menuUI.SetPrivateField("_instructionsText", instr);
            
            // Start Button
            GameObject btnGO = CreatePremiumButton(menuGO.transform, "StartButton", "START GAME");
            RectTransform btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.3f);
            btnRect.anchorMax = new Vector2(0.5f, 0.3f);
            btnRect.anchoredPosition = Vector2.zero;
            
            menuUI.SetPrivateField("_startButton", btnGO.GetComponent<Button>());
        }

        private static void CreateGameOver(Transform parent)
        {
            // 1. Logic Root (Always Active)
            GameObject logicGO = new GameObject("GameOverUI_Logic");
            logicGO.transform.SetParent(parent, false);
            RectTransform logicRect = logicGO.AddComponent<RectTransform>();
            logicRect.anchorMin = Vector2.zero;
            logicRect.anchorMax = Vector2.one;
            logicRect.sizeDelta = Vector2.zero;
            
            GameOverUI goUI = logicGO.AddComponent<GameOverUI>();
            
            // 2. Visual Panel (Initially Inactive)
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(logicGO.transform, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            goUI.SetPrivateField("_panel", panelGO);
            panelGO.AddComponent<CanvasGroup>(); // For fading
            
            // Background
            CreatePanelBackground(panelGO.transform);
            
            // Title
            TextMeshProUGUI title = CreateTMP(panelGO.transform, "Title", "GAME OVER", 
                new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
            title.fontSize = 70;
            title.color = Color.red;
            
            // Stats
            goUI.SetPrivateField("_scoreText", CreateTMP(panelGO.transform, "Score", "SCORE: 0", 
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center));
            
            goUI.SetPrivateField("_highScoreText", CreateTMP(panelGO.transform, "HighScore", "HIGH: 0", 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center));
            
            goUI.SetPrivateField("_coinsText", CreateTMP(panelGO.transform, "Coins", "COINS: 0", 
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center));
            
            goUI.SetPrivateField("_distanceText", CreateTMP(panelGO.transform, "Distance", "DISTANCE: 0m", 
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center));
                
            // Restart Button
            GameObject btnGO = CreatePremiumButton(panelGO.transform, "RestartButton", "RESTART");
            RectTransform btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.15f);
            btnRect.anchorMax = new Vector2(0.5f, 0.15f);
            btnRect.anchoredPosition = Vector2.zero;
            
            goUI.SetPrivateField("_restartButton", btnGO.GetComponent<Button>());
            
            // IMPORTANT: Disable Panel, but keep Logic Active
            panelGO.SetActive(false); 
        }

        // --- Helpers ---

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = 36;
            tmp.alignment = align;
            tmp.color = Color.white;
#pragma warning disable 0618
            tmp.enableWordWrapping = false;
#pragma warning restore 0618

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            
            if (align == TextAlignmentOptions.Left || align == TextAlignmentOptions.Right)
                rect.sizeDelta = new Vector2(300, 50); // Fixed width for side texts
            else
                rect.sizeDelta = new Vector2(600, 80); // Wider for center texts
                
            return tmp;
        }

        private static GameObject CreateIcon(Transform parent, string name, Color color, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(40, 40);
            
            go.SetActive(false); // Hidden by default
            return go;
        }
        
        private static void CreatePanelBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();
            
            Image img = bg.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.85f);
            
            RectTransform rect = bg.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
        
        private static GameObject CreatePremiumButton(Transform parent, string name, string label)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 1f); 

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); 
            btn.colors = colors;
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 70);

            // Text
            TextMeshProUGUI tmp = CreateTMP(btnObj.transform, "Text", label, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
            tmp.color = Color.white;
            tmp.fontSize = 32;
            
            return btnObj;
        }
        
        // Reflection helper to set private [SerializeField] fields
        public static void SetPrivateField(this object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
            }
        }
    }
}
