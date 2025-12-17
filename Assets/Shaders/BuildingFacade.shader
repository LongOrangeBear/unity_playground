Shader "Runner/BuildingFacade"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.05, 0.05, 0.1, 1)
        _WindowColor("Window Color", Color) = (1, 0.9, 0.5, 1)
        _EmissionIntensity("Emission Intensity", Float) = 5.0
        _WindowScale("Window Scale", Float) = 1.0
        _WindowDensity("Window Density", Range(0,1)) = 0.4
        
        _RimColor("Rim Color", Color) = (0.2, 0.5, 1, 1)
        _RimPower("Rim Power", Float) = 3.0
        
        _GradientHeight("Gradient Height", Float) = 10.0
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
                float height : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _WindowColor;
                float _EmissionIntensity;
                float _WindowScale;
                float _WindowDensity;
                float4 _RimColor;
                float _RimPower;
                float _GradientHeight;
            CBUFFER_END

            // Pseudo-random function
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 originalPositionWS = positionWS; // Capture original for UV/Height
                output.height = positionWS.y; 
                
                ApplyCurvedWorld(positionWS);
                
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // World Space UVs for Triplanar-ish vertical mapping
                // Use Original Pos to avoid texture swimming/distortion when curving
                float2 worldUV = float2(originalPositionWS.x + originalPositionWS.z, originalPositionWS.y);
                output.uv = worldUV * _WindowScale;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Procedural Windows
                float2 cellID = floor(input.uv);
                float randVal = random(cellID);
                
                // Determine if this cell is a window
                float isWindow = step(1.0 - _WindowDensity, randVal);
                
                // Check if inside the "window frame" (border)
                float2 cellUV = frac(input.uv);
                float borderX = step(0.1, cellUV.x) * step(cellUV.x, 0.9);
                float borderY = step(0.1, cellUV.y) * step(cellUV.y, 0.9);
                float inWindow = borderX * borderY;

                half3 albedo = _BaseColor.rgb;
                half3 emission = float3(0,0,0);

                if (isWindow > 0.5 && inWindow > 0.5)
                {
                    // Randomize window brightness slightly
                    float brightness = 0.5 + 0.5 * randVal;
                    emission = _WindowColor.rgb * _EmissionIntensity * brightness;
                    albedo = _WindowColor.rgb; // Make base color lighter too
                }

                // 2. Vertical Gradient (Fade bottom)
                // Normalize height from 0 to _GradientHeight
                float gradient = saturate(input.height / _GradientHeight);
                albedo *= gradient;
                emission *= gradient; // Dim windows at bottom too? Maybe.
                
                // 3. Rim Light
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                float3 normal = normalize(input.normalWS);
                float NdotV = 1.0 - saturate(dot(normal, viewDir));
                float rim = pow(NdotV, _RimPower);
                
                half3 rimEmission = _RimColor.rgb * rim;
                
                half3 finalColor = albedo + emission + rimEmission;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
