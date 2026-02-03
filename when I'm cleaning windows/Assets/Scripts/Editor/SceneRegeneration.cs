using UnityEditor;
using UnityEditor.SceneManagement;

namespace WhenImCleaningWindows.Editor
{
    /// <summary>
    /// Automated scene regeneration on project reload
    /// </summary>
    [InitializeOnLoad]
    public class SceneRegeneration
    {
        private static readonly string RegenerationKey = "WhenImCleaning_NeedSceneRegen";

        static SceneRegeneration()
        {
            // Check if scene needs regeneration
            if (EditorPrefs.GetBool(RegenerationKey, false))
            {
                EditorPrefs.DeleteKey(RegenerationKey);
                EditorApplication.update += RegenerateScene;
            }
        }

        private static void RegenerateScene()
        {
            EditorApplication.update -= RegenerateScene;
            
            // Run the full scene setup
            SceneSetupUtility.SetupProjectFull();
            
            UnityEngine.Debug.Log("[SceneRegeneration] Completed");
        }

        // Public method to trigger on-demand
        [MenuItem("Tools/When I'm Cleaning Windows/Regenerate Scene Now")]
        public static void RegenerateNow()
        {
            SceneSetupUtility.SetupProjectFull();
        }
    }
}
