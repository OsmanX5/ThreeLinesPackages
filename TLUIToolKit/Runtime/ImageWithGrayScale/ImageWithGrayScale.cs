using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ThreeLines.Helpers;
using UnityEngine;
using UnityEngine.UI;


namespace TLUIToolkit
{
    [ExecuteInEditMode]                             //Required to check the OnEnable function
    [DisallowMultipleComponent]                     //You can only have one of these in every object.
    [RequireComponent(typeof(RectTransform))]
    public class ImageWithGrayScale : MonoBehaviour
    {
        private static readonly string SHADER_NAME = "Shader Graphs/SH_ImageWithGrayScale";

        private static readonly int prop_GrayScaleLevel = Shader.PropertyToID("_GrayScaleLevel");

        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        [Range(0,1)]
        public float grayScaleLevel = 1f;

        private Material material;
        private Vector4 outerUV = new Vector4(0, 0, 1, 1);

        [HideInInspector, SerializeField] private MaskableGraphic image;

        private void OnValidate()
        {
            Validate();
            Refresh();
        }

        private void OnDestroy()
        {
            if (image != null)
            {
                image.material = null;      //This makes so that when the component is removed, the UI material returns to null
            }

            material.CleanDestroy();
            image = null;
            material = null;
        }

        private void OnEnable()
        {
            Validate();
            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (enabled && material != null)
            {
                Refresh();
            }
        }

        public void Validate()
        {
            if (material == null)
            {
                material = new Material(Shader.Find(SHADER_NAME));
            }

            if (image == null)
            {
                TryGetComponent(out image);
            }

            if (image != null)
            {
                image.material = material;
            }

            if (image is Image uiImage && uiImage.sprite != null)
            {
                outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
            }
        }

        public void Refresh()
        {
            var rect = ((RectTransform)transform).rect;
            material.SetFloat(prop_GrayScaleLevel, grayScaleLevel);
        }


        enum GradientAxis
        {
            X,
            Y,
        }
        enum GradientType
        {
            Linear,
            Center
        }
    }

}
