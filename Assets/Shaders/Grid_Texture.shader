Shader "Unlit/TriplanarGrid"
{
    Properties
    {
        _MainTex("Grid Texture", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZWrite On
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Tiling;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 absNormal = abs(normalize(i.normal));
                float3 worldPos = i.worldPos * _Tiling;

                float2 uvX = worldPos.zy;
                float2 uvY = worldPos.xz;
                float2 uvZ = worldPos.xy;

                float3 blend = absNormal;
                blend = pow(blend, 5);
                blend /= dot(blend, 1.0);

                fixed4 xTex = tex2D(_MainTex, uvX);
                fixed4 yTex = tex2D(_MainTex, uvY);
                fixed4 zTex = tex2D(_MainTex, uvZ);

                return xTex * blend.x + yTex * blend.y + zTex * blend.z;
            }
            ENDCG
        }
    }
}
