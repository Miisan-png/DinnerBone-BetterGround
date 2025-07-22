Shader "Custom/TilingPatternShader"
{
    Properties
    {
        _MainTex ("Pattern Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _TilingX ("Tiling X", Range(1, 50)) = 10
        _TilingY ("Tiling Y", Range(1, 50)) = 1
        _OffsetX ("Offset X", Range(-1, 1)) = 0
        _OffsetY ("Offset Y", Range(-1, 1)) = 0
        _AnimationSpeed ("Animation Speed", Range(0, 5)) = 0
        _PatternAlpha ("Pattern Alpha", Range(0, 1)) = 1
        _BackgroundAlpha ("Background Alpha", Range(0, 1)) = 0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
        _Rotation ("Rotation", Range(0, 360)) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _TilingX;
                float _TilingY;
                float _OffsetX;
                float _OffsetY;
                float _AnimationSpeed;
                float _PatternAlpha;
                float _BackgroundAlpha;
                float _AlphaThreshold;
                float _Rotation;
            CBUFFER_END
            
            float2 RotateUV(float2 uv, float rotation)
            {
                float rad = radians(rotation);
                float cosAngle = cos(rad);
                float sinAngle = sin(rad);
                
                uv -= 0.5;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosAngle - uv.y * sinAngle;
                rotatedUV.y = uv.x * sinAngle + uv.y * cosAngle;
                rotatedUV += 0.5;
                
                return rotatedUV;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float2 uv = RotateUV(i.uv, _Rotation);
                
                float time = _Time.y * _AnimationSpeed;
                float2 animatedOffset = float2(_OffsetX + time, _OffsetY);
                
                float2 tiledUV = uv * float2(_TilingX, _TilingY) + animatedOffset;
                
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, tiledUV);
                
                float3 finalColor = texColor.rgb * _Color.rgb;
                
                float alpha = texColor.a;
                
                if (alpha < _AlphaThreshold)
                {
                    alpha = _BackgroundAlpha;
                }
                else
                {
                    alpha *= _PatternAlpha * _Color.a;
                }
                
                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _TilingX;
            float _TilingY;
            float _OffsetX;
            float _OffsetY;
            float _AnimationSpeed;
            float _PatternAlpha;
            float _BackgroundAlpha;
            float _AlphaThreshold;
            float _Rotation;
            
            float2 RotateUV(float2 uv, float rotation)
            {
                float rad = radians(rotation);
                float cosAngle = cos(rad);
                float sinAngle = sin(rad);
                
                uv -= 0.5;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosAngle - uv.y * sinAngle;
                rotatedUV.y = uv.x * sinAngle + uv.y * cosAngle;
                rotatedUV += 0.5;
                
                return rotatedUV;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = RotateUV(i.uv, _Rotation);
                
                float time = _Time.y * _AnimationSpeed;
                float2 animatedOffset = float2(_OffsetX + time, _OffsetY);
                
                float2 tiledUV = uv * float2(_TilingX, _TilingY) + animatedOffset;
                
                fixed4 texColor = tex2D(_MainTex, tiledUV);
                
                fixed3 finalColor = texColor.rgb * _Color.rgb;
                
                float alpha = texColor.a;
                
                if (alpha < _AlphaThreshold)
                {
                    alpha = _BackgroundAlpha;
                }
                else
                {
                    alpha *= _PatternAlpha * _Color.a;
                }
                
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}