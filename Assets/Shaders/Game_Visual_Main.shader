Shader "Custom/OvercookedStyleCharacter"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Stylized Shading)]
        _ShadowTint ("Shadow Tint", Color) = (0.7, 0.7, 0.9, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0, 1)) = 0.1
        
        [Header(Rim Light)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0, 10)) = 2
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1
        
        [Header(Highlights)]
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
        _SpecularThreshold ("Specular Threshold", Range(0, 1)) = 0.9
        _SpecularSoftness ("Specular Softness", Range(0, 1)) = 0.05
        
        [Header(Color Grading)]
        _Saturation ("Saturation", Range(0, 2)) = 1.2
        _Brightness ("Brightness", Range(0, 2)) = 1.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float4 _ShadowTint;
        float _ShadowThreshold;
        float _ShadowSoftness;
        float4 _RimColor;
        float _RimPower;
        float _RimIntensity;
        float4 _SpecularColor;
        float _Smoothness;
        float _SpecularThreshold;
        float _SpecularSoftness;
        float _Saturation;
        float _Brightness;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            float3 viewDirWS : TEXCOORD3;
        };
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float3 baseColor = baseMap.rgb * _BaseColor.rgb;
                
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                float3 finalColor = float3(0, 0, 0);
                
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                float3 lightDirWS = normalize(mainLight.direction);
                float3 halfDirWS = normalize(lightDirWS + viewDirWS);
                
                float NdotL = dot(normalWS, lightDirWS);
                float NdotV = dot(normalWS, viewDirWS);
                float NdotH = dot(normalWS, halfDirWS);
                
                float lightAttenuation = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                
                float toonNdotL = smoothstep(_ShadowThreshold - _ShadowSoftness, _ShadowThreshold + _ShadowSoftness, NdotL * lightAttenuation);
                float3 diffuse = lerp(_ShadowTint.rgb, float3(1, 1, 1), toonNdotL);
                
                float toonSpecular = smoothstep(_SpecularThreshold - _SpecularSoftness, _SpecularThreshold + _SpecularSoftness, NdotH);
                toonSpecular *= toonNdotL;
                float3 specular = toonSpecular * _SpecularColor.rgb;
                
                float3 mainLightContribution = (diffuse + specular) * mainLight.color * baseColor;
                finalColor += mainLightContribution;
                
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    float3 additionalLightDir = normalize(light.direction);
                    float3 additionalHalfDir = normalize(additionalLightDir + viewDirWS);
                    
                    float additionalNdotL = dot(normalWS, additionalLightDir);
                    float additionalNdotH = dot(normalWS, additionalHalfDir);
                    
                    float additionalAttenuation = light.shadowAttenuation * light.distanceAttenuation;
                    
                    float additionalToonNdotL = smoothstep(_ShadowThreshold - _ShadowSoftness, _ShadowThreshold + _ShadowSoftness, additionalNdotL * additionalAttenuation);
                    float3 additionalDiffuse = lerp(_ShadowTint.rgb, float3(1, 1, 1), additionalToonNdotL);
                    
                    float additionalToonSpecular = smoothstep(_SpecularThreshold - _SpecularSoftness, _SpecularThreshold + _SpecularSoftness, additionalNdotH);
                    additionalToonSpecular *= additionalToonNdotL;
                    float3 additionalSpecular = additionalToonSpecular * _SpecularColor.rgb;
                    
                    float3 additionalLightContribution = (additionalDiffuse + additionalSpecular) * light.color * baseColor;
                    finalColor += additionalLightContribution;
                }
                
                float rimFactor = 1.0 - saturate(NdotV);
                float rim = pow(rimFactor, _RimPower) * _RimIntensity;
                float3 rimLight = rim * _RimColor.rgb;
                finalColor += rimLight;
                
                float luminance = dot(finalColor, float3(0.299, 0.587, 0.114));
                finalColor = lerp(float3(luminance, luminance, luminance), finalColor, _Saturation);
                finalColor *= _Brightness;
                
                return float4(finalColor, baseMap.a * _BaseColor.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct ShadowVaryings
            {
                float4 positionHCS : SV_POSITION;
            };
            
            ShadowVaryings vert(ShadowAttributes input)
            {
                ShadowVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                Light mainLight = GetMainLight();
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                positionWS = ApplyShadowBias(positionWS, normalWS, mainLight.direction);
                
                output.positionHCS = TransformWorldToHClip(positionWS);
                return output;
            }
            
            float4 frag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}