using UnityEngine;
using UnityEditor;

namespace TLUIToolkit.Editor
{
    public static class HorizontalSelectorEditor
    {
        // Define the path where the HorizontalSelector prefab should be located
        // This assumes you have a folder named 'Resources' in your project,
        // and inside it, a 'Prefabs' folder, containing 'HorizontalSelectorUI.prefab'.
        private const string PREFAB_RESOURCES_PATH = "TLUIToolkit/Prefabs/TLUIHorizntalSelector";

        // Define the menu path in the Unity editor's GameObject menu
        private const string MENU_PATH = "GameObject/ThreeLinesUI/UIToolkit/Horizontal Selector";
        private const int MENU_PRIORITY = 0; // Priority for the menu item

        /// <summary>
        /// Creates a new instance of the HorizontalSelectorUI prefab in the scene.
        /// This method is invoked when the user clicks the menu item.
        /// </summary>
        [MenuItem(MENU_PATH, false, MENU_PRIORITY)]
        private static void CreateHorizontalSelectorUI()
        {
            // Call the centralized creation logic from TLUIToolkitCreationEditor
            TLUIToolkitCreationEditor.CreateUIElementFromPrefab(
                PREFAB_RESOURCES_PATH,
                "Create Horizontal Selector UI", // Undo name
                MENU_PATH // Menu path for error messages
            );
        }

        /// <summary>
        /// Validation method to determine if the "Create Horizontal Selector" menu item should be enabled.
        /// It's enabled only when the editor is not in play mode.
        /// </summary>
        /// <returns>True if the menu item should be enabled, false otherwise.</returns>
        [MenuItem(MENU_PATH, true)]
        private static bool ValidateCreateHorizontalSelectorUI()
        {
            // The menu item should only be available when not in play mode.
            return !EditorApplication.isPlaying;
        }

        /// <summary>
        /// Provides an alternative way to create the HorizontalSelectorUI via the Assets/Create menu.
        /// This is optional but can be convenient.
        /// </summary>
        [MenuItem("Assets/Create/TLUIToolkit/UI/Horizontal Selector", false, 81)]
        private static void CreateHorizontalSelectorUIFromAssets()
        {
            // Call the centralized creation logic from TLUIToolkitCreationEditor
            TLUIToolkitCreationEditor.CreateUIElementFromPrefab(
                PREFAB_RESOURCES_PATH,
                "Create Horizontal Selector UI", // Undo name
                "Assets/Create/TLUIToolkit/UI/Horizontal Selector" // Menu path for error messages
            );
        }
    }
}
