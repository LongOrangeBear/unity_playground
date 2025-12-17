using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Runner.Editor
{
    public class UIBuilder : EditorWindow
    {
        [MenuItem("Tools/Runner/Create Premium UI Assets")]
        public static void CreatePremiumAssets()
        {
            CreatePremiumButton();
            CreateGlassPanel();
        }

        private static void CreatePremiumButton()
        {
            GameObject canvas = GetOrCreateCanvas();
            
            GameObject btnObj = new GameObject("UI_Button_Premium");
            btnObj.transform.SetParent(canvas.transform, false);
            
            // Image
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 1f); // Nice blue
            // Note: In a real scenario we'd assign a sprite with rounded corners here
            // img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("...");
            
            // Button
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); // Slight bloom
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            btn.colors = colors;
            
            // Shadow
            btnObj.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);

            // TMP Text
            GameObject textObj = new GameObject("Text (TMP)");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "BUTTON";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
#pragma warning disable 0618
            tmp.enableWordWrapping = false;
#pragma warning restore 0618
            
            // RectTransform sizing
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(200, 60);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero; // Stretch
            
            // Save as Prefab
            string path = "Assets/Prefabs/UI/UI_Button_Premium.prefab";
            EnsureFolder("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(btnObj, path);
            GameObject.DestroyImmediate(btnObj);
            
            Debug.Log($"Created Premium Button at {path}");
        }

        private static void CreateGlassPanel()
        {
            GameObject canvas = GetOrCreateCanvas();
            
            GameObject panelObj = new GameObject("UI_Panel_Glass");
            panelObj.transform.SetParent(canvas.transform, false);
            
            Image img = panelObj.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // Dark semi-transparent
            
            // Outline/Stroke
            Outline outline = panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.1f);
            outline.effectDistance = new Vector2(2, -2);
            
            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.2f);
            rect.anchorMax = new Vector2(0.8f, 0.8f);
            
            string path = "Assets/Prefabs/UI/UI_Panel_Glass.prefab";
            EnsureFolder("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(panelObj, path);
            GameObject.DestroyImmediate(panelObj);
            
            Debug.Log($"Created Glass Panel at {path}");
        }

        private static GameObject GetOrCreateCanvas()
        {
            Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject startCanvas = new GameObject("TempCanvas");
                canvas = startCanvas.AddComponent<Canvas>();
            }
            return canvas.gameObject;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string current = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(current + "/" + folders[i]))
                    {
                        AssetDatabase.CreateFolder(current, folders[i]);
                    }
                    current += "/" + folders[i];
                }
            }
        }
    }
}
