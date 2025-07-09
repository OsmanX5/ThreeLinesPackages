using UnityEngine;
using UnityEditor;

namespace TLUIToolkit.Editor
{
    /// <summary>
    /// Provides utility methods for creating UI elements from prefabs in the Unity Editor.
    /// This class centralizes common logic for loading, instantiating, parenting,
    /// positioning, and registering undo operations for UI prefabs.
    /// </summary>
    public static class TLUIToolkitCreationEditor
    {
        /// <summary>
        /// Creates a new instance of a UI prefab in the scene, handling loading,
        /// parent determination, instantiation, positioning, and undo registration.
        /// </summary>
        /// <param name="prefabResourcesPath">The path to the prefab in a Resources folder (e.g., "TLUIToolkit/Prefabs/MyUIElement").</param>
        /// <param name="undoName">The name to be used for the Undo operation (e.g., "Create My UI Element").</param>
        /// <param name="menuPath">The menu path from which this creation was invoked, used for error messages.</param>
        /// <returns>The newly created GameObject instance, or null if creation failed.</returns>
        public static GameObject CreateUIElementFromPrefab(string prefabResourcesPath, string undoName, string menuPath)
        {
            // 1. Load the prefab from the Resources folder.
            GameObject prefab = Resources.Load<GameObject>(prefabResourcesPath);

            if (prefab == null)
            {
                // If the prefab is not found, log an error and display a dialog to the user.
                Debug.LogError($"Prefab not found at path: {prefabResourcesPath} for menu item: {menuPath}");
                EditorUtility.DisplayDialog("Error",
                    $"Could not find prefab at:\n'{prefabResourcesPath}'\n\n" +
                    "Please ensure the prefab exists at this location (e.g., Assets/Resources/TLUIToolkit/Prefabs/YourPrefab.prefab).",
                    "OK");
                return null;
            }

            // 2. Determine the parent for the new instance.
            // Try to use the currently selected GameObject as parent.
            Transform parent = Selection.activeTransform;

            // If a parent is selected, ensure it's a UI element (has a RectTransform).
            if (parent != null)
            {
                RectTransform parentRectTransform = parent.GetComponent<RectTransform>();
                if (parentRectTransform == null)
                {
                    // If the selected parent is not a UI element, inform the user.
                    Debug.LogError($"Selected parent '{parent.name}' is not a UI element (no RectTransform component). " +
                                   $"Please select a UI element (Canvas, Panel, etc.) as parent for {undoName}.");
                    EditorUtility.DisplayDialog("Invalid Parent",
                        $"Selected parent '{parent.name}' is not a UI element.\n\n" +
                        $"{undoName} must be created as a child of a UI element with a RectTransform component.\n\n" +
                        "Please select a Canvas, Panel, or other UI element as parent.",
                        "OK");
                    return null;
                }
            }
            else
            {
                // If no parent is selected, try to find an existing Canvas in the scene.
                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    parent = canvas.transform;
                }
                else
                {
                    // If no parent is selected and no Canvas is found, inform the user.
                    Debug.LogError("No UI parent selected and no Canvas found in scene. " +
                                   $"Please select a UI element as parent or create a Canvas first for {undoName}.");
                    EditorUtility.DisplayDialog("No UI Parent",
                        "No UI parent selected and no Canvas found in the scene.\n\n" +
                        "Please select a UI element (Canvas, Panel, etc.) as parent, " +
                        "or create a Canvas in the scene first.",
                        "OK");
                    return null;
                }
            }

            // 3. Instantiate the prefab.
            // PrefabUtility.InstantiatePrefab is used to correctly instantiate prefabs in the editor.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (instance == null)
            {
                Debug.LogError($"Failed to instantiate prefab from path: {prefabResourcesPath}");
                return null;
            }

            // 4. Set the parent of the instantiated object.
            // 'false' for worldPositionStays ensures local position/rotation/scale are reset.
            instance.transform.SetParent(parent, false);

            // 5. Position and configure the new instance for UI.
            PositionNewUIInstance(instance);

            // 6. Select the newly created instance in the Hierarchy window.
            Selection.activeGameObject = instance;

            // 7. Register the creation with Undo system, allowing the user to undo the action.
            Undo.RegisterCreatedObjectUndo(instance, undoName);

            Debug.Log($"{undoName} prefab created successfully as child of '{parent.name}': {instance.name}");
            return instance;
        }

        /// <summary>
        /// Resets the transform and sets common RectTransform properties for a new UI instance.
        /// This ensures the UI element is correctly positioned and anchored within its parent.
        /// </summary>
        /// <param name="instance">The newly created GameObject instance.</param>
        public static void PositionNewUIInstance(GameObject instance)
        {
            // Reset local transform properties to default.
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Get the RectTransform component, which is essential for UI elements.
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Center the UI element within its parent.
                rectTransform.anchoredPosition = Vector2.zero;

                // Set default anchors to center if they are not already set (e.g., from prefab).
                // This ensures it's centered by default if the prefab doesn't specify anchors.
                if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.zero)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                }
            }
        }
    }
}
