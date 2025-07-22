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
    }
    
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100
        
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
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
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
            
            v2f vert (appdata v)
            {
                v2f o;
                
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 windDir = normalize(_WindDirection.xyz);
                
                float time = _Time.y * _WindSpeed;
                float heightFactor = v.vertex.y + 0.5;
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
                
                v.vertex.xyz += windOffset;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - _AlphaThreshold);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}