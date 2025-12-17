Shader "Runner/NeonGridRoad"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.02, 0.02, 0.02, 1) // Matte Black
        _SheenColor("Sheen Color", Color) = (0, 0.2, 0.5, 1) // Blue Tint
        _SheenPower("Sheen Power", Float) = 0.5 // Broad falloff
        _Smoothness("Smoothness", Range(0, 1)) = 0.2 // Matte
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
                float4 _SheenColor;
                float _SheenPower;
                float _Smoothness;
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
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                float3 normal = normalize(input.normalWS);
                
                // Fresnel / Sheen
                float NdotV = 1.0 - saturate(dot(normal, viewDir));
                float sheen = pow(NdotV, 1.0 / _SheenPower); // Inverse power for broader spread if < 1
                
                half3 color = _BaseColor.rgb + (_SheenColor.rgb * sheen);
                
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
