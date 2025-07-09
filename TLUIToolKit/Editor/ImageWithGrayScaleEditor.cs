using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TLUIToolkit.Editor
{
    public class ImageWithGrayscaleEditor
    {
        [MenuItem("GameObject/ThreeLinesUI/UIToolkit/Image/Create Grayscale Image", false, 11)]
        public static void CreateGrayscaleImage()
        {
            Transform parentTransform = ImageWithEditorUtility.GetOrCreateCanvasParent();
            GameObject imageObject = ImageWithEditorUtility.CreateImageWithRectTransform(
                "Grayscale_Image",
                new Vector2(100f, 100f),
                parentTransform
            );

            // Add the GrayscaleEffect component
            imageObject.AddComponent<ImageWithGrayScale>();

            ImageWithEditorUtility.SelectAndMarkDirty(imageObject);

            Debug.Log($"Created {imageObject.name} with dimensions 100x100 and added GrayscaleEffect component.");
        }

        /// <summary>
        /// Converts selected Image to use a grayscale effect.
        /// Shows in context menu when Image component is selected.
        /// </summary>
        [MenuItem("CONTEXT/Image/Convert to Grayscale")]
        public static void ConvertToGrayscale(MenuCommand command)
        {
            Image targetImage = (Image)command.context;

            if (targetImage == null)
            {
                Debug.LogError("No Image component found in selection.");
                return;
            }

            // Check if GrayscaleEffect already exists to prevent duplicates
            if (targetImage.gameObject.GetComponent<ImageWithGrayScale>() == null)
            {
                targetImage.gameObject.AddComponent<ImageWithGrayScale>();
                Debug.Log($"Successfully converted {targetImage.gameObject.name} to grayscale.");
                EditorUtility.SetDirty(targetImage.gameObject); // Mark the object as dirty to save changes
            }
            else
            {
                Debug.LogWarning($"{targetImage.gameObject.name} already has a GrayscaleEffect component.");
            }
        }

        /// <summary>
        /// Validates if Convert to Grayscale menu item should be shown.
        /// Only shows when an Image component is selected.
        /// </summary>
        [MenuItem("CONTEXT/Image/Convert to Grayscale", true)]
        public static bool ValidateConvertToGrayscale(MenuCommand command)
        {
            Image targetImage = (Image)command.context;
            // The menu item is enabled if an Image component is selected.
            return targetImage != null;
        }
    }
}