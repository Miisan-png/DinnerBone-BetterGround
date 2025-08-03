Shader "Custom/ShadowEnemy" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0.1, 0.05, 0.2, 0.8)
        _EdgeColor ("Edge Glow Color", Color) = (0.3, 0.1, 0.5, 1)
        
        // Dissolve/Void Effects
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.2
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        
        // Animation
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        _NoiseScale ("Noise Scale", Float) = 1.0
        _NoiseSpeed ("Noise Speed", Float) = 0.5
        
        // Transparency
        _Alpha ("Overall Alpha", Range(0, 1)) = 0.7
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
    }
    
    SubShader {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 normal : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            
            fixed4 _ShadowColor;
            fixed4 _EdgeColor;
            float _DissolveAmount;
            float _EdgeWidth;
            float _WaveSpeed;
            float _WaveAmplitude;
            float _NoiseScale;
            float _NoiseSpeed;
            float _Alpha;
            float _Cutoff;
            
            // Simple noise function
            float noise(float2 uv) {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // Vertex displacement for floating/wavy effect
                float time = _Time.y * _WaveSpeed;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Add wave displacement
                float wave = sin(worldPos.x * 2.0 + time) * sin(worldPos.z * 1.5 + time * 0.7);
                wave += sin(worldPos.y * 3.0 + time * 1.3) * 0.5;
                
                // Sample noise for organic movement
                float2 noiseUV = worldPos.xz * _NoiseScale + _Time.y * _NoiseSpeed;
                float noiseValue = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).r;
                
                // Apply displacement
                float3 displacement = v.normal * wave * _WaveAmplitude;
                displacement += v.normal * (noiseValue - 0.5) * _WaveAmplitude * 0.5;
                
                v.vertex.xyz += displacement;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // Base texture
                fixed4 mainTex = tex2D(_MainTex, i.uv);
                
                // Animated noise for dissolve effect
                float2 noiseUV = i.uv * _NoiseScale + _Time.y * _NoiseSpeed;
                float noise1 = tex2D(_NoiseTex, noiseUV).r;
                float noise2 = tex2D(_NoiseTex, noiseUV * 0.7 + float2(0.3, 0.8)).g;
                float combinedNoise = (noise1 + noise2) * 0.5;
                
                // Dissolve calculation
                float dissolve = combinedNoise + _DissolveAmount;
                clip(dissolve - _Cutoff);
                
                // Edge glow effect
                float edge = smoothstep(_Cutoff, _Cutoff + _EdgeWidth, dissolve);
                fixed4 edgeGlow = _EdgeColor * (1.0 - edge);
                
                // Fresnel effect for rim lighting
                float fresnel = 1.0 - saturate(dot(i.normal, i.viewDir));
                fresnel = pow(fresnel, 2.0);
                
                // Combine colors
                fixed4 shadowCol = _ShadowColor * mainTex;
                fixed4 finalColor = lerp(shadowCol, edgeGlow, edgeGlow.a);
                
                // Add fresnel rim
                finalColor.rgb += _EdgeColor.rgb * fresnel * 0.3;
                
                // Pulsing effect
                float pulse = sin(_Time.y * 3.0) * 0.1 + 0.9;
                finalColor.a *= _Alpha * pulse * edge;
                
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/Diffuse"
}