using UnityEngine;
using UnityEditor;

namespace ThreeLinesUI.UIToolkit.Editor
{
    public static class TasksCheckUIEditor
    {
        private const string PREFAB_RESOURCES_PATH = "Prefabs/TasksCheckUI";
        private const string MENU_PATH = "GameObject/ThreeLinesUI/UIToolkit/TasksCheckUI";
        private const int MENU_PRIORITY = 0;

        [MenuItem(MENU_PATH, false, MENU_PRIORITY)]
        private static void CreateTasksCheckUI()
        {
            CreateTasksCheckUIPrefab();
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ValidateCreateTasksCheckUI()
        {
            // Only show the menu item if we're in a scene
            return !EditorApplication.isPlaying;
        }

        private static void CreateTasksCheckUIPrefab()
        {
            // Load the prefab from the specified path
            GameObject prefab = Resources.Load<GameObject>(PREFAB_RESOURCES_PATH);

            if (prefab == null)
            {
                Debug.LogError($"TasksCheckUI prefab not found at path: {PREFAB_RESOURCES_PATH}");
                EditorUtility.DisplayDialog("Error",
                    $"Could not find TasksCheckUI prefab at:\n{PREFAB_RESOURCES_PATH}\n\nPlease ensure the prefab exists at this location.",
                    "OK");
                return;
            }

            // Get the currently selected transform
            Transform parent = Selection.activeTransform;

            // Check if parent has RectTransform (UI element)
            if (parent != null)
            {
                RectTransform parentRectTransform = parent.GetComponent<RectTransform>();
                if (parentRectTransform == null)
                {
                    Debug.LogError($"Selected parent '{parent.name}' is not a UI element (no RectTransform component). " +
                                 "Please select a UI element (Canvas, Panel, etc.) as parent for TasksCheckUI.");
                    EditorUtility.DisplayDialog("Invalid Parent",
                        $"Selected parent '{parent.name}' is not a UI element.\n\n" +
                        "TasksCheckUI must be created as a child of a UI element with RectTransform component.\n\n" +
                        "Please select a Canvas, Panel, or other UI element as parent.",
                        "OK");
                    return;
                }
            }
            else
            {
                // If no parent is selected, try to find a Canvas in the scene
                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    parent = canvas.transform;
                }
                else
                {
                    Debug.LogError("No UI parent selected and no Canvas found in scene. " +
                                 "Please select a UI element as parent or create a Canvas first.");
                    EditorUtility.DisplayDialog("No UI Parent",
                        "No UI parent selected and no Canvas found in the scene.\n\n" +
                        "Please select a UI element (Canvas, Panel, etc.) as parent, " +
                        "or create a Canvas in the scene first.",
                        "OK");
                    return;
                }
            }

            // Instantiate the prefab
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (instance == null)
            {
                Debug.LogError("Failed to instantiate TasksCheckUI prefab");
                return;
            }

            // Set the parent (we know it has RectTransform at this point)
            instance.transform.SetParent(parent, false);

            // Position the instance
            PositionNewInstance(instance);

            // Select the newly created instance
            Selection.activeGameObject = instance;

            // Register undo
            Undo.RegisterCreatedObjectUndo(instance, "Create TasksCheckUI");

            Debug.Log($"TasksCheckUI prefab created successfully as child of '{parent.name}': {instance.name}");
        }

        private static void PositionNewInstance(GameObject instance)
        {
            // Reset the transform
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Set up RectTransform properties for UI element
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Center the UI element
                rectTransform.anchoredPosition = Vector2.zero;

                // Set default anchors if not already set
                if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.zero)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                }
            }
        }

        // Alternative method to create via Assets menu (optional)
        [MenuItem("Assets/Create/ThreeLinesUI/UIToolkit/TasksCheckUI", false, 81)]
        private static void CreateTasksCheckUIFromAssets()
        {
            CreateTasksCheckUIPrefab();
        }
    }
}