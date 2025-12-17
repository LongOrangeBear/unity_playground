Shader "Runner/HolographicObstacle"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 0, 0, 1)
        _OutlineColor("Outline Color", Color) = (1, 1, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 1)) = 0.6
        _GlowIntensity("Glow Intensity", Range(0, 2)) = 0.2
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
                float4 _OutlineColor;
                float _OutlineWidth;
                float _GlowIntensity;
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
                
                // Rim Light as "Inner Outline"
                float NdotV = 1.0 - saturate(dot(normal, viewDir));
                
                // Outline at the edge (High NdotV because NdotV = 1 - dot)
                // Wait, my NdotV calculation above is: 1 - dot.
                // dot(normal, view) is 1 at center, 0 at edge.
                // so NdotV is 0 at center, 1 at edge.
                
                // So High NdotV (near 1) = Edge.
                // smoothstep(0.6, 0.7, NdotV) -> 1 at Edge, 0 at Center.
                // lerp(base, outline, 1) -> Outline at Edge.
                
                // Ah, previous logic:
                // float NdotV = 1.0 - saturate(dot(normal, viewDir));
                // Center: dot=1, NdotV=0.
                // Edge: dot=0, NdotV=1.
                // smoothstep(0.6, 0.7, 0) = 0 -> Base. (Center)
                // smoothstep(0.6, 0.7, 1) = 1 -> Outline. (Edge)
                
                // So existing logic WAS correct for "Outline at Edge" based on "1-dot".
                // Let's re-verify the request "Opaque with Outline".
                
                // Maybe the parameters are off? 
                // Grid log might be unrelated.
                
                // Let's adjust opacity or mixing?
                float outline = smoothstep(_OutlineWidth, _OutlineWidth + 0.1, NdotV);
                
                half3 baseColor = _BaseColor.rgb * (1.0 + _GlowIntensity);
                half3 finalColor = lerp(baseColor, _OutlineColor.rgb, outline);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
