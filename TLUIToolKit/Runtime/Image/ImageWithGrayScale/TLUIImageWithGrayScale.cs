using Sirenix.OdinInspector;
using System;
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
    public class TLUIImageWithGrayScale : MonoBehaviour
    {
        private static readonly string SHADER_NAME = $"TLShaders/SH_{nameof(TLUIImageWithGrayScale)}";

        private static readonly int prop_GrayScaleLevel = Shader.PropertyToID("_GrayScaleLevel");
        private static readonly int prop_GrayScaleAxis = Shader.PropertyToID("_GrayScaleAxis");
        private static readonly int prop_GrayScaleDarken = Shader.PropertyToID("_GrayScaleDarken");
        [SerializeField]
        [OnValueChanged(nameof(Refresh))]
        [Range(0,1)]
        public float grayScaleLevel =1;
        
        [SerializeField]
        [Range(0, 1)]
        float grayScaleDarken = 0.5f;

        [SerializeField]
        [EnumToggleButtons]
        GrayScaleAxis grayScaleAxis = GrayScaleAxis.X; //This is the axis on which the gray scale will be applied. X means horizontal, Y means vertical.
        private Material material;
        private Vector4 outerUV = new Vector4(0, 0, 1, 1);

        public float GrayScaleLevel
        {
            get => grayScaleLevel;
            set
            {

                grayScaleLevel = Mathf.Clamp01(value);
                Refresh();
            }
        }
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
            material.SetInt(prop_GrayScaleAxis, (int)grayScaleAxis);
            material.SetFloat(prop_GrayScaleDarken, grayScaleDarken); 
        }
        enum GrayScaleAxis
        {
            X =0,
            Y =1,
        }
    }

}
