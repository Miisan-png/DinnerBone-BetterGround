Shader "Universal Render Pipeline/TreeSwayLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _WindStrength ("Wind Strength", Range(0, 5)) = 1
        _WindSpeed ("Wind Speed", Range(0, 10)) = 2
        _WindDirection ("Wind Direction", Vector) = (1, 0, 0, 0)
        _TrunkStiffness ("Trunk Stiffness", Range(0, 1)) = 0.3
        _SwayFrequency ("Sway Frequency", Range(0.5, 3)) = 1.5
        _TurbulenceStrength ("Turbulence Strength", Range(0, 2)) = 0.5
        _TurbulenceFreq ("Turbulence Frequency", Range(1, 10)) = 3
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="TransparentCutout" 
            "Queue"="AlphaTest" 
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 200
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float4 positionCS : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _WindStrength;
                float _WindSpeed;
                float4 _WindDirection;
                float _TrunkStiffness;
                float _SwayFrequency;
                float _TurbulenceStrength;
                float _TurbulenceFreq;
                float _AlphaThreshold;
            CBUFFER_END
            
            float4 SmoothCurve(float4 x)
            {
                return x * x * (3.0 - 2.0 * x);
            }
            
            float4 TriangleWave(float4 x)
            {
                return abs(frac(x + 0.5) * 2.0 - 1.0);
            }
            
            float4 SmoothTriangleWave(float4 x)
            {
                return SmoothCurve(TriangleWave(x));
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                float4 worldPos = mul(UNITY_MATRIX_M, input.positionOS);
                float3 windDir = normalize(_WindDirection.xyz);
                
                float time = _TimeParameters.x * _WindSpeed;
                float heightFactor = input.positionOS.y + 0.5;
                float stiffness = lerp(1.0, _TrunkStiffness, 1.0 - heightFactor);
                
                float4 phases = time * float4(_SwayFrequency, _SwayFrequency * 1.3, _SwayFrequency * 0.7, _SwayFrequency * 1.7);
                phases += worldPos.x * 0.2 + worldPos.z * 0.15;
                
                float4 wavesIn = phases + float4(worldPos.x, worldPos.z, worldPos.x + worldPos.z, worldPos.x - worldPos.z) * 0.1;
                float4 waves = SmoothTriangleWave(wavesIn);
                float2 wavesSum = waves.xz + waves.yw;
                
                float turbulence = sin(time * _TurbulenceFreq + worldPos.x * 0.5) * cos(time * _TurbulenceFreq * 1.2 + worldPos.z * 0.7);
                turbulence *= _TurbulenceStrength;
                
                float3 windOffset = windDir * (wavesSum.x + turbulence) * _WindStrength * heightFactor * stiffness;
                windOffset.y *= 0.3;
                
                float3 perpWind = float3(-windDir.z, 0, windDir.x);
                windOffset += perpWind * wavesSum.y * _WindStrength * 0.4 * heightFactor * stiffness;
                
                input.positionOS.xyz += windOffset;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.positionCS = vertexInput.positionCS;
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                output.shadowCoord = GetShadowCoord(vertexInput);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(col.a - _AlphaThreshold);
                
                // Get main light
                Light mainLight = GetMainLight(input.shadowCoord);
                
                // Calculate lighting
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                // Apply lighting
                half3 color = col.rgb * mainLight.color * NdotL * mainLight.shadowAttenuation;
                
                // Add ambient lighting
                color += col.rgb * SampleSH(normalWS) * 0.5;
                
                // Apply fog
                color = MixFog(color, input.fogCoord);
                
                return half4(color, col.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _AlphaThreshold;
                float _WindStrength;
                float _WindSpeed;
                float4 _WindDirection;
                float _TrunkStiffness;
                float _SwayFrequency;
                float _TurbulenceStrength;
                float _TurbulenceFreq;
            CBUFFER_END
            
            float4 SmoothCurve(float4 x)
            {
                return x * x * (3.0 - 2.0 * x);
            }
            
            float4 TriangleWave(float4 x)
            {
                return abs(frac(x + 0.5) * 2.0 - 1.0);
            }
            
            float4 SmoothTriangleWave(float4 x)
            {
                return SmoothCurve(TriangleWave(x));
            }
            
            float3 _LightDirection;
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                // Apply wind animation (same as main pass)
                float4 worldPos = mul(UNITY_MATRIX_M, input.positionOS);
                float3 windDir = normalize(_WindDirection.xyz);
                
                float time = _TimeParameters.x * _WindSpeed;
                float heightFactor = input.positionOS.y + 0.5;
                float stiffness = lerp(1.0, _TrunkStiffness, 1.0 - heightFactor);
                
                float4 phases = time * float4(_SwayFrequency, _SwayFrequency * 1.3, _SwayFrequency * 0.7, _SwayFrequency * 1.7);
                phases += worldPos.x * 0.2 + worldPos.z * 0.15;
                
                float4 wavesIn = phases + float4(worldPos.x, worldPos.z, worldPos.x + worldPos.z, worldPos.x - worldPos.z) * 0.1;
                float4 waves = SmoothTriangleWave(wavesIn);
                float2 wavesSum = waves.xz + waves.yw;
                
                float turbulence = sin(time * _TurbulenceFreq + worldPos.x * 0.5) * cos(time * _TurbulenceFreq * 1.2 + worldPos.z * 0.7);
                turbulence *= _TurbulenceStrength;
                
                float3 windOffset = windDir * (wavesSum.x + turbulence) * _WindStrength * heightFactor * stiffness;
                windOffset.y *= 0.3;
                
                float3 perpWind = float3(-windDir.z, 0, windDir.x);
                windOffset += perpWind * wavesSum.y * _WindStrength * 0.4 * heightFactor * stiffness;
                
                input.positionOS.xyz += windOffset;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionCS = GetShadowPositionHClip(input.positionOS, normalInput.normalWS);
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                clip(col.a - _AlphaThreshold);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}