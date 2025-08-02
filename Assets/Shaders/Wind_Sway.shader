Shader "Unlit/TreeSway"
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
        _Brightness ("Brightness", Range(0.1, 3)) = 1
        _ShadowBrightness ("Shadow Brightness", Range(0, 1)) = 0.5
        _ShadowStrength ("Shadow Strength", Range(0, 2)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
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
            float _Brightness;
            float _ShadowBrightness;
            float _ShadowStrength;
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

            // Wind animation function
            float3 ApplyWindAnimation(float4 positionOS)
            {
                float4 worldPos = mul(unity_ObjectToWorld, positionOS);
                float3 windDir = normalize(_WindDirection.xyz);
                
                float time = _Time.y * _WindSpeed;
                float heightFactor = positionOS.y + 0.5;
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
                
                return positionOS.xyz + windOffset;
            }
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                
                // Apply wind animation
                input.positionOS.xyz = ApplyWindAnimation(input.positionOS);
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);
                output.shadowCoord = GetShadowCoord(positionInputs);
                
                return output;
            }
            
            half4 frag (Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(albedo.a - _AlphaThreshold);
                
                float4 shadowCoord = input.shadowCoord;
                Light mainLight = GetMainLight(shadowCoord);
                
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                float shadowAttenuation = mainLight.shadowAttenuation;
                shadowAttenuation = pow(shadowAttenuation, _ShadowStrength);
                shadowAttenuation = lerp(_ShadowBrightness, 1.0, shadowAttenuation);
                float3 radiance = mainLight.color * (NdotL * shadowAttenuation);
                
                float3 ambient = SampleSH(normalWS);
                
                half3 color = albedo.rgb * (radiance + ambient) * _Brightness;
                color = MixFog(color, input.fogCoord);
                
                return half4(color, albedo.a);
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
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            
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
            float _Brightness;
            float _ShadowBrightness;
            float _ShadowStrength;
            CBUFFER_END

            // Fix LErp to white bullshiwt
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoordOS : TEXCOORD0;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            // Shared wind animation function
            float3 ApplyWindAnimationShadow(float4 positionOS)
            {
                float4 worldPos = mul(unity_ObjectToWorld, positionOS);
                float3 windDir = normalize(_WindDirection.xyz);
                
                float time = _Time.y * _WindSpeed;
                float heightFactor = positionOS.y + 0.5;
                float stiffness = lerp(1.0, _TrunkStiffness, 1.0 - heightFactor);
                
                float4 phases = time * float4(_SwayFrequency, _SwayFrequency * 1.3, _SwayFrequency * 0.7, _SwayFrequency * 1.7);
                phases += worldPos.x * 0.2 + worldPos.z * 0.15;
                
                float4 wavesIn = phases + float4(worldPos.x, worldPos.z, worldPos.x + worldPos.z, worldPos.x - worldPos.z) * 0.1;
                float4 waves = abs(frac(wavesIn + 0.5) * 2.0 - 1.0);
                waves = waves * waves * (3.0 - 2.0 * waves);
                float2 wavesSum = waves.xz + waves.yw;
                
                float turbulence = sin(time * _TurbulenceFreq + worldPos.x * 0.5) * cos(time * _TurbulenceFreq * 1.2 + worldPos.z * 0.7);
                turbulence *= _TurbulenceStrength;
                
                float3 windOffset = windDir * (wavesSum.x + turbulence) * _WindStrength * heightFactor * stiffness;
                windOffset.y *= 0.3;
                
                float3 perpWind = float3(-windDir.z, 0, windDir.x);
                windOffset += perpWind * wavesSum.y * _WindStrength * 0.4 * heightFactor * stiffness;
                
                return positionOS.xyz + windOffset;
            }
            
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 animatedPosition = ApplyWindAnimationShadow(input.positionOS);
                
                float3 positionWS = TransformObjectToWorld(animatedPosition);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                float4 positionCS = TransformWorldToHClip(positionWS);
                
                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return positionCS;
            }
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.uv = TRANSFORM_TEX(input.texcoordOS, _MainTex);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _Color.a;
                clip(alpha - _AlphaThreshold);
                return 0;
            }
            
            ENDHLSL
        }
    }
}