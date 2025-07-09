using UnityEngine;
using UnityEditor;

// Update the namespace to be consistent with TLUIToolkit.Editor
namespace TLUIToolkit.Editor
{
    public static class TasksCheckUIEditor
    {
        // Define the path where the ProgressUI prefab should be located
        private const string PREFAB_RESOURCES_PATH = "TLUIToolkit/Prefabs/ProgressUI";

        // Define the menu path in the Unity editor's GameObject menu
        private const string MENU_PATH = "GameObject/ThreeLinesUI/UIToolkit/ProgressUI";
        private const int MENU_PRIORITY = 0; // Priority for the menu item

        /// <summary>
        /// Creates a new instance of the ProgressUI prefab in the scene.
        /// This method is invoked when the user clicks the menu item.
        /// </summary>
        [MenuItem(MENU_PATH, false, MENU_PRIORITY)]
        private static void CreateProgressUI()
        {
            // Call the centralized creation logic from TLUIToolkitCreationEditor
            TLUIToolkitCreationEditor.CreateUIElementFromPrefab(
                PREFAB_RESOURCES_PATH,
                "Create ProgressUI", // Undo name
                MENU_PATH // Menu path for error messages
            );
        }

        /// <summary>
        /// Validation method to determine if the "Create ProgressUI" menu item should be enabled.
        /// It's enabled only when the editor is not in play mode.
        /// </summary>
        /// <returns>True if the menu item should be enabled, false otherwise.</returns>
        [MenuItem(MENU_PATH, true)]
        private static bool ValidateCreateProgressUI()
        {
            // The menu item should only be available when not in play mode.
            return !EditorApplication.isPlaying;
        }

        /// <summary>
        /// Provides an alternative way to create the ProgressUI via the Assets/Create menu.
        /// This is optional but can be convenient.
        /// </summary>
        [MenuItem("Assets/Create/TLUIToolkit/UI/ProgressUI", false, 81)]
        private static void CreateProgressUIFromAssets()
        {
            // Call the centralized creation logic from TLUIToolkitCreationEditor
            TLUIToolkitCreationEditor.CreateUIElementFromPrefab(
                PREFAB_RESOURCES_PATH,
                "Create ProgressUI", // Undo name
                "Assets/Create/TLUIToolkit/UI/ProgressUI" // Menu path for error messages
            );
        }
    }
}
