Shader "Unlit/SimpleDissolve"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _EdgeColor("Edge Color", Color) = (1, 0.5, 0, 1)
        _TransitionColor("Transition Color", Color) = (1, 1, 1, 1)
        _DissolveHeight("Dissolve Plane Height", Range(0, 1)) = 0.0
        _Alpha("Alpha", Range(0, 1)) = 1.0
        _EdgeWidth("Edge Width", Float) = 0.1
        _TransitionPhase("Transition Phase (0 = dissolve, 1 = appear)", Float) = 0.0
        _BoundsMinY("Bounds Min Y", Float) = -1.0
        _BoundsMaxY("Bounds Max Y", Float) = 1.0
        _NoiseTex("Noise Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            Offset -1, -1 // pull closer to camera in depth buffer

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                // float3 objectPos : TEXCOORD2;
                float2 uv : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _EdgeColor;
            float4 _TransitionColor;
            float _DissolveHeight;
            float _Alpha;
            float _EdgeWidth;
            float _TransitionPhase;
            float _BoundsMinY;
            float _BoundsMaxY;

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // OUT.objectPos = IN.positionOS.xyz;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv).r;
                float heightDiff = IN.worldPos.y - lerp(_BoundsMinY, _BoundsMaxY, _DissolveHeight);
                float dissolveFactor = lerp(heightDiff, -heightDiff, _TransitionPhase);

                dissolveFactor += (noise - 0.5) * _EdgeWidth;

                clip(dissolveFactor);

                float edgeFade = smoothstep(0.0, _EdgeWidth, dissolveFactor);
                float4 mainColor = lerp(_TransitionColor, _BaseColor, edgeFade);

                mainColor.rgb = lerp(_EdgeColor.rgb, mainColor.rgb, edgeFade);
                mainColor.a *= edgeFade;
                mainColor.a *= _Alpha;

                return mainColor;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 objectPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            float _DissolveHeight;
            float _EdgeWidth;
            float _TransitionPhase;
            float _BoundsMinY;
            float _BoundsMaxY;

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.objectPos = IN.positionOS.xyz;
                OUT.worldPos = worldPos;
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            void fragShadow(Varyings IN)
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv).r;
                float heightDiff = IN.worldPos.y - lerp(_BoundsMinY, _BoundsMaxY, _DissolveHeight);
                float dissolveFactor = lerp(heightDiff, -heightDiff, _TransitionPhase);
                dissolveFactor += (noise - 0.5) * _EdgeWidth;

                clip(dissolveFactor);
            }
            ENDHLSL
        }

    }
}