using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;

namespace TLUIToolkit.Editor
{
    public class GradientBackgroundEditor
    {
        
        [MenuItem("GameObject/ThreeLinesUI/UIToolkit/Create Gradient Background Image", false, 10)]
        public static void CreateGradientBackgroundImage()
        {
            // Create a new GameObject with Image component
            GameObject imageObject = new GameObject("GradientBackground_Image");

            // Add Image component
            Image imageComponent = imageObject.AddComponent<Image>();

            // Set the RectTransform properties (100x100 dimensions)
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 100f);

            // Check if currently selected object has RectTransform
            Transform parentTransform = null;
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject != null && selectedObject.GetComponent<RectTransform>() != null)
            {
                // Parent to the selected RectTransform
                parentTransform = selectedObject.transform;
                Debug.Log($"Parenting image to selected RectTransform: {selectedObject.name}");
            }
            else
            {
                // Parent to Canvas if one exists, otherwise create one
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    // Create a new Canvas
                    GameObject canvasObject = new GameObject("Canvas");
                    canvas = canvasObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();

                    Debug.Log("Created new Canvas for the image.");
                }
                parentTransform = canvas.transform;
            }

            // Set the image as child of determined parent
            imageObject.transform.SetParent(parentTransform, false);
            imageObject.AddComponent<GradientBackground>();
            // Select the created object in hierarchy
            Selection.activeGameObject = imageObject;

            // Mark scene as dirty to ensure changes are saved
            EditorUtility.SetDirty(imageObject);

            Debug.Log($"Created {imageObject.name} with dimensions 100x100 and applied gradient material.");
        }

        /// <summary>
        /// Converts selected Image to use gradient material
        /// Shows in context menu when Image component is selected
        /// </summary>
        [MenuItem("CONTEXT/Image/Convert to Gradient")]
        public static void ConvertToGradient(MenuCommand command)
        {
            Image targetImage = (Image)command.context;

            if (targetImage == null)
            {
                Debug.LogError("No Image component found in selection.");
                return;
            }

            // Apply gradient material
            if (ApplyGradientMaterial(targetImage))
            {
                Debug.Log($"Successfully converted {targetImage.gameObject.name} to gradient background.");
            }
            else
            {
                Debug.LogError($"Failed to convert {targetImage.gameObject.name} to gradient background.");
            }
        }

        /// <summary>
        /// Validates if Convert to Gradient menu item should be shown
        /// Only shows when Image component is selected
        /// </summary>
        [MenuItem("CONTEXT/Image/Convert to Gradient", true)]
        public static bool ValidateConvertToGradient(MenuCommand command)
        {
            Image targetImage = (Image)command.context;
            return targetImage != null;
        }
        public static GameObject CreateGradientImage()
        {
            GameObject imageObject = new GameObject("GradientBackground_Image");
            Image imageComponent = imageObject.AddComponent<Image>();

            // Set dimensions
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 100f);

            ApplyGradientMaterial(imageComponent);

            return imageObject;
        }

        /// <summary>
        /// Utility method to apply the gradient material to an existing Image component
        /// </summary>
        /// <param name="targetImage">The Image component to apply the material to</param>
        /// <returns>True if material was successfully applied</returns>
        public static bool ApplyGradientMaterial(Image targetImage)
        {
            if (targetImage == null)
            {
                Debug.LogError("Target Image component is null.");
                return false;
            }
            targetImage.AddComponent<GradientBackground>();
            return true;
        }
    }
}