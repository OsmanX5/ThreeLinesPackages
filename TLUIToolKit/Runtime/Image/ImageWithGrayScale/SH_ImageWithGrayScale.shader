Shader "TLShaders/SH_ImageWithGrayScale"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _GrayScaleLevel ("Gray Scale Level",Range(0, 1)) = 1.0
        _GrayScaleAxis ("Gray Scale Axis", int) = 0
        _GrayScaleDarken("Gray Scale Darken", Range(0, 1)) = 0.5
        // --- Mask support ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
                // --- Mask support ---
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"          
            #include "SDFUtils.cginc"
            #include "ShaderSetup.cginc"
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _GrayScaleLevel;
            float _GrayScaleAxis;
            float _GrayScaleDarken;
            fixed4 frag (v2f i) : SV_Target {
                float2 uvSample = i.uv;

                half4 color = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                if (color.a <= 0) {
                    return color;
                }

                float alpha = color.a;

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif
 
                half gray = dot(color, float3(0.299, 0.587, 0.114)) * _GrayScaleDarken;
                half4 grayed = half4(gray, gray, gray,alpha);
                float uvVal = _GrayScaleAxis == 0 ? i.uv.x : i.uv.y;
                if(_GrayScaleLevel>=uvVal){
                    return color;
                }
                return grayed;//mixAlpha(tex2D(_MainTex, i.uv), grayed, alpha);
            }

            ENDCG
        }
    }
}
