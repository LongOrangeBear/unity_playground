Shader "Runner/CollectibleGlow"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 0.8, 0, 1)
        _EmissionColor("Emission Color", Color) = (1, 0.5, 0, 1)
        _PulseSpeed("Pulse Speed", Float) = 3.0
        _PulseMin("Pulse Min", Range(0, 1)) = 0.5
        _PulseMax("Pulse Max", Range(0, 2)) = 1.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/Includes/CurvedWorld.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;
                float _PulseSpeed;
                float _PulseMin;
                float _PulseMax;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                ApplyCurvedWorld(positionWS);
                
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Basic pulsing logic using Sine wave
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5; // 0..1
                float intensity = lerp(_PulseMin, _PulseMax, pulse);
                
                // Add rim light for extra pop?
                // For collectibles, just flat bright color + emission is usually enough
                
                half3 albedo = _BaseColor.rgb;
                half3 emission = _EmissionColor.rgb * intensity;
                
                return half4(albedo + emission, 1.0);
            }
            ENDHLSL
        }
    }
}
