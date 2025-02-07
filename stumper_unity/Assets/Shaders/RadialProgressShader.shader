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

            // https://www.ronja-tutorials.com/post/034-2d-sdf-basics/#rotating
            float2 rotate(float2 samplePosition, float theta){
                float sine, cosine;
                sincos(theta, sine, cosine);
                return float2(cosine * samplePosition.x + sine * samplePosition.y, cosine * samplePosition.y - sine * samplePosition.x);
            }

            // Adapted from the pie SDF here - https://iquilezles.org/articles/distfunctions2d/
            float wedge_sdf(float2 uv, float amount)
            {
                // Not super elegant, but used for bounds checking and prevents some artifacts with the wedge
                // not closing all the way when amount is very close to bounds.
                const float EPSILON = 0.0001;
                if (amount < EPSILON) {
                    return -length(uv);
                }
                if (amount > 1 - EPSILON)
                {
                    return length(uv);
                }

                float theta = amount * 3.14159;
                float2 sc = float2(sin(theta), cos(theta));
                // Without this, the wedge is opens symmetrically. This rotation pins one side of the wedge
                // to pointing straight up.
                uv = rotate(uv, -theta);

                uv.x = abs(uv.x);
                float m = length(uv - sc*max(dot(uv,sc),0.0));
                
                // I don't fully understand the math behind the inner part of this sign expression.
                // You need it to differentiate between inside / outside the wedge, though.
                return -m*sign(sc.y*uv.x-sc.x*uv.y);
            }

            float open_ring_sdf(float2 uv, float outer_radius, float inner_radius, float amount) {
                float radius = length(uv);
                float outside_dist = radius - outer_radius;
                float inside_dist = inner_radius - radius;

                float ring_sdf = max(outside_dist, inside_dist);
                return max(wedge_sdf(uv, 1 - amount), ring_sdf);
            }

            half4 UnlitFragment(Varyings i) : SV_Target
            {
                float dist = open_ring_sdf(i.uv, _OuterRadius, _InnerRadius, _Progress);
                float smoothing_width = fwidth(dist);
                float alpha = smoothstep(0, -smoothing_width, dist);

                clip(alpha);

                return half4(_Color.xyz, alpha);
            }
            ENDHLSL
        }
    }
}
