using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThreeLines.Helpers;
using Sirenix.OdinInspector;
namespace TLUIToolkit
{
    [ExecuteInEditMode]                             //Required to check the OnEnable function
    [DisallowMultipleComponent]                     //You can only have one of these in every object.
    [RequireComponent(typeof(RectTransform))]
    public class ImageWithGradient : MonoBehaviour
    {
        private static readonly string SHADER_NAME = $"Shader Graphs/SH_{nameof(ImageWithGradient)}";
        
        private static readonly int Props = Shader.PropertyToID("_WidthHeightRadius");
        private static readonly int prop_OuterUV = Shader.PropertyToID("_OuterUV");
        private static readonly int prop_MainColor = Shader.PropertyToID("_MainColor");
        private static readonly int prop_SecondaryColor = Shader.PropertyToID("_SecondaryColor");
        private static readonly int prop_GradientType = Shader.PropertyToID("_GRADIENTTYPE");
        private static readonly int prop_GradientAxis = Shader.PropertyToID("_GRADIENTAXIS");
        private static readonly int prop_BorderThickness = Shader.PropertyToID("_BorderThickness");
        private static readonly int prop_MidPoint = Shader.PropertyToID("_MidPoint");
        static readonly float MAX_BORDER_THICKNESS_IN_SHADER = 0.01f; 
        public float radius = 10f;
        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        Color mainColor = TLUIColors.PrimaryColor;

        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        Color secondaryColor = TLUIColors.SecondaryColor;

        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        [EnumToggleButtons]
        private GradientType gradientType = GradientType.Linear;

        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        [EnumToggleButtons]
        private GradientAxis gradientAxis = GradientAxis.X;

        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        [ShowIf(nameof(gradientType), GradientType.Linear)]
        [Range(0, 1)]
        float midPoint = 0.5f; //This is used for the center gradient type, it is the point where the gradient will be centered. It is between 0 and 1, where 0 is the left side and 1 is the right side of the image.
        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        [Range(0,1)]
        float borderThickness = 0f; 
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
            material.SetVector(Props, new Vector4(rect.width, rect.height, radius * 2, 0));
            material.SetVector(prop_OuterUV, outerUV);
            material.SetColor(prop_MainColor, mainColor);
            material.SetColor(prop_SecondaryColor, secondaryColor);
            material.SetInt(prop_GradientType, (int)gradientType);
            material.SetInt(prop_GradientAxis, (int)gradientAxis);
            material.SetFloat(prop_BorderThickness, borderThickness * MAX_BORDER_THICKNESS_IN_SHADER);
            material.SetFloat(prop_MidPoint, midPoint);
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
