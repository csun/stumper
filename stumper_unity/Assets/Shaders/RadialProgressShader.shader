Shader "Stumper/RadialProgressShader"
{
    Properties
    {
        // Unused but required for sprites. Maybe a better way to do this.
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Smoothing ("Smoothing", Float) = 0.1
        _OuterRadius ("Outer Radius", Float) = 0.5
        _InnerRadius ("Inner Radius", Float) = 0.4
        _Progress ("Progress", Float) = 1.0
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float _OuterRadius; 
            float _InnerRadius; 
            float _Progress;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);


            Varyings UnlitVertex(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex);
                // Center UV about 0.5, 0.5
                o.uv = v.uv - 0.5;
                return o;
            }

            half4 UnlitFragment(Varyings i) : SV_Target
            {
                const float DOUBLE_PI = 2 * 3.141592653589793238462;

                // TODO Look at how rounded corners library handles antialiasing - probably sdf
                float radius = length(i.uv);
                float progress = acos(dot(i.uv, float2(0, 1)) / radius) / DOUBLE_PI;
                progress = i.uv.x <= 0 ? 1 - progress : progress;

                clip(radius > _OuterRadius || radius < _InnerRadius || progress > _Progress ? -1 : 1);
                return _Color;
            }
            ENDHLSL
        }
    }
}
