Shader "Runner/ProceduralSkybox"
{
    Properties
    {
        _TopColor("Top Color", Color) = (0.1, 0, 0.2, 1)
        _BottomColor("Bottom Color", Color) = (0, 0, 0.1, 1)
        _HorizonColor("Horizon Color", Color) = (0.5, 0, 0.5, 1)
        _HorizonPower("Horizon Power", Float) = 3.0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
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
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float4 _HorizonColor;
                float _HorizonPower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Simple gradient based on Y
                float3 pos = normalize(input.positionOS);
                float y = pos.y;
                
                // Horizon mask (y near 0)
                float horizon = 1.0 - abs(y);
                horizon = pow(horizon, _HorizonPower);
                
                half3 color;
                if (y > 0)
                {
                    color = lerp(_HorizonColor.rgb, _TopColor.rgb, y);
                }
                else
                {
                    color = lerp(_HorizonColor.rgb, _BottomColor.rgb, -y);
                }
                
                // Add pulse? 
                // float pulse = sin(_Time.y) * 0.1;
                // color += pulse;

                // Procedural Symbols (Stars/Crosses)
                // Use polar coordinates interpretation roughly or just directional noise
                // Mapping: Sphere to UV (Equirectangular) or simple direction hashing
                
                // Simple directional noise
                float2 skyUV = pos.xz / (pos.y + 0.1); // Planar spread for top? 
                // Better: generic hash based on view direction
                
                // Scale direction to grid
                float3 dir = pos * 20.0;
                float3 cell = floor(dir);
                float3 fractPos = frac(dir);
                
                // Random value per cell
                float rnd = frac(sin(dot(cell, float3(12.9898, 78.233, 45.164))) * 43758.5453123);
                
                // 5% chance of star
                if (rnd > 0.98)
                {
                    // Draw small cross or dot
                    float dist = length(fractPos - 0.5);
                    float star = smoothstep(0.2, 0.1, dist);
                    
                    // Cross shape
                    float crossV = smoothstep(0.4, 0.2, abs(fractPos.x - 0.5)) * smoothstep(0.1, 0.05, abs(fractPos.y - 0.5));
                    float crossH = smoothstep(0.4, 0.2, abs(fractPos.y - 0.5)) * smoothstep(0.1, 0.05, abs(fractPos.x - 0.5));
                    // Check logic: width 0.05, length 0.4
                    
                    // Use dot for simplicity first
                    float glow = smoothstep(0.5, 0.0, dist); // Broad glow
                    float core = smoothstep(0.1, 0.0, dist); // Bright core
                    
                    // Flicker
                    float flicker = sin(_Time.y * 5.0 + rnd * 100.0) * 0.5 + 0.5;
                    
                    half3 starColor = _HorizonColor.rgb * 2.0; // Brighter horizon color
                    color += starColor * (core + glow * 0.5) * flicker;
                }

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
