Shader "Runner/PlayerGlitch"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0, 0.5, 1, 1)
        _GlitchIntensity("Glitch Intensity", Range(0, 1)) = 0.0
        _GlitchSpeed("Glitch Speed", Float) = 20.0
        _FresnelPower("Fresnel Power", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

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
                float _GlitchIntensity;
                float _GlitchSpeed;
                float _FresnelPower;
            CBUFFER_END

            // Simple pseudo-random noise
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                // Construct logic for Glitch
                if (_GlitchIntensity > 0.01)
                {
                    float time = _Time.y * _GlitchSpeed;
                    float r = random(float2(time, positionWS.y)); // Random per Y slice
                    
                    // Only displace some slices
                    if (r > 0.8)
                    {
                        float jump = (r - 0.8) * 5.0 * _GlitchIntensity;
                        positionWS.x += jump;
                    }
                }
                
                ApplyCurvedWorld(positionWS);
                
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Fresnel
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                float3 normal = normalize(input.normalWS);
                float NdotV = 1.0 - saturate(dot(normal, viewDir));
                float fresnel = pow(NdotV, _FresnelPower);
                
                half3 color = _BaseColor.rgb + (fresnel * 0.5);
                
                // Color glitch?
                // if (_GlitchIntensity > 0.5) color.r += 0.5;
                
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
