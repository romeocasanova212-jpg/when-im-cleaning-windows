using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WhenImCleaningWindows.Editor
{
    /// <summary>
    /// Auto-generates MainGame scene on project load if it doesn't exist
    /// </summary>
    [InitializeOnLoad]
    public class AutoSceneGenerator
    {
        static AutoSceneGenerator()
        {
            // Try immediate generation first
            GenerateSceneIfNeeded();
            
            // Also schedule for delayed call as backup
            EditorApplication.delayCall += GenerateSceneIfNeeded;
        }

        private static void GenerateSceneIfNeeded()
        {
            string mainGamePath = "Assets/Scenes/MainGame.unity";
            
            // If scene already exists, don't regenerate
            if (System.IO.File.Exists(mainGamePath))
            {
                return;
            }
            
            Debug.Log("[AutoSceneGenerator] MainGame.unity not found. Generating...");
            
            try
            {
                // Generate the scene
                SceneSetupUtility.SetupProjectFull();
                Debug.Log("[AutoSceneGenerator] Scene generation complete!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoSceneGenerator] Failed to generate scene: {ex.Message}");
            }
        }
    }
}
