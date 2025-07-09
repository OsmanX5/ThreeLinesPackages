using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TLUIToolkit.Editor
{
    public class ImageWithGradientEditor
    {
        [MenuItem("GameObject/ThreeLinesUI/UIToolkit/Image/Create Image with Gradient Background ", false, 10)]
        public static void CreateGradientBackgroundImage()
        {
            Transform parentTransform = ImageWithEditorUtility.GetOrCreateCanvasParent();
            GameObject imageObject = ImageWithEditorUtility.CreateImageWithRectTransform(
                "GradientBackground_Image",
                new Vector2(100f, 100f),
                parentTransform
            );

            imageObject.AddComponent<TLUIToolkit.ImageWithGradient>();

            ImageWithEditorUtility.SelectAndMarkDirty(imageObject);

            Debug.Log($"Created {imageObject.name} with dimensions 100x100 and added ImageWithGradientEditor component.");
        }

        [MenuItem("CONTEXT/Image/Convert to Gradient")]
        public static void ConvertToGradient(MenuCommand command)
        {
            Image targetImage = (Image)command.context;

            if (targetImage == null)
            {
                Debug.LogError("No Image component found in selection.");
                return;
            }

            if (targetImage.gameObject.GetComponent<TLUIToolkit.ImageWithGradient>() == null)
            {
                targetImage.gameObject.AddComponent<TLUIToolkit.ImageWithGradient>();
                Debug.Log($"Successfully converted {targetImage.gameObject.name} to gradient background.");
                EditorUtility.SetDirty(targetImage.gameObject);
            }
            else
            {
                Debug.LogWarning($"{targetImage.gameObject.name} already has a ImageWithGradientEditor component.");
            }
        }

        [MenuItem("CONTEXT/Image/Convert to Gradient", true)]
        public static bool ValidateConvertToGradient(MenuCommand command)
        {
            Image targetImage = (Image)command.context;
            return targetImage != null;
        }
    }
}