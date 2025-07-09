using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TLUIToolkit.Editor
{
    public static class ImageWithEditorUtility
    {
        public static GameObject CreateImageWithRectTransform(string name, Vector2 size, Transform parent)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.AddComponent<Image>();
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            imageObject.transform.SetParent(parent, false);
            return imageObject;
        }

        public static Transform GetOrCreateCanvasParent()
        {
            Transform parentTransform = null;
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject != null && selectedObject.GetComponent<RectTransform>() != null)
            {
                parentTransform = selectedObject.transform;
                Debug.Log($"Parenting to selected RectTransform: {selectedObject.name}");
            }
            else
            {
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObject = new GameObject("Canvas");
                    canvas = canvasObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                    Debug.Log("Created new Canvas.");
                }
                parentTransform = canvas.transform;
            }
            return parentTransform;
        }

        public static void SelectAndMarkDirty(GameObject obj)
        {
            Selection.activeGameObject = obj;
            EditorUtility.SetDirty(obj);
        }
    }
}
