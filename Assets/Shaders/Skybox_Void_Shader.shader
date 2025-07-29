Shader "Custom/BlackVoidSkybox"
{
    Properties
    {
        _MainColor ("Main Void Color", Color) = (0, 0, 0, 1)
        _HazeColor ("Purple Haze Color", Color) = (0.5, 0, 0.8, 1)
        _HazeIntensity ("Haze Intensity", Range(0, 1)) = 0.3
        _WispScale ("Wisp Scale", Range(0.1, 2)) = 1
        _WispSpeed ("Wisp Speed", Range(0, 5)) = 1
        _WispDensity ("Wisp Density", Range(0, 1)) = 0.4
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            fixed4 _MainColor;
            fixed4 _HazeColor;
            float _HazeIntensity;
            float _WispScale;
            float _WispSpeed;
            float _WispDensity;
            
            // Simple noise function
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            float fbm(float2 st)
            {
                float value = 0.0;
                float amplitude = 0.5;
                
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(st);
                    st *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize world position for consistent UV mapping
                float3 viewDir = normalize(i.worldPos);
                
                // Create UV coordinates from world position
                float2 uv = viewDir.xz * _WispScale + viewDir.y * 0.5;
                
                // Animate the UV coordinates
                float time = _Time.y * _WispSpeed;
                uv += float2(sin(time * 0.3), cos(time * 0.4)) * 0.1;
                
                // Generate noise for wisps
                float noise1 = fbm(uv + time * 0.2);
                float noise2 = fbm(uv * 1.5 - time * 0.15);
                float noise3 = fbm(uv * 2.3 + time * 0.1);
                
                // Combine noises for wisp effect
                float wispMask = noise1 * noise2 * noise3;
                wispMask = smoothstep(1.0 - _WispDensity, 1.0, wispMask);
                
                // Distance-based fade (stronger haze in distance)
                float distanceFade = saturate(length(viewDir.xz) * 2.0);
                
                // Create the final color
                fixed4 voidColor = _MainColor;
                fixed4 hazeColor = _HazeColor * _HazeIntensity * wispMask;
                
                // Mix based on distance and wisp intensity
                fixed4 finalColor = lerp(voidColor, voidColor + hazeColor, distanceFade);
                
                return finalColor;
            }
            ENDCG
        }
    }
}