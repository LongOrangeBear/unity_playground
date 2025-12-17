using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Runner.Editor
{
    /// <summary>
    /// Automates the upgrade of the UI to TextMeshPro and assigns references.
    /// </summary>
    [InitializeOnLoad]
    public class UIAutoUpgrader
    {
        static UIAutoUpgrader()
        {
            // Auto-run once after compilation to handle user request without manual menu click
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("UIAutoUpgradeRan", false))
                {
                    SessionState.SetBool("UIAutoUpgradeRan", true);
                    RunUpgrade();
                    UIBuilder.CreatePremiumAssets();
                }
            };
        }

        [MenuItem("Tools/Runner/Auto Upgrade UI to TMP")]
        public static void RunUpgrade()
        {
            UpgradeHUD();
            UpgradeGameOver();
            UpgradeStartMenu();
            
            Debug.Log("UI Auto-Upgrade Complete!");
        }

        private static void UpgradeHUD()
        {
            HUD hud = GameObject.FindFirstObjectByType<HUD>();
            if (hud == null)
            {
                Debug.LogWarning("HUD not found in scene.");
                return;
            }

            Undo.RecordObject(hud, "Upgrade HUD References");

            // Define mapping from Field Name (in script) to likely Child Name
            AssignTMP(hud, "_scoreText", "Score");
            AssignTMP(hud, "_coinsText", "Coin"); // Matches "Coins" or "Coin"
            AssignTMP(hud, "_distanceText", "Distance");
            AssignTMP(hud, "_speedText", "Speed");
            AssignTMP(hud, "_powerUpTimerText", "Timer");

            EditorUtility.SetDirty(hud);
        }

        private static void UpgradeGameOver()
        {
            GameOverUI goUI = GameObject.FindFirstObjectByType<GameOverUI>();
            if (goUI == null) return;
            
            Undo.RecordObject(goUI, "Upgrade Game Over References");
            
            AssignTMP(goUI, "_scoreText", "Score");
            AssignTMP(goUI, "_highScoreText", "High");
            AssignTMP(goUI, "_distanceText", "Distance");
            AssignTMP(goUI, "_coinsText", "Coin");
            
            EditorUtility.SetDirty(goUI);
        }

        private static void UpgradeStartMenu()
        {
            StartMenuUI startUI = GameObject.FindFirstObjectByType<StartMenuUI>();
            if (startUI == null) return;

            Undo.RecordObject(startUI, "Upgrade Start Menu References");

            AssignTMP(startUI, "_titleText", "Title");
            AssignTMP(startUI, "_instructionsText", "Instruction");

            EditorUtility.SetDirty(startUI);
        }

        /// <summary>
        /// Finds a child containing the search name, swaps Text for TMP, and assigns to the SerializedProperty.
        /// </summary>
        private static void AssignTMP(MonoBehaviour script, string fieldName, string childSearchName)
        {
            SerializedObject so = new SerializedObject(script);
            SerializedProperty prop = so.FindProperty(fieldName);
            
            if (prop == null)
            {
                Debug.LogWarning($"Property {fieldName} not found on {script.name}");
                return;
            }

            // Find child
            Transform targetChild = FindChildRecursive(script.transform, childSearchName);
            if (targetChild == null)
            {
                Debug.LogWarning($"Could not find child containing '{childSearchName}' under {script.name}");
                return;
            }

            // Swap Components
            Text legacyText = targetChild.GetComponent<Text>();
            TextMeshProUGUI tmp = targetChild.GetComponent<TextMeshProUGUI>();

            if (legacyText != null)
            {
                string content = legacyText.text;
                Color color = legacyText.color;
                int size = legacyText.fontSize;
                FontStyle style = legacyText.fontStyle;
                TextAnchor anchor = legacyText.alignment;

                Undo.DestroyObjectImmediate(legacyText);
                
                if (tmp == null)
                    tmp = Undo.AddComponent<TextMeshProUGUI>(targetChild.gameObject);
                
                // Copy basic properties
                tmp.text = content;
                tmp.color = color;
                tmp.fontSize = size > 0 ? size : 24;
#pragma warning disable 0618
                tmp.enableWordWrapping = false;
#pragma warning restore 0618
                
                // Convert alignment (approximate)
                if (anchor.ToString().Contains("Center")) tmp.alignment = TextAlignmentOptions.Center;
                else if (anchor.ToString().Contains("Right")) tmp.alignment = TextAlignmentOptions.Right;
                else tmp.alignment = TextAlignmentOptions.Left;
            }
            else if (tmp == null)
            {
                // Create if neither exists
                 tmp = Undo.AddComponent<TextMeshProUGUI>(targetChild.gameObject);
                 tmp.text = "New Text";
                 tmp.fontSize = 24;
            }

            // Assign to script
            prop.objectReferenceValue = tmp;
            so.ApplyModifiedProperties();
            Debug.Log($"Assigned {fieldName} to {tmp.name} (TMP) on {script.name}");
        }

        private static Transform FindChildRecursive(Transform parent, string nameFragment)
        {
            foreach (Transform child in parent)
            {
                if (child.name.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return child;
                }
                
                Transform result = FindChildRecursive(child, nameFragment);
                if (result != null) return result;
            }
            return null;
        }
    }
}
