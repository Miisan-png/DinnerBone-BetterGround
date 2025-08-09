Shader "Custom/RE4StyleWaterWorldUV"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (0.2,0.25,0.25,0.6)
        _Speed ("Scroll Speed", Vector) = (0.01, 0.005, 0, 0)
        _WaveStrength ("Wave Strength", Range(0,0.05)) = 0.015
        _Tiling ("Texture Scale", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Tint;
            float4 _Speed;
            float _WaveStrength;
            float _Tiling;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float t = _Time.y;

                float2 uv = worldPos.xz * _Tiling;
                uv += _Speed.xy * t;
                uv.x += sin((uv.y + t * 0.2) * 6.28318) * _WaveStrength;
                uv.y += cos((uv.x + t * 0.2) * 6.28318) * _WaveStrength;

                o.uv = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Tint;
                return col;
            }
            ENDCG
        }
    }
}
