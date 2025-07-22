// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LazyEti/URP/FakePointLight"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[NoScaleOffset][SingleLineTexture]_GradientTexture("Gradient Texture", 2D) = "white" {}
		[HDR]_LightTint("Light Tint", Color) = (1,1,1,1)
		[Space(5)]_LightSoftness("Light Softness", Range( 0 , 1)) = 1
		[IntRange]_LightPosterize("Light Posterize", Range( 0 , 128)) = 1
		[Space(5)]_ShadingBlend("Shading Blend", Range( 0 , 1)) = 0.5
		_ShadingSoftness("Shading Softness", Range( 0.01 , 1)) = 0.5
		[Toggle(___HALO____ON)] ___Halo___("___Halo___", Float) = 1
		[HDR]_HaloTint("Halo Tint", Color) = (1,1,1,1)
		_HaloSize("Halo Size", Range( 0 , 5)) = 0
		[IntRange]_HaloPosterize("Halo Posterize", Range( 0 , 128)) = 0
		_HaloDepthFade("Halo Depth Fade", Range( 0.1 , 2)) = 0.5
		[Space(25)][Toggle]DistanceFade("___Distance Fade___", Float) = 0
		[Tooltip(Starts fading away at this distance from the camera)]_FarFade("Far Fade", Range( 0 , 400)) = 200
		_FarTransition("Far Transition", Range( 1 , 100)) = 50
		_CloseFade("Close Fade", Range( 0 , 50)) = 0
		_CloseTransition("Close Transition", Range( 0 , 50)) = 0
		[Space(25)][Toggle(___FLICKERING____ON)] ___Flickering___("___Flickering___", Float) = 0
		_FlickerIntensity("Flicker Intensity", Range( 0 , 1)) = 0.5
		_FlickerHue("Flicker Hue", Color) = (1,1,1)
		_FlickerSpeed("Flicker Speed", Range( 0.01 , 5)) = 1
		_FlickerSoftness("Flicker Softness", Range( 0 , 1)) = 0.5
		_SizeFlickering("Size Flickering", Range( 0 , 0.5)) = 0.1
		[Space(25)][Toggle(___NOISE____ON)] ___Noise___("___Noise___", Float) = 0
		[NoScaleOffset][SingleLineTexture]_NoiseTexture("Noise Texture", 2D) = "white" {}
		[KeywordEnum(Red,RedxGreen,Alpha)] _TexturePacking("Texture Packing", Float) = 0
		_Noisiness("Noisiness", Range( 0 , 2)) = 1
		_NoiseScale("Noise Scale", Range( 0.1 , 5)) = 0.1
		_NoiseMovement("Noise Movement", Range( 0 , 1)) = 0
		[Space(20)][Toggle(_SPECULARHIGHLIGHT_ON)] _SpecularHighlight("Specular Highlight", Float) = 0
		_SpecIntensity("Spec Intensity", Range( 0 , 1)) = 0.5
		[Space(20)][Toggle(_DITHERINGPATTERN_ON)] _DitheringPattern("Dithering Pattern", Float) = 0
		_DitherIntensity("Dither Intensity", Range( 0.01 , 1)) = 0.5
		[Space(20)][KeywordEnum(OFF,Low,Medium,High,Insane)] _ScreenShadows("Screen Shadows (HEAVY)", Float) = 0
		[space(25)]_ShadowThreshold("Shadow Threshold", Range( 0.05 , 1)) = 0.5
		[Toggle(_PARTICLEMODE_ON)] _ParticleMode("Particle Mode", Float) = 0
		[Space(15)][Toggle(_ACCURATECOLORS_ON)] _AccurateColors("Accurate Colors", Float) = 0
		[Space(15)][Toggle(_DAYFADING_ON)] _DayFading("Day Fading", Float) = 0
		[Space(15)][KeywordEnum(Additive,Contrast,Negative)] _Blendmode("Blendmode", Float) = 0
		[Enum(Default,0,Off,1,On,2)][Space(5)]_DepthWrite("Depth Write", Float) = 0
		[HideInInspector][IntRange][Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Range( 0 , 12)) = 1
		[HideInInspector][IntRange][Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Range( 0 , 12)) = 1
		[HideInInspector]_RandomOffset("RandomOffset", Range( 0 , 1)) = 0


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Overlay" "Queue"="Overlay" "UniversalMaterialType"="Unlit" }

		Cull Front
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 3.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_DepthWrite]
			ZTest Always
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM
            #pragma multi_compile_instancing
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ASE_VERSION 19701
            #define ASE_SRP_VERSION 100400
            #define REQUIRE_DEPTH_TEXTURE 1
            #define REQUIRE_OPAQUE_TEXTURE 1
            #define ASE_USING_SAMPLING_MACROS 1

            #pragma multi_compile _ DOTS_INSTANCING_ON

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _BLENDMODE_ADDITIVE _BLENDMODE_CONTRAST _BLENDMODE_NEGATIVE
			#pragma shader_feature_local _DITHERINGPATTERN_ON
			#pragma shader_feature_local _ACCURATECOLORS_ON
			#pragma shader_feature_local _PARTICLEMODE_ON
			#pragma shader_feature_local ___FLICKERING____ON
			#pragma shader_feature_local ___NOISE____ON
			#pragma shader_feature_local _TEXTUREPACKING_RED _TEXTUREPACKING_REDXGREEN _TEXTUREPACKING_ALPHA
			#pragma shader_feature_local _SCREENSHADOWS_OFF _SCREENSHADOWS_LOW _SCREENSHADOWS_MEDIUM _SCREENSHADOWS_HIGH _SCREENSHADOWS_INSANE
			#pragma shader_feature_local _DAYFADING_ON
			#pragma shader_feature_local _SPECULARHIGHLIGHT_ON
			#pragma shader_feature_local ___HALO____ON
			#ifdef STEREO_INSTANCING_ON
			TEXTURE2D_ARRAY(_CameraNormalsTexture); SAMPLER(sampler_CameraNormalsTexture);
			#else
			TEXTURE2D(_CameraNormalsTexture); SAMPLER(sampler_CameraNormalsTexture);
			#endif


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HaloTint;
			float4 _LightTint;
			float3 _FlickerHue;
			float _SrcBlend;
			float _HaloDepthFade;
			float _HaloPosterize;
			float _HaloSize;
			float _SpecIntensity;
			float _ShadingBlend;
			float _CloseTransition;
			float _CloseFade;
			float _FarTransition;
			float _FarFade;
			float DistanceFade;
			float _ShadowThreshold;
			float _ShadingSoftness;
			float _Noisiness;
			float _NoiseMovement;
			float _NoiseScale;
			float _SizeFlickering;
			float _FlickerIntensity;
			float _FlickerSoftness;
			float _RandomOffset;
			float _FlickerSpeed;
			float _LightSoftness;
			float _DepthWrite;
			float _DstBlend;
			float _LightPosterize;
			float _DitherIntensity;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			TEXTURE2D(_GradientTexture);
			TEXTURE2D(_NoiseTexture);
			SAMPLER(sampler_NoiseTexture);
			SAMPLER(sampler_GradientTexture);


			float4x4 Inverse4x4(float4x4 input)
			{
				#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
				float4x4 cofactors = float4x4(
				minor( _22_23_24, _32_33_34, _42_43_44 ),
				-minor( _21_23_24, _31_33_34, _41_43_44 ),
				minor( _21_22_24, _31_32_34, _41_42_44 ),
				-minor( _21_22_23, _31_32_33, _41_42_43 ),
			
				-minor( _12_13_14, _32_33_34, _42_43_44 ),
				minor( _11_13_14, _31_33_34, _41_43_44 ),
				-minor( _11_12_14, _31_32_34, _41_42_44 ),
				minor( _11_12_13, _31_32_33, _41_42_43 ),
			
				minor( _12_13_14, _22_23_24, _42_43_44 ),
				-minor( _11_13_14, _21_23_24, _41_43_44 ),
				minor( _11_12_14, _21_22_24, _41_42_44 ),
				-minor( _11_12_13, _21_22_23, _41_42_43 ),
			
				-minor( _12_13_14, _22_23_24, _32_33_34 ),
				minor( _11_13_14, _21_23_24, _31_33_34 ),
				-minor( _11_12_14, _21_22_24, _31_32_34 ),
				minor( _11_12_13, _21_22_23, _31_32_33 ));
				#undef minor
				return transpose( cofactors ) / determinant( input );
			}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g2082( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float noise58_g1436( float x )
			{
				float n = sin (2 * x) + sin(3.14159265 * x);
				return n;
			}
			
			float4 NormalTexURP2275( float2 uvs )
			{
				#ifdef STEREO_INSTANCING_ON
				return SAMPLE_TEXTURE2D_ARRAY(_CameraNormalsTexture,sampler_CameraNormalsTexture,uvs,unity_StereoEyeIndex);
				#else
				return SAMPLE_TEXTURE2D(_CameraNormalsTexture,sampler_CameraNormalsTexture,uvs);
				#endif
			}
			
			inline float Dither8x8Bayer( int x, int y )
			{
				const float dither[ 64 ] = {
			 1, 49, 13, 61,  4, 52, 16, 64,
			33, 17, 45, 29, 36, 20, 48, 32,
			 9, 57,  5, 53, 12, 60,  8, 56,
			41, 25, 37, 21, 44, 28, 40, 24,
			 3, 51, 15, 63,  2, 50, 14, 62,
			35, 19, 47, 31, 34, 18, 46, 30,
			11, 59,  7, 55, 10, 58,  6, 54,
			43, 27, 39, 23, 42, 26, 38, 22};
				int r = y * 8 + x;
				return dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
			}
			
			float ExperimentalScreenShadowsURP( float2 lightDirScreen, float threshold, float stepsSpace, float stepsNum, float radius, float mask, float3 wPos, float3 lightPos, float3 camPos, float2 screenPos, float3 offsetDir )
			{
				if(mask<=0) return 1;
				float depth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenPos), _ZBufferParams);
											
				//offset light position by its max radius:
				float3 lightRadiusOffsetPos = lightPos+ (offsetDir * radius);
				//convert position to real depth distance:
				float MaxDist = -mul( UNITY_MATRIX_V, float4(lightRadiusOffsetPos, 1)).z;
				//Early Return if greater than max radius:
				if(depth > MaxDist) return 1;
				//initialization:
				float shadow =0;
				float op = 2/stepsNum;
				float spacing =((stepsSpace/stepsNum)) *(clamp(distance (lightPos,camPos),radius,1));
				//float spacing =((stepsSpace/stepsNum)) ;
				float t = spacing;
				float realLightDist = -mul( UNITY_MATRIX_V, float4(lightPos, 1)).z;
				[unroll]  for (int i = 1;i <= stepsNum ;i++)
				{                    
					float2 uvs = screenPos + lightDirScreen.xy * t; //offset uv
					t = clamp( spacing * i,-1,1); //ray march
					float d = SHADERGRAPH_SAMPLE_SCENE_DEPTH(uvs); //sample depth
					float l = LinearEyeDepth(d, _ZBufferParams);
					if(MaxDist < l) return 1;
					float3 world = ComputeWorldSpacePosition(uvs, d, UNITY_MATRIX_I_VP);
					if(distance(wPos,lightPos) > radius) return 1; //remove out of range artifacts
					if(shadow>=1) break; 
					if(world.y - wPos.y> threshold * MaxDist && abs(world.y-wPos.y) < radius)  shadow  += op;
				}
				 //return smoothstep(.9,1,shadow);
				shadow = step (0.01, shadow)*mask;
				return (1- shadow);
			}
			
			float2 WorldToScreen83_g2060( float3 pos )
			{
				float4 wts = ComputeScreenPos( TransformWorldToHClip(pos));
				float3 wts_NDC = wts.xyz / wts.w;
				return wts_NDC.xy;
			}
			
			inline float Dither4x4Bayer( int x, int y )
			{
				const float dither[ 16 ] = {
			 1,  9,  3, 11,
			13,  5, 15,  7,
			 4, 12,  2, 10,
			16,  8, 14,  6 };
				int r = y * 4 + x;
				return dither[r] / 16; // same # of instructions as pre-dividing due to compiler magic
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_objectPosition = GetAbsolutePositionWS( UNITY_MATRIX_M._m03_m13_m23 );
				#ifdef _PARTICLEMODE_ON
				float3 staticSwitch255 = v.ase_texcoord.xyz;
				#else
				float3 staticSwitch255 = ase_objectPosition;
				#endif
				float3 POSITION653 = staticSwitch255;
				float3 _Vector0 = float3(1,0,1);
				float Dist41_g1803 = distance( ( POSITION653 * _Vector0 ) , ( _Vector0 * _WorldSpaceCameraPos ) );
				float vertexToFrag49_g1803 = ( saturate( ( 1.0 - ( ( Dist41_g1803 - _FarFade ) / _FarTransition ) ) ) * saturate( ( ( Dist41_g1803 - _CloseFade ) / _CloseTransition ) ) );
				o.ase_texcoord5.w = vertexToFrag49_g1803;
				float dotResult3_g1441 = dot( -_MainLightPosition.xyz , float3( 0,1,0 ) );
				float vertexToFrag9_g1441 = saturate( ( dotResult3_g1441 * 4.0 ) );
				o.ase_texcoord6.x = vertexToFrag9_g1441;
				#ifdef _PARTICLEMODE_ON
				float staticSwitch1913 = v.ase_texcoord.w;
				#else
				float staticSwitch1913 = _RandomOffset;
				#endif
				float RANDOMNESS3727 = staticSwitch1913;
				float temp_output_29_0_g1436 = RANDOMNESS3727;
				float mulTime17_g1436 = _TimeParameters.x * ( ( _FlickerSpeed + ( temp_output_29_0_g1436 * 0.1 ) ) * 4 );
				float x58_g1436 = ( mulTime17_g1436 + ( temp_output_29_0_g1436 * PI ) );
				float localnoise58_g1436 = noise58_g1436( x58_g1436 );
				float temp_output_44_0_g1436 = ( ( 1.0 - _FlickerSoftness ) * 0.5 );
				#ifdef ___FLICKERING____ON
				float staticSwitch53_g1436 = saturate( (( 1.0 - _FlickerIntensity ) + ((0.0 + (localnoise58_g1436 - -2.0) * (1.0 - 0.0) / (2.0 - -2.0)) - ( 1.0 - temp_output_44_0_g1436 )) * (1.0 - ( 1.0 - _FlickerIntensity )) / (temp_output_44_0_g1436 - ( 1.0 - temp_output_44_0_g1436 ))) );
				#else
				float staticSwitch53_g1436 = 1.0;
				#endif
				float FlickerAlpha416 = staticSwitch53_g1436;
				float FlickerSize477 = (( 1.0 - _SizeFlickering ) + (FlickerAlpha416 - 0.0) * (1.0 - ( 1.0 - _SizeFlickering )) / (1.0 - 0.0));
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				#ifdef _PARTICLEMODE_ON
				float3 staticSwitch3833 = ( ase_objectScale * v.ase_texcoord1.xyz );
				#else
				float3 staticSwitch3833 = ase_objectScale;
				#endif
				float3 break3887 = staticSwitch3833;
				float SCALE3835 = max( max( break3887.x , break3887.y ) , break3887.z );
				#ifdef _PARTICLEMODE_ON
				float staticSwitch4535 = ( SCALE3835 * 0.1 );
				#else
				float staticSwitch4535 = 1.0;
				#endif
				float vertexToFrag32_g2060 = ( ( _HaloSize * ( FlickerSize477 * staticSwitch4535 ) ) * 0.5 );
				o.ase_texcoord6.y = vertexToFrag32_g2060;
				float3 pos75_g2060 = POSITION653;
				float vertexToFrag15_g2063 = ( unity_OrthoParams.w == 0.0 ? ( distance( _WorldSpaceCameraPos , pos75_g2060 ) / -UNITY_MATRIX_P[ 1 ][ 1 ] ) : unity_OrthoParams.y );
				o.ase_texcoord6.z = vertexToFrag15_g2063;
				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				float3 ase_viewDirSafeWS = SafeNormalize( ase_viewVectorWS );
				float dotResult31_g2060 = dot( ase_viewDirSafeWS , ( _WorldSpaceCameraPos - pos75_g2060 ) );
				float vertexToFrag36_g2060 = step( 0.0 , dotResult31_g2060 );
				o.ase_texcoord6.w = vertexToFrag36_g2060;
				float vertexToFrag62_g2060 = distance( _WorldSpaceCameraPos , pos75_g2060 );
				o.ase_texcoord7.x = vertexToFrag62_g2060;
				float3 temp_cast_0 = (1.0).xxx;
				float3 lerpResult51_g1436 = lerp( _FlickerHue , temp_cast_0 , ( staticSwitch53_g1436 * staticSwitch53_g1436 ));
				float3 vertexToFrag67_g1436 = lerpResult51_g1436;
				o.ase_texcoord7.yzw = vertexToFrag67_g1436;
				
				o.ase_texcoord4 = v.ase_texcoord;
				o.ase_texcoord5.xyz = v.ase_texcoord1.xyz;
				o.ase_color = v.ase_color;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float temp_output_3711_0 = ( ( 1.0 - ( _LightSoftness * 1.1 ) ) * 0.5 );
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth6_g2080 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float4x4 invertVal5_g2081 = Inverse4x4( UNITY_MATRIX_M );
				float dotResult4_g2080 = dot( ase_viewDirWS , -mul( UNITY_MATRIX_M, float4( (transpose( mul( invertVal5_g2081, UNITY_MATRIX_I_V ) )[2]).xyz , 0.0 ) ).xyz );
				float2 UV22_g2083 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g2083 = UnStereo( UV22_g2083 );
				float2 break64_g2082 = localUnStereo22_g2083;
				float clampDepth69_g2082 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g2082 = ( 1.0 - clampDepth69_g2082 );
				#else
				float staticSwitch38_g2082 = clampDepth69_g2082;
				#endif
				float3 appendResult39_g2082 = (float3(break64_g2082.x , break64_g2082.y , staticSwitch38_g2082));
				float4 appendResult42_g2082 = (float4((appendResult39_g2082*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g2082 = mul( unity_CameraInvProjection, appendResult42_g2082 );
				float3 temp_output_46_0_g2082 = ( (temp_output_43_0_g2082).xyz / (temp_output_43_0_g2082).w );
				float3 In75_g2082 = temp_output_46_0_g2082;
				float3 localInvertDepthDirURP75_g2082 = InvertDepthDirURP75_g2082( In75_g2082 );
				float4 appendResult49_g2082 = (float4(localInvertDepthDirURP75_g2082 , 1.0));
				float4 ReconstructedPos539 = ( unity_OrthoParams.w == 0.0 ? float4( ( ( eyeDepth6_g2080 * ( ase_viewDirWS / dotResult4_g2080 ) ) + _WorldSpaceCameraPos ) , 0.0 ) : mul( unity_CameraToWorld, appendResult49_g2082 ) );
				float3 ase_objectPosition = GetAbsolutePositionWS( UNITY_MATRIX_M._m03_m13_m23 );
				#ifdef _PARTICLEMODE_ON
				float3 staticSwitch255 = IN.ase_texcoord4.xyz;
				#else
				float3 staticSwitch255 = ase_objectPosition;
				#endif
				float3 POSITION653 = staticSwitch255;
				float4 LocalPos4107 = ( ReconstructedPos539 - float4( POSITION653 , 0.0 ) );
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				#ifdef _PARTICLEMODE_ON
				float3 staticSwitch3833 = ( ase_objectScale * IN.ase_texcoord5.xyz );
				#else
				float3 staticSwitch3833 = ase_objectScale;
				#endif
				float3 break3887 = staticSwitch3833;
				float SCALE3835 = max( max( break3887.x , break3887.y ) , break3887.z );
				#ifdef _PARTICLEMODE_ON
				float staticSwitch1913 = IN.ase_texcoord4.w;
				#else
				float staticSwitch1913 = _RandomOffset;
				#endif
				float RANDOMNESS3727 = staticSwitch1913;
				float temp_output_29_0_g1436 = RANDOMNESS3727;
				float mulTime17_g1436 = _TimeParameters.x * ( ( _FlickerSpeed + ( temp_output_29_0_g1436 * 0.1 ) ) * 4 );
				float x58_g1436 = ( mulTime17_g1436 + ( temp_output_29_0_g1436 * PI ) );
				float localnoise58_g1436 = noise58_g1436( x58_g1436 );
				float temp_output_44_0_g1436 = ( ( 1.0 - _FlickerSoftness ) * 0.5 );
				#ifdef ___FLICKERING____ON
				float staticSwitch53_g1436 = saturate( (( 1.0 - _FlickerIntensity ) + ((0.0 + (localnoise58_g1436 - -2.0) * (1.0 - 0.0) / (2.0 - -2.0)) - ( 1.0 - temp_output_44_0_g1436 )) * (1.0 - ( 1.0 - _FlickerIntensity )) / (temp_output_44_0_g1436 - ( 1.0 - temp_output_44_0_g1436 ))) );
				#else
				float staticSwitch53_g1436 = 1.0;
				#endif
				float FlickerAlpha416 = staticSwitch53_g1436;
				float FlickerSize477 = (( 1.0 - _SizeFlickering ) + (FlickerAlpha416 - 0.0) * (1.0 - ( 1.0 - _SizeFlickering )) / (1.0 - 0.0));
				float temp_output_3981_0 = saturate( ( 1.0 - ( length( LocalPos4107 ) / ( SCALE3835 * ( FlickerSize477 * 0.45 ) ) ) ) );
				float3 UVS73_g1804 = ( ( ReconstructedPos539.xyz * 0.1 ) * _NoiseScale );
				float2 ScreenPos4516 = (ase_screenPosNorm).xy;
				float2 uvs2275 = ScreenPos4516;
				float4 localNormalTexURP2275 = NormalTexURP2275( uvs2275 );
				float4 worldNormals1927 = localNormalTexURP2275;
				float3 temp_cast_7 = (3.0).xxx;
				float3 temp_output_141_0_g1804 = pow( abs( worldNormals1927.xyz ) , temp_cast_7 );
				float3 temp_cast_8 = (1.0).xxx;
				float dotResult144_g1804 = dot( temp_output_141_0_g1804 , temp_cast_8 );
				float3 break147_g1804 = ( saturate( temp_output_141_0_g1804 ) / dotResult144_g1804 );
				float2 clipScreen151_g1804 = abs( ase_screenPosNorm.xy ) * _ScreenParams.xy;
				float dither151_g1804 = Dither8x8Bayer( fmod(clipScreen151_g1804.x, 8), fmod(clipScreen151_g1804.y, 8) );
				float2 lerpResult19_g1804 = lerp( (UVS73_g1804).xz , ( (UVS73_g1804).yz * 0.9 ) , round( ( ( ( ( 1.0 - break147_g1804.x ) * break147_g1804.x ) * dither151_g1804 ) + break147_g1804.x ) ));
				float2 lerpResult20_g1804 = lerp( lerpResult19_g1804 , ( (UVS73_g1804).xy * 0.94 ) , round( ( break147_g1804.z + ( dither151_g1804 * ( break147_g1804.z * ( 1.0 - break147_g1804.z ) ) ) ) ));
				float temp_output_60_0_g1804 = RANDOMNESS3727;
				float mulTime41_g1804 = _TimeParameters.x * ( ( _NoiseMovement + ( temp_output_60_0_g1804 * 0.1 ) ) * 0.2 );
				float Time70_g1804 = ( mulTime41_g1804 + ( temp_output_60_0_g1804 * PI ) );
				float4 tex2DNode3_g1804 = SAMPLE_TEXTURE2D( _NoiseTexture, sampler_NoiseTexture, ( lerpResult20_g1804 + ( Time70_g1804 * float2( 1.02,0.87 ) ) ) );
				#if defined( _TEXTUREPACKING_RED )
				float staticSwitch211_g1804 = tex2DNode3_g1804.r;
				#elif defined( _TEXTUREPACKING_REDXGREEN )
				float staticSwitch211_g1804 = tex2DNode3_g1804.g;
				#elif defined( _TEXTUREPACKING_ALPHA )
				float staticSwitch211_g1804 = tex2DNode3_g1804.a;
				#else
				float staticSwitch211_g1804 = tex2DNode3_g1804.r;
				#endif
				float4 tex2DNode14_g1804 = SAMPLE_TEXTURE2D( _NoiseTexture, sampler_NoiseTexture, ( ( lerpResult20_g1804 * 0.7 ) + ( Time70_g1804 * float2( -0.72,-0.67 ) ) ) );
				#if defined( _TEXTUREPACKING_RED )
				float staticSwitch212_g1804 = tex2DNode14_g1804.r;
				#elif defined( _TEXTUREPACKING_REDXGREEN )
				float staticSwitch212_g1804 = tex2DNode14_g1804.r;
				#elif defined( _TEXTUREPACKING_ALPHA )
				float staticSwitch212_g1804 = tex2DNode14_g1804.a;
				#else
				float staticSwitch212_g1804 = tex2DNode14_g1804.r;
				#endif
				#ifdef ___NOISE____ON
				float staticSwitch186_g1804 = ( 1.0 + ( ( staticSwitch211_g1804 * staticSwitch212_g1804 * _Noisiness ) - ( _Noisiness * 0.2 ) ) );
				#else
				float staticSwitch186_g1804 = 1.0;
				#endif
				float noise3969 = staticSwitch186_g1804;
				float smoothstepResult745 = smoothstep( temp_output_3711_0 , ( 1.0 - temp_output_3711_0 ) , ( temp_output_3981_0 * ( temp_output_3981_0 + noise3969 ) ));
				float lPosterize4215 = _LightPosterize;
				float temp_output_8_0_g1808 = lPosterize4215;
				float BrightnessBoost3902 = saturate( pow( temp_output_3981_0 , 30.0 ) );
				float temp_output_9_0_g1808 = ( smoothstepResult745 + BrightnessBoost3902 );
				float temp_output_5_0_g1808 = ( 256.0 / temp_output_8_0_g1808 );
				float GradientMask555 = ( smoothstepResult745 * ( temp_output_8_0_g1808 <= 0.0 ? temp_output_9_0_g1808 : saturate( ( floor( ( temp_output_9_0_g1808 * temp_output_5_0_g1808 ) ) / temp_output_5_0_g1808 ) ) ) );
				float2 temp_cast_9 = (( 1.0 - GradientMask555 )).xx;
				float4 temp_output_2_0_g1809 = ( SAMPLE_TEXTURE2D( _GradientTexture, sampler_GradientTexture, temp_cast_9 ) * _LightTint * IN.ase_color );
				float temp_output_8_0_g1807 = lPosterize4215;
				float4 normalizeResult3808 = normalize( -LocalPos4107 );
				float4 LightDir4264 = normalizeResult3808;
				float dotResult436 = dot( LightDir4264 , worldNormals1927 );
				float3 pos338_g1805 = POSITION653;
				float rad280_g1805 = ( SCALE3835 * 0.5 );
				float temp_output_298_0_g1805 = ( 1.0 - ( abs( (pos338_g1805).y ) / rad280_g1805 ) );
				float3 normalizeResult278_g1805 = normalize( ( pos338_g1805 - _WorldSpaceCameraPos ) );
				float4x4 invertVal5_g1806 = Inverse4x4( UNITY_MATRIX_M );
				float dotResult283_g1805 = dot( normalizeResult278_g1805 , -mul( UNITY_MATRIX_M, float4( (transpose( mul( invertVal5_g1806, UNITY_MATRIX_I_V ) )[2]).xyz , 0.0 ) ).xyz );
				float temp_output_310_0_g1805 = ( temp_output_298_0_g1805 * dotResult283_g1805 );
				float4 worldToScreen279_g1805 = ComputeScreenPos( TransformWorldToHClip(( pos338_g1805 * float3(1,0.1,1) )) );
				float3 worldToScreen279_g1805NDC = worldToScreen279_g1805/worldToScreen279_g1805.w;
				float2 temp_output_344_0_g1805 = (( float4( worldToScreen279_g1805NDC , 0.0 ) - ase_screenPosNorm )).xy;
				float2 temp_output_384_0_g1805 = ( temp_output_310_0_g1805 * temp_output_344_0_g1805 );
				float2 lightDirScreen483_g1805 = temp_output_384_0_g1805;
				float temp_output_476_0_g1805 = ( _ShadowThreshold * 0.01 );
				float threshold483_g1805 = temp_output_476_0_g1805;
				float stepsSpace483_g1805 = 1.0;
				#if defined( _SCREENSHADOWS_OFF )
				float staticSwitch296_g1805 = -1.0;
				#elif defined( _SCREENSHADOWS_LOW )
				float staticSwitch296_g1805 = 16.0;
				#elif defined( _SCREENSHADOWS_MEDIUM )
				float staticSwitch296_g1805 = 32.0;
				#elif defined( _SCREENSHADOWS_HIGH )
				float staticSwitch296_g1805 = 64.0;
				#elif defined( _SCREENSHADOWS_INSANE )
				float staticSwitch296_g1805 = 128.0;
				#else
				float staticSwitch296_g1805 = -1.0;
				#endif
				float StepQual300_g1805 = staticSwitch296_g1805;
				float stepsNum483_g1805 = StepQual300_g1805;
				float radius483_g1805 = rad280_g1805;
				float vertexToFrag49_g1803 = IN.ase_texcoord5.w;
				float vertexToFrag9_g1441 = IN.ase_texcoord6.x;
				#ifdef _DAYFADING_ON
				float staticSwitch11_g1441 = vertexToFrag9_g1441;
				#else
				float staticSwitch11_g1441 = 1.0;
				#endif
				float DistanceFade3571 = ( (( DistanceFade )?( vertexToFrag49_g1803 ):( 1.0 )) * staticSwitch11_g1441 );
				float temp_output_335_0_g1805 = DistanceFade3571;
				float mask483_g1805 = temp_output_335_0_g1805;
				float3 worldPos353_g1805 = ReconstructedPos539.xyz;
				float3 wPos483_g1805 = worldPos353_g1805;
				float3 lightPos483_g1805 = pos338_g1805;
				float3 camPos483_g1805 = _WorldSpaceCameraPos;
				float2 screenPos483_g1805 = ase_screenPosNorm.xy;
				float3 offsetDir485_g1805 = ( dotResult283_g1805 * normalizeResult278_g1805 );
				float3 offsetDir483_g1805 = offsetDir485_g1805;
				float localExperimentalScreenShadowsURP483_g1805 = ExperimentalScreenShadowsURP( lightDirScreen483_g1805 , threshold483_g1805 , stepsSpace483_g1805 , stepsNum483_g1805 , radius483_g1805 , mask483_g1805 , wPos483_g1805 , lightPos483_g1805 , camPos483_g1805 , screenPos483_g1805 , offsetDir483_g1805 );
				#if defined( _SCREENSHADOWS_OFF )
				float staticSwitch341_g1805 = 1.0;
				#elif defined( _SCREENSHADOWS_LOW )
				float staticSwitch341_g1805 = localExperimentalScreenShadowsURP483_g1805;
				#elif defined( _SCREENSHADOWS_MEDIUM )
				float staticSwitch341_g1805 = localExperimentalScreenShadowsURP483_g1805;
				#elif defined( _SCREENSHADOWS_HIGH )
				float staticSwitch341_g1805 = localExperimentalScreenShadowsURP483_g1805;
				#elif defined( _SCREENSHADOWS_INSANE )
				float staticSwitch341_g1805 = localExperimentalScreenShadowsURP483_g1805;
				#else
				float staticSwitch341_g1805 = 1.0;
				#endif
				float ScreenSpaceShadows1881 = staticSwitch341_g1805;
				float smoothstepResult4598 = smoothstep( 0.0 , _ShadingSoftness , saturate( ( dotResult436 * noise3969 * ScreenSpaceShadows1881 ) ));
				float temp_output_9_0_g1807 = smoothstepResult4598;
				float temp_output_5_0_g1807 = ( 256.0 / temp_output_8_0_g1807 );
				float ShadingMask552 = saturate( ( ( temp_output_8_0_g1807 <= 0.0 ? temp_output_9_0_g1807 : saturate( ( floor( ( temp_output_9_0_g1807 * temp_output_5_0_g1807 ) ) / temp_output_5_0_g1807 ) ) ) + _ShadingBlend ) );
				float surfaceMask487 = step( 0.01 , temp_output_3981_0 );
				float FinalLightMask4298 = ( GradientMask555 * ShadingMask552 * surfaceMask487 );
				float4 normalizeResult4267 = normalize( ( float4( ase_viewDirWS , 0.0 ) + LightDir4264 ) );
				float dotResult4231 = dot( normalizeResult4267 , ( worldNormals1927 * float4( float3(1,0.99,1) , 0.0 ) ) );
				#ifdef _SPECULARHIGHLIGHT_ON
				float staticSwitch4285 = ( ( _SpecIntensity * 2 ) * pow( saturate( dotResult4231 ) , ( (0.5*0.5 + 0.5) * 200 ) ) * FinalLightMask4298 );
				#else
				float staticSwitch4285 = 0.0;
				#endif
				float Spec4287 = staticSwitch4285;
				float3 temp_output_12_0_g1814 = ( ( (temp_output_2_0_g1809).rgb * ( (temp_output_2_0_g1809).a * FinalLightMask4298 * 0.1 ) ) + Spec4287 );
				float4 fetchOpaqueVal5_g1814 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( ScreenPos4516 ), 1.0 );
				float3 saferPower9_g1814 = abs( (fetchOpaqueVal5_g1814).rgb );
				float3 temp_cast_17 = (saturate( (0.8 + (GradientMask555 - 0.0) * (0.0 - 0.8) / (2.0 - 0.0)) )).xxx;
				#ifdef _ACCURATECOLORS_ON
				float3 staticSwitch11_g1814 = ( ( temp_output_12_0_g1814 * 4 ) * pow( saferPower9_g1814 , temp_cast_17 ) );
				#else
				float3 staticSwitch11_g1814 = temp_output_12_0_g1814;
				#endif
				float3 temp_cast_18 = (0.0).xxx;
				float vertexToFrag32_g2060 = IN.ase_texcoord6.y;
				float3 pos75_g2060 = POSITION653;
				float3 pos83_g2060 = pos75_g2060;
				float2 localWorldToScreen83_g2060 = WorldToScreen83_g2060( pos83_g2060 );
				float2 appendResult15_g2060 = (float2(( _ScreenParams.x / _ScreenParams.y ) , 1.0));
				float vertexToFrag15_g2063 = IN.ase_texcoord6.z;
				float smoothstepResult33_g2060 = smoothstep( 0.0 , vertexToFrag32_g2060 , length( ( ( ( (localWorldToScreen83_g2060).xy - ScreenPos4516 ) * appendResult15_g2060 ) * vertexToFrag15_g2063 ) ));
				float vertexToFrag36_g2060 = IN.ase_texcoord6.w;
				float HaloMask38_g2060 = ( ( 1.0 - smoothstepResult33_g2060 ) * vertexToFrag36_g2060 );
				float temp_output_8_0_g2061 = _HaloPosterize;
				float temp_output_9_0_g2061 = HaloMask38_g2060;
				float temp_output_5_0_g2061 = ( 256.0 / temp_output_8_0_g2061 );
				float HaloPosterized43_g2060 = ( HaloMask38_g2060 * ( temp_output_8_0_g2061 <= 0.0 ? temp_output_9_0_g2061 : saturate( ( floor( ( temp_output_9_0_g2061 * temp_output_5_0_g2061 ) ) / temp_output_5_0_g2061 ) ) ) );
				float2 temp_cast_19 = (( 1.0 - HaloPosterized43_g2060 )).xx;
				float4 temp_output_2_0_g2062 = ( SAMPLE_TEXTURE2D( _GradientTexture, sampler_GradientTexture, temp_cast_19 ) * _HaloTint * IN.ase_color );
				float vertexToFrag62_g2060 = IN.ase_texcoord7.x;
				float HaloPenetrationMask68_g2060 = saturate( pow( saturate( ( distance( ReconstructedPos539.xyz , _WorldSpaceCameraPos ) - vertexToFrag62_g2060 ) ) , _HaloDepthFade ) );
				#ifdef ___HALO____ON
				float3 staticSwitch54_g2060 = ( (temp_output_2_0_g2062).rgb * ( (temp_output_2_0_g2062).a * HaloMask38_g2060 * HaloPenetrationMask68_g2060 * HaloPosterized43_g2060 ) );
				#else
				float3 staticSwitch54_g2060 = temp_cast_18;
				#endif
				float3 halo4548 = staticSwitch54_g2060;
				float3 vertexToFrag67_g1436 = IN.ase_texcoord7.yzw;
				float3 FlickerHue1892 = vertexToFrag67_g1436;
				float3 temp_output_17_0_g1815 = ( ( ( staticSwitch11_g1814 + halo4548 ) * FlickerHue1892 ) * ( DistanceFade3571 * FlickerAlpha416 ) );
				float2 clipScreen10_g1815 = abs( ase_screenPosNorm.xy ) * _ScreenParams.xy;
				float dither10_g1815 = Dither4x4Bayer( fmod(clipScreen10_g1815.x, 4), fmod(clipScreen10_g1815.y, 4) );
				float3 break3_g1815 = temp_output_17_0_g1815;
				float smoothstepResult9_g1815 = smoothstep( 0.0 , _DitherIntensity , ( 0.333 * ( break3_g1815.x + break3_g1815.y + break3_g1815.z ) * ( _DitherIntensity + 1.0 ) ));
				dither10_g1815 = step( dither10_g1815, saturate( smoothstepResult9_g1815 * 1.00001 ) );
				#ifdef _DITHERINGPATTERN_ON
				float3 staticSwitch12_g1815 = ( temp_output_17_0_g1815 * dither10_g1815 );
				#else
				float3 staticSwitch12_g1815 = temp_output_17_0_g1815;
				#endif
				float4 appendResult3578 = (float4(staticSwitch12_g1815 , 1.0));
				#if defined( _BLENDMODE_ADDITIVE )
				float4 staticSwitch4585 = appendResult3578;
				#elif defined( _BLENDMODE_CONTRAST )
				float4 staticSwitch4585 = appendResult3578;
				#elif defined( _BLENDMODE_NEGATIVE )
				float4 staticSwitch4585 = ( 1.0 - saturate( appendResult3578 ) );
				#else
				float4 staticSwitch4585 = appendResult3578;
				#endif
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = staticSwitch4585.xyz;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.positionCS.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

	
	}
	
	CustomEditor "FPL.CustomMaterialEditor"
	Fallback Off
	
}
/*ASEBEGIN
Version=19701
Node;AmplifyShaderEditor.CommentaryNode;3728;-3339.878,-1045.967;Inherit;False;734.679;386.8545;;5;1914;742;3832;3727;1913;Random;0.6886792,0,0.67818,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;476;-4574.993,-894.2358;Inherit;False;1143.62;715.2286;;15;3834;4591;260;711;709;486;3835;3888;3886;3887;653;3833;255;252;3814;Particle transform;0.5424528,1,0.9184569,1;0;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;742;-3294.294,-865.4466;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1914;-3302.18,-986.6736;Inherit;False;Property;_RandomOffset;RandomOffset;52;1;[HideInInspector];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;260;-4548.069,-345.5797;Inherit;False;1;3;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectScaleNode;3834;-4523.051,-532.8078;Inherit;False;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;1913;-3048.042,-882.5033;Inherit;False;Property;_ParticleMesh;ParticleMesh;43;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;255;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;484;-2586.187,-1047.923;Inherit;False;970.912;381.7733;;7;1892;477;466;467;416;463;4498;Flicker;0.5613208,0.8882713,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4591;-4327.826,-369.1419;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;3727;-2813.076,-881.6541;Inherit;False;RANDOMNESS;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;4517;-4769.077,-132.6272;Inherit;False;628.2538;236.657;screen pos;3;4516;4515;4514;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ObjectPositionNode;3814;-4228.537,-841.8353;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexCoordVertexDataNode;252;-4448.995,-741.957;Inherit;False;0;3;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;3833;-4177.858,-412.674;Inherit;False;Property;_ParticleMesh;ParticleMesh;43;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;255;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;463;-2436.796,-808.6651;Inherit;False;Property;_SizeFlickering;Size Flickering;24;0;Create;True;0;0;0;False;0;False;0.1;0.5;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4498;-2565.921,-954.3461;Inherit;False;FlickerFunction;18;;1436;f6225b1ef66c663478bc4f0259ec00df;0;4;9;FLOAT;0;False;8;FLOAT;0;False;21;FLOAT;0;False;29;FLOAT;0;False;2;FLOAT;0;FLOAT3;45
Node;AmplifyShaderEditor.StaticSwitch;255;-4025.325,-766.166;Inherit;False;Property;_ParticleMode;Particle Mode;43;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;3887;-3949.127,-349.636;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ScreenPosInputsNode;4514;-4719.077,-82.62715;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;416;-2267.689,-954.5582;Inherit;False;FlickerAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;467;-2163.736,-808.0251;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;480;-1586.786,-1078.922;Inherit;False;1355.739;434.4033;;11;3954;4107;262;3966;55;3819;3708;539;478;654;3836;World SphericalMask;0.9034846,0.5330188,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;3573;-4116.101,-14.06753;Inherit;False;870.2667;343.3553;;3;3571;4467;3821;Distance Fading;0.4079299,0.8396226,0.4819806,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;653;-3777.223,-766.7069;Inherit;False;POSITION;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;3886;-3845.127,-349.636;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;4515;-4550.602,-81.08909;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;466;-2013.38,-954.6299;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4867;-1566.887,-1021.288;Inherit;False;Reconstruct World Pos from Depth VR;-1;;2080;474d2b03c8647914986393f8dfbd9fe4;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;2273;-4730.541,143.3773;Inherit;False;579.5842;224.0259;;3;1927;2275;4518;NormalsTexture;0.5424528,1,0.8822392,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;3821;-4086.4,142.9396;Inherit;False;653;POSITION;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;3888;-3744.452,-324.9914;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;539;-1228.561,-1021.273;Inherit;False;ReconstructedPos;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;654;-1194.366,-953.3782;Inherit;False;653;POSITION;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4516;-4350.054,-80.06077;Inherit;False;ScreenPos;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;477;-1827.317,-955.2652;Inherit;False;FlickerSize;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4569;-3723.431,239.1101;Inherit;False;DayAlpha;46;;1441;bc1f8ebe2e26696419e0099f8a3e27dc;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;3835;-3635.171,-324.2941;Inherit;False;SCALE;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3819;-1017.181,-1021.682;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;4518;-4696.018,239.1036;Inherit;False;4516;ScreenPos;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;478;-931.2987,-775.1152;Inherit;False;477;FlickerSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4792;-3905.166,46.45875;Inherit;False;AdvancedCameraFade;12;;1803;e6e830f789d28b746963801d61c2a1ec;0;6;40;FLOAT;0;False;46;FLOAT;0;False;47;FLOAT;0;False;48;FLOAT;0;False;17;FLOAT3;0,0,0;False;20;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;442;-2577.175,-598.0592;Inherit;False;2284.388;428.8218;Be sure to have a renderer feature that writes to _CameraNormalsTexture for this to work;18;549;4264;552;551;562;4213;471;4216;1882;4219;553;436;2274;3808;4235;4234;4595;4598;Normal Direction Masking;0.6086246,0.5235849,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4467;-3592.764,45.93977;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;2275;-4517.167,239.2081;Inherit;False;#ifdef STEREO_INSTANCING_ON$return SAMPLE_TEXTURE2D_ARRAY(_CameraNormalsTexture,sampler_CameraNormalsTexture,uvs,unity_StereoEyeIndex)@$#else$return SAMPLE_TEXTURE2D(_CameraNormalsTexture,sampler_CameraNormalsTexture,uvs)@$#endif;4;Create;1;True;uvs;FLOAT2;0,0;In;;Inherit;False;Normal Tex URP;True;False;0;;False;1;0;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScaleNode;3708;-762.1542,-775.359;Inherit;False;0.45;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;3836;-855.9872,-877.1537;Inherit;False;3835;SCALE;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4107;-871.8685,-1021.94;Inherit;False;LocalPos;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;1356;-3111.287,-14.07189;Inherit;False;907.1592;385.8282;;5;1881;3576;3838;3540;3830;Experimental Shadows;1,0.0518868,0.0518868,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;513;-3363.908,-590.4105;Inherit;False;781.0093;390.9941;;5;3969;1929;3729;542;4605;Noise;1,0.6084906,0.6084906,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;3571;-3455.373,47.83217;Inherit;False;DistanceFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1927;-4377.426,238.8262;Inherit;False;worldNormals;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3966;-642.7975,-873.377;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;55;-663.6138,-1022.612;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4234;-2473.791,-472.0054;Inherit;False;4107;LocalPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;510;-92.06051,-1081.912;Inherit;False;1034.724;415.4808;;10;66;745;514;769;509;3711;3971;3712;3981;4624;Light Mask Hardness;1,0.8561655,0.3632075,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;3830;-3027.767,81.16975;Inherit;False;653;POSITION;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;3540;-3057.828,146.3713;Inherit;False;539;ReconstructedPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;3576;-3055.692,271.4933;Inherit;False;3571;DistanceFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;3838;-3026.032,208.7609;Inherit;False;3835;SCALE;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;262;-492.2797,-1022.586;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;4235;-2304.88,-472.5057;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;542;-3313.745,-372.6483;Inherit;False;539;ReconstructedPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;3729;-3310.612,-298.886;Inherit;False;3727;RANDOMNESS;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1929;-3314.647,-449.2849;Inherit;False;1927;worldNormals;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;3954;-380.211,-1022.654;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;3808;-2124.026,-473.2162;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;4619;-3084.65,-435.7374;Inherit;False;3DNoiseMap;25;;1804;2fca756491ec7bf4e9c71d18280c45cc;0;5;257;FLOAT3;0,0,0;False;21;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;60;FLOAT;0;False;229;FLOAT2;0,0;False;2;FLOAT;0;FLOAT;213
Node;AmplifyShaderEditor.RangedFloatNode;66;-106.8148,-767.4788;Inherit;False;Property;_LightSoftness;Light Softness;2;0;Create;True;0;0;0;False;1;Space(5);False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4796;-2811.362,115.2712;Inherit;False;ExperimentalScreenSpaceShadows;40;;1805;79f826106fc5f154c96059cc1326b755;0;4;337;FLOAT3;0,0,0;False;336;FLOAT3;0,0,0;False;370;FLOAT;0;False;335;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;3968;1169.43,-1086.585;Inherit;False;675.3597;364.2049;Additional Masks;6;3902;487;485;3958;3952;3984;;0.3531061,0.406577,0.6509434,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1881;-2463.868,109.6569;Inherit;False;ScreenSpaceShadows;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;3969;-2798.872,-440.3917;Inherit;False;noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;3981;3.173216,-1021.999;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4264;-1983.805,-473.4308;Inherit;False;LightDir;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;2274;-2071.707,-400.0095;Inherit;False;1927;worldNormals;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScaleNode;4624;157.0977,-735.9185;Inherit;False;1.1;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;500;962.7515,-619.572;Inherit;False;1117.27;328.2859;;8;4215;492;4203;3992;3896;4214;555;4200;Light Posterize;0.5707547,1,0.9954711,1;0;0
Node;AmplifyShaderEditor.RelayNode;3984;1200.194,-1020.967;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;3712;282.4139,-766.8303;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;3971;55.14507,-891.7;Inherit;False;3969;noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;436;-1785.206,-473.019;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;553;-1831.932,-373.276;Inherit;False;3969;noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1882;-1877.893,-306.75;Inherit;False;1881;ScreenSpaceShadows;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;3952;1324.082,-1017.805;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;30;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;3711;416.8805,-766.8687;Inherit;False;0.5;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;509;234.626,-914.4458;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;492;990.0972,-391.6727;Inherit;False;Property;_LightPosterize;Light Posterize;3;1;[IntRange];Create;True;0;0;0;False;0;False;1;0;0;128;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4595;-1638.537,-474.6681;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;3958;1457.735,-1017.747;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;769;577.5012,-769.8422;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;514;373.3369,-938.2104;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4215;1264.353,-392.126;Inherit;False;lPosterize;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;549;-1501.529,-344.7776;Inherit;False;Property;_ShadingSoftness;Shading Softness;5;0;Create;True;0;0;0;False;0;False;0.5;0.554;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;4219;-1489.469,-476.0597;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;3902;1622.798,-1016.304;Inherit;False;BrightnessBoost;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;745;732.6475,-938.0301;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;4598;-1214.524,-476.1245;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4216;-1210.152,-357.961;Inherit;False;4215;lPosterize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;4214;1151.298,-550.8469;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;3896;1069.695,-469.6752;Inherit;False;3902;BrightnessBoost;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4213;-1042.616,-472.2397;Inherit;False;SimplePosterize;-1;;1807;163fbd1f7d6893e4ead4288913aedc26;0;2;9;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;471;-1017.391,-356.4963;Inherit;False;Property;_ShadingBlend;Shading Blend;4;0;Create;True;0;0;0;False;1;Space(5);False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;4286;-2084.174,-101.4854;Inherit;False;1843.076;430.4736;;20;4287;4285;4302;4274;4500;4229;4301;4278;4230;4280;4282;4231;4244;4275;4267;4276;4268;4266;4265;4236;Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;3992;1299.5,-489.5305;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;562;-753.7457,-471.736;Inherit;False;2;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;485;1328.647,-900.0927;Inherit;False;2;0;FLOAT;0.01;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4236;-2034.173,90.58859;Inherit;False;4264;LightDir;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;4265;-2009.022,-51.48553;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;4203;1440.282,-490.2576;Inherit;False;SimplePosterize;-1;;1808;163fbd1f7d6893e4ead4288913aedc26;0;2;9;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;551;-646.3391,-471.2158;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;4299;2112.15,-616.8647;Inherit;False;601.5642;321.6143;;4;4298;4297;4294;4296;Final Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;487;1448.365,-899.7147;Inherit;False;surfaceMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4266;-1821.406,-24.03392;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;4268;-1831.354,76.26473;Inherit;False;1927;worldNormals;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector3Node;4276;-1887.234,153.2693;Inherit;False;Constant;_Vector0;Vector 0;22;0;Create;True;0;0;0;False;0;False;1,0.99,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4200;1722.03,-553.4279;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;552;-509.6927,-471.2604;Inherit;False;ShadingMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;684;-214.2027,-140.3656;Inherit;False;1486.598;484.9661;;14;141;557;707;1976;143;200;140;201;481;4288;4300;4006;4289;4537;Light Radius Mix;1,0.4198113,0.7623972,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;4267;-1650.484,-22.0349;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4275;-1627.239,74.07735;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;4244;-1708.263,204.247;Inherit;False;Constant;_SpecPower;Spec Power;43;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;555;1872.633,-552.6373;Inherit;False;GradientMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4296;2156.409,-418.4277;Inherit;False;487;surfaceMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4294;2139.463,-487.8464;Inherit;False;552;ShadingMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;4231;-1461.507,41.2541;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;4282;-1450.671,149.3001;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;557;-211.6858,77.72614;Inherit;False;555;GradientMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4297;2358.666,-551.0913;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;4547;-256.7654,-604.4515;Inherit;False;1195.04;419.7061;;10;4548;4529;4531;4528;4527;4530;4535;4534;4533;4532;Halo;0,0.9419041,1,1;0;0
Node;AmplifyShaderEditor.ScaleNode;4280;-1247.206,151.5504;Inherit;False;200;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;4230;-1300.086,40.97846;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4278;-1293.954,-48.4472;Inherit;False;Property;_SpecIntensity;Spec Intensity;35;0;Create;True;0;0;0;False;0;False;0.5;0.79;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;707;-7.319501,78.04494;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4298;2502.125,-549.413;Inherit;False;FinalLightMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;4537;-162.6543,-104.7736;Inherit;True;Property;_GradientTexture;Gradient Texture;0;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;4301;-1081.074,142.3799;Inherit;False;4298;FinalLightMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;4229;-1083.26,40.02241;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;4500;-1021.588,-48.47894;Inherit;False;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;481;152.536,-81.76846;Inherit;True;Property;_GradientTexture1;GradientTexture1;1;3;[Header];[NoScaleOffset];[SingleLineTexture];Create;True;1;___Light Settings___;0;0;False;1;Space(10);False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;140;162.8897,108.7269;Inherit;False;Property;_LightTint;Light Tint;1;1;[HDR];Create;True;1;___Light Settings___;0;0;False;0;False;1,1,1,1;3.550702,5.723955,7.603524,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;4532;-244.6469,-300.284;Inherit;False;3835;SCALE;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;201;417.1223,100.2544;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4274;-873.5836,14.90791;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4302;-869.3937,-55.40606;Inherit;False;Constant;_s;s;23;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;200;462.0041,-82.44735;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4533;-75.64525,-367.3888;Inherit;False;Constant;_Float1;Float 1;23;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;4534;-76.67821,-301.1487;Inherit;False;0.1;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4006;611.8979,-81.23064;Inherit;False;Alpha Split;-1;;1809;07dab7960105b86429ac8eebd729ed6d;0;1;2;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.GetLocalVarNode;4300;594.6234,17.88296;Inherit;False;4298;FinalLightMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1976;618.0422,86.14977;Inherit;False;Constant;_intensityScale;intensityScale;20;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;4535;69.02787,-324.9747;Inherit;False;Property;_ParticleMesh;ParticleMesh;43;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;255;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4530;120.6909,-395.9944;Inherit;False;477;FlickerSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;4285;-720.1268,-10.71825;Inherit;False;Property;_SpecularHighlight;Specular Highlight;34;0;Create;True;0;0;0;False;1;Space(20);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;804.3857,-15.83389;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4527;236.7258,-485.5515;Inherit;False;539;ReconstructedPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;4528;261.4258,-554.4515;Inherit;False;653;POSITION;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4531;313.5437,-351.4328;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;4529;268.9737,-420.355;Inherit;False;4516;ScreenPos;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4287;-453.33,-8.372881;Inherit;False;Spec;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;3841;1315.504,-130.634;Inherit;False;1131.77;484.6983;Final Mix;11;4553;3579;1901;657;3572;1384;1897;600;4549;2676;4520;;0.2959238,0.4695747,0.6603774,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;956.7151,-80.82604;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;4289;955.4199,27.88382;Inherit;False;4287;Spec;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;4862;501.7485,-425.7458;Inherit;False;HaloFunction;6;;2060;739bbcda129bbae47870a33d01fe91b1;0;5;69;FLOAT3;0,0,0;False;70;FLOAT3;0,0,0;False;72;FLOAT2;0,0;False;74;FLOAT;0;False;80;SAMPLER2D;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4288;1126.497,-80.44186;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4548;735.5288,-426.4152;Inherit;False;halo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;4520;1340.243,-18.70964;Inherit;False;4516;ScreenPos;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;2676;1338.536,49.75436;Inherit;False;555;GradientMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1892;-2269.525,-885.0829;Inherit;False;FlickerHue;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;4549;1564.018,42.02893;Inherit;False;4548;halo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;4793;1520.805,-78.38047;Inherit;False;AccurateColors;44;;1814;570575a1eb6bdc7409ed58545512a33b;0;3;12;FLOAT3;0,0,0;False;13;FLOAT2;0,0;False;15;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;3572;1753.636,108.4281;Inherit;False;3571;DistanceFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1897;1764.77,19.50891;Inherit;False;1892;FlickerHue;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;600;1766.66,-78.28804;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1384;1783.247,185.341;Inherit;False;416;FlickerAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;657;1946.323,-78.65127;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1901;1965.301,108.9108;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3579;2094.853,-78.75454;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;4553;2248.181,-79.12321;Inherit;False;Dithering;36;;1815;b490ae982132eea449373f63bf44f108;0;1;17;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;3578;2466.482,-78.76494;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;4587;2545.603,40.57138;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;4615;2965.705,94.20473;Inherit;False;297;283;BehindTheScene;3;4617;4616;4561;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;4572;2679.029,39.72437;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;4605;-3287.827,-512.5876;Inherit;False;4107;LocalPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StickyNoteNode;486;-4165.404,-623.873;Inherit;False;371.5999;141.3;Particle Custom Vertex stream setup !!;;1,0.9012449,0.3254717,1;1. Center = TexCoord0.xyz  (Particle Position)$$2. StableRandom.x TexCoord0.w (random flicker)$$3. Size.xyz = TexCoord1.xyz (Particle Size);0;0
Node;AmplifyShaderEditor.StickyNoteNode;709;-4461.079,-779.02;Inherit;False;215;182;Center (Texcoord0.xyz);;1,1,1,1;;0;0
Node;AmplifyShaderEditor.StickyNoteNode;711;-4555.474,-386.069;Inherit;False;216;177;Size.xyz (Texcoord1.xyz);;1,1,1,1;;0;0
Node;AmplifyShaderEditor.StickyNoteNode;3832;-3307.867,-904.8484;Inherit;False;232.9993;214.9999;Random (Texcoord0.w);;1,1,1,1;;0;0
Node;AmplifyShaderEditor.RangedFloatNode;4616;2984.638,214.6363;Inherit;False;Property;_SrcBlend;SrcBlend;50;3;[HideInInspector];[IntRange];[Enum];Create;True;0;3;Default;0;Off;1;On;2;1;UnityEngine.Rendering.BlendMode;True;0;False;1;1;0;12;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4617;2985.638,285.6364;Inherit;False;Property;_DstBlend;DstBlend;51;3;[HideInInspector];[IntRange];[Enum];Create;True;0;3;Default;0;Off;1;On;2;1;UnityEngine.Rendering.BlendMode;True;0;False;1;1;0;12;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4561;3030.705,144.2048;Inherit;False;Property;_DepthWrite;Depth Write;49;1;[Enum];Create;True;0;3;Default;0;Off;1;On;2;0;True;1;Space(5);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;4585;2772.483,-80.11815;Inherit;False;Property;_Blendmode;Blendmode;48;0;Create;True;0;0;0;False;1;Space(15);False;0;0;0;True;;KeywordEnum;3;Additive;Contrast;Negative;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2107;2560,208;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2108;2560,208;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2109;2560,208;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2105;1519.678,2379.429;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;3;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2106;3002.883,-80.01085;Float;False;True;-1;2;FPL.CustomMaterialEditor;0;13;LazyEti/URP/FakePointLight;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;1;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Overlay=RenderType;Queue=Overlay=Queue=0;UniversalMaterialType=Unlit;True;3;True;12;all;0;True;True;1;1;True;_SrcBlend;1;True;_DstBlend;0;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;True;_DepthWrite;True;7;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;7;Define;REQUIRE_DEPTH_TEXTURE 1;False;;Custom;False;0;0;;Include;;False;;Native;False;0;0;;Custom;#ifdef STEREO_INSTANCING_ON;False;;Custom;False;0;0;;Custom;TEXTURE2D_ARRAY(_CameraNormalsTexture)@ SAMPLER(sampler_CameraNormalsTexture)@;False;;Custom;False;0;0;;Custom;#else;False;;Custom;False;0;0;;Custom;TEXTURE2D(_CameraNormalsTexture)@ SAMPLER(sampler_CameraNormalsTexture)@;False;;Custom;False;0;0;;Custom;#endif;False;;Custom;False;0;0;;;0;0;Standard;22;Surface;1;638684424641455607;  Blend;0;0;Two Sided;1;0;Alpha Clipping;0;638684424561819053;  Use Shadow Threshold;0;0;Cast Shadows;0;638684424595876409;Receive Shadows;0;638684424611325586;GPU Instancing;1;0;LOD CrossFade;0;0;Built-in Fog;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;5;False;True;False;False;False;False;;True;0
WireConnection;1913;1;1914;0
WireConnection;1913;0;742;4
WireConnection;4591;0;3834;0
WireConnection;4591;1;260;0
WireConnection;3727;0;1913;0
WireConnection;3833;1;3834;0
WireConnection;3833;0;4591;0
WireConnection;4498;29;3727;0
WireConnection;255;1;3814;0
WireConnection;255;0;252;0
WireConnection;3887;0;3833;0
WireConnection;416;0;4498;0
WireConnection;467;0;463;0
WireConnection;653;0;255;0
WireConnection;3886;0;3887;0
WireConnection;3886;1;3887;1
WireConnection;4515;0;4514;0
WireConnection;466;0;416;0
WireConnection;466;3;467;0
WireConnection;3888;0;3886;0
WireConnection;3888;1;3887;2
WireConnection;539;0;4867;0
WireConnection;4516;0;4515;0
WireConnection;477;0;466;0
WireConnection;3835;0;3888;0
WireConnection;3819;0;539;0
WireConnection;3819;1;654;0
WireConnection;4792;17;3821;0
WireConnection;4467;0;4792;0
WireConnection;4467;1;4569;0
WireConnection;2275;0;4518;0
WireConnection;3708;0;478;0
WireConnection;4107;0;3819;0
WireConnection;3571;0;4467;0
WireConnection;1927;0;2275;0
WireConnection;3966;0;3836;0
WireConnection;3966;1;3708;0
WireConnection;55;0;4107;0
WireConnection;262;0;55;0
WireConnection;262;1;3966;0
WireConnection;4235;0;4234;0
WireConnection;3954;0;262;0
WireConnection;3808;0;4235;0
WireConnection;4619;21;1929;0
WireConnection;4619;1;542;0
WireConnection;4619;60;3729;0
WireConnection;4796;337;3830;0
WireConnection;4796;336;3540;0
WireConnection;4796;370;3838;0
WireConnection;4796;335;3576;0
WireConnection;1881;0;4796;0
WireConnection;3969;0;4619;0
WireConnection;3981;0;3954;0
WireConnection;4264;0;3808;0
WireConnection;4624;0;66;0
WireConnection;3984;0;3981;0
WireConnection;3712;0;4624;0
WireConnection;436;0;4264;0
WireConnection;436;1;2274;0
WireConnection;3952;0;3984;0
WireConnection;3711;0;3712;0
WireConnection;509;0;3981;0
WireConnection;509;1;3971;0
WireConnection;4595;0;436;0
WireConnection;4595;1;553;0
WireConnection;4595;2;1882;0
WireConnection;3958;0;3952;0
WireConnection;769;0;3711;0
WireConnection;514;0;3981;0
WireConnection;514;1;509;0
WireConnection;4215;0;492;0
WireConnection;4219;0;4595;0
WireConnection;3902;0;3958;0
WireConnection;745;0;514;0
WireConnection;745;1;3711;0
WireConnection;745;2;769;0
WireConnection;4598;0;4219;0
WireConnection;4598;2;549;0
WireConnection;4214;0;745;0
WireConnection;4213;9;4598;0
WireConnection;4213;8;4216;0
WireConnection;3992;0;4214;0
WireConnection;3992;1;3896;0
WireConnection;562;0;4213;0
WireConnection;562;1;471;0
WireConnection;485;1;3984;0
WireConnection;4203;9;3992;0
WireConnection;4203;8;4215;0
WireConnection;551;0;562;0
WireConnection;487;0;485;0
WireConnection;4266;0;4265;0
WireConnection;4266;1;4236;0
WireConnection;4200;0;4214;0
WireConnection;4200;1;4203;0
WireConnection;552;0;551;0
WireConnection;4267;0;4266;0
WireConnection;4275;0;4268;0
WireConnection;4275;1;4276;0
WireConnection;555;0;4200;0
WireConnection;4231;0;4267;0
WireConnection;4231;1;4275;0
WireConnection;4282;0;4244;0
WireConnection;4297;0;555;0
WireConnection;4297;1;4294;0
WireConnection;4297;2;4296;0
WireConnection;4280;0;4282;0
WireConnection;4230;0;4231;0
WireConnection;707;0;557;0
WireConnection;4298;0;4297;0
WireConnection;4229;0;4230;0
WireConnection;4229;1;4280;0
WireConnection;4500;0;4278;0
WireConnection;481;0;4537;0
WireConnection;481;1;707;0
WireConnection;4274;0;4500;0
WireConnection;4274;1;4229;0
WireConnection;4274;2;4301;0
WireConnection;200;0;481;0
WireConnection;200;1;140;0
WireConnection;200;2;201;0
WireConnection;4534;0;4532;0
WireConnection;4006;2;200;0
WireConnection;4535;1;4533;0
WireConnection;4535;0;4534;0
WireConnection;4285;1;4302;0
WireConnection;4285;0;4274;0
WireConnection;143;0;4006;6
WireConnection;143;1;4300;0
WireConnection;143;2;1976;0
WireConnection;4531;0;4530;0
WireConnection;4531;1;4535;0
WireConnection;4287;0;4285;0
WireConnection;141;0;4006;0
WireConnection;141;1;143;0
WireConnection;4862;69;4528;0
WireConnection;4862;70;4527;0
WireConnection;4862;72;4529;0
WireConnection;4862;74;4531;0
WireConnection;4862;80;4537;0
WireConnection;4288;0;141;0
WireConnection;4288;1;4289;0
WireConnection;4548;0;4862;0
WireConnection;1892;0;4498;45
WireConnection;4793;12;4288;0
WireConnection;4793;13;4520;0
WireConnection;4793;15;2676;0
WireConnection;600;0;4793;0
WireConnection;600;1;4549;0
WireConnection;657;0;600;0
WireConnection;657;1;1897;0
WireConnection;1901;0;3572;0
WireConnection;1901;1;1384;0
WireConnection;3579;0;657;0
WireConnection;3579;1;1901;0
WireConnection;4553;17;3579;0
WireConnection;3578;0;4553;0
WireConnection;4587;0;3578;0
WireConnection;4572;0;4587;0
WireConnection;4585;1;3578;0
WireConnection;4585;0;3578;0
WireConnection;4585;2;4572;0
WireConnection;2106;2;4585;0
ASEEND*/
//CHKSM=B805C4DCFEB2E5AC89F2B680BE215D5566157047