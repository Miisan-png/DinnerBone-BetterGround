Shader "Unlit/Wall_Fade_Effect" 
{     
    Properties     
    {         
        _Color ("Base Color", Color) = (0.5, 0.7, 1.0, 1.0)
        _FadeAmount ("Fade Amount", Range(0, 1)) = 0.2
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _FadeDistance ("Fade Distance", Float) = 5.0
        _FadeSharpness ("Fade Sharpness", Float) = 2.0
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 0.5
        _Fresnel ("Fresnel Power", Range(0, 5)) = 1.5
    }     
    SubShader     
    {         
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
                float3 normal : NORMAL;             
            };              
            struct v2f             
            {                 
                float2 uv : TEXCOORD0;                 
                UNITY_FOG_COORDS(1)                 
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 viewDir : TEXCOORD4;             
            };              
            sampler2D _MainTex;             
            float4 _MainTex_ST;
            float4 _Color;
            float _FadeAmount;
            float4 _PlayerPos;
            float _FadeDistance;
            float _FadeSharpness;
            float _EdgeGlow;
            float _Fresnel;              
            v2f vert (appdata v)             
            {                 
                v2f o;                 
                o.vertex = UnityObjectToClipPos(v.vertex);                 
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);                 
                UNITY_TRANSFER_FOG(o,o.vertex);                 
                return o;             
            }              
            fixed4 frag (v2f i) : SV_Target             
            {                 
                // Base color
                fixed4 col = _Color;
                
                // Calculate distance from fragment to player
                float distanceToPlayer = distance(i.worldPos, _PlayerPos.xyz);
                
                // Create fade factor based on distance
                float fadeFactor = saturate(distanceToPlayer / _FadeDistance);
                fadeFactor = pow(fadeFactor, _FadeSharpness);
                
                // Fresnel effect for nice edge glow
                float fresnel = 1.0 - saturate(dot(i.worldNormal, i.viewDir));
                fresnel = pow(fresnel, _Fresnel);
                
                // Add edge glow
                col.rgb += fresnel * _EdgeGlow;
                
                // Create a subtle grid pattern for visual interest
                float2 grid = abs(frac(i.uv * 8.0) - 0.5) / fwidth(i.uv * 8.0);
                float gridLine = saturate(min(grid.x, grid.y));
                col.rgb += (1.0 - gridLine) * 0.1;
                
                // Interpolate between fade amount and full opacity
                float alpha = lerp(_FadeAmount, 1.0, fadeFactor);
                
                col.a *= alpha;
                
                // Apply fog                 
                UNITY_APPLY_FOG(i.fogCoord, col);                 
                return col;             
            }             
            ENDCG         
        }     
    } 
}