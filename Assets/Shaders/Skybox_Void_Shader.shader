Shader "Custom/BlackVoidSkybox"
{
    Properties
    {
        _MainColor ("Main Void Color", Color) = (0, 0, 0, 1)
        _HazeColor1 ("Pastel Haze Color 1", Color) = (0.8, 0.6, 0.9, 1)
        _HazeColor2 ("Pastel Haze Color 2", Color) = (0.6, 0.8, 0.9, 1)
        _HazeColor3 ("Pastel Haze Color 3", Color) = (0.9, 0.7, 0.6, 1)
        _HazeIntensity ("Haze Intensity", Range(0, 1)) = 0.3
        _WispScale ("Wisp Scale", Range(0.1, 2)) = 1
        _WispSpeed ("Wisp Speed", Range(0, 5)) = 1
        _WispDensity ("Wisp Density", Range(0, 1)) = 0.4
        _StarDensity ("Star Density", Range(0, 1)) = 0.5
        _StarBrightness ("Star Brightness", Range(0, 2)) = 1.2
        _StarTwinkleSpeed ("Star Twinkle Speed", Range(0, 10)) = 3
        _StarSize ("Star Size", Range(1, 10)) = 3
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
            fixed4 _HazeColor1;
            fixed4 _HazeColor2;
            fixed4 _HazeColor3;
            float _HazeIntensity;
            float _WispScale;
            float _WispSpeed;
            float _WispDensity;
            float _StarDensity;
            float _StarBrightness;
            float _StarTwinkleSpeed;
            float _StarSize;
            
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
            
            float starField(float2 uv, float threshold)
            {
                float2 gridUV = floor(uv * 50.0);
                float2 localUV = frac(uv * 50.0) - 0.5;
                
                float starRandom = random(gridUV);
                
                if (starRandom > threshold)
                {
                    float starDistance = length(localUV);
                    float twinkle = sin(_Time.y * _StarTwinkleSpeed + starRandom * 6.28) * 0.3 + 0.7;
                    float star = 1.0 - smoothstep(0.0, 0.1 / _StarSize, starDistance);
                    return star * twinkle * _StarBrightness;
                }
                
                return 0.0;
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
                float3 viewDir = normalize(i.worldPos);
                
                float2 uv = viewDir.xz * _WispScale + viewDir.y * 0.5;
                
                float time = _Time.y * _WispSpeed;
                uv += float2(sin(time * 0.3), cos(time * 0.4)) * 0.1;
                
                float noise1 = fbm(uv + time * 0.2);
                float noise2 = fbm(uv * 1.5 - time * 0.15);
                float noise3 = fbm(uv * 2.3 + time * 0.1);
                
                float wispMask = noise1 * noise2 * noise3;
                wispMask = smoothstep(1.0 - _WispDensity, 1.0, wispMask);
                
                float distanceFade = saturate(length(viewDir.xz) * 2.0);
                
                float colorNoise1 = fbm(uv * 0.5 + time * 0.05);
                float colorNoise2 = fbm(uv * 0.3 - time * 0.03);
                float colorNoise3 = fbm(uv * 0.7 + time * 0.07);
                
                fixed4 mixedHazeColor = _HazeColor1 * colorNoise1 + 
                                       _HazeColor2 * colorNoise2 + 
                                       _HazeColor3 * colorNoise3;
                mixedHazeColor = normalize(mixedHazeColor);
                
                fixed4 voidColor = _MainColor;
                fixed4 hazeColor = mixedHazeColor * _HazeIntensity * wispMask;
                
                fixed4 nebula = lerp(voidColor, voidColor + hazeColor, distanceFade);
                
                float2 starUV = viewDir.xz + viewDir.y * 0.2;
                float stars = starField(starUV, 1.0 - _StarDensity);
                
                fixed4 starColor = fixed4(1, 1, 1, 1) * stars;
                starColor.rgb *= lerp(fixed3(1, 1, 1), mixedHazeColor.rgb, 0.3);
                
                fixed4 finalColor = nebula + starColor;
                
                return finalColor;
            }
            ENDCG
        }
    }
}