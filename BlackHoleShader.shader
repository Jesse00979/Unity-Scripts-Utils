Shader "Custom/BlackHoleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        Decain ("Decain", Float) = 8
        Speed ("Speed", Range(0, 100)) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Inclui as definições para renderização no URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float Decain;
            float Speed;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float getangle(float2 uv, float raio, float pi)
            {
                float ang = asin((uv.y - 0.5f) / raio);
                if(uv.x-0.5f<0.0 && uv.y-0.5f>=0.0)
                    ang = (pi/2.0-ang)+(pi/2.0);
                else if(uv.x-0.5f<0.0 && uv.y-0.5f<0.0)
                    ang = (pi/2.0) - (ang-pi/2.0);
                
                return ang;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                const float pi = 3.1415926536f;
                const float hip = 0.7071067811865f;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float raio = sqrt(pow(IN.uv.x - 0.5f, 2) + pow(IN.uv.y - 0.5f, 2));
                
                if(raio<=0.1){
                    col.rgb = 0;
                }else if(raio<=0.46415888f){
                    float ang = getangle(IN.uv, raio, pi);
                    float d = (hip - Decain * pow(raio, 3));
                    ang += (pi*4)*d + _Time*Speed;

                    float2 pos = float2(cos(ang)*raio+0.5f, sin(ang)*raio+0.5f);
                    col.rgb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pos).rgb;

                    if(abs(pos.x - 0.5f) + abs(4*(pos.y - 0.5f)) <= 0.3f){
                        col.rgb = 0;
                    }
                }else{
                    col.rgba = float4(0, 0, 0, 0);
                    // Aplica corte de transparência (Alpha Clipping)
                }

                clip(col.a-1);

                return col;
            }
            ENDHLSL
        }

        // Passagem para projeção de sombras
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float Decain;
            float Speed;

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformWorldToHClip(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.uv = IN.uv;
                return OUT;
            }

            float getangle(float2 uv, float raio, float pi)
            {
                float ang = asin((uv.y - 0.5f) / raio);
                if(uv.x-0.5f<0.0 && uv.y-0.5f>=0.0)
                    ang = (pi/2.0-ang)+(pi/2.0);
                else if(uv.x-0.5f<0.0 && uv.y-0.5f<0.0)
                    ang = (pi/2.0) - (ang-pi/2.0);
                
                return ang;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_Target
            {
                const float pi = 3.1415926536f;
                const float hip = 0.7071067811865f;

                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;
                float raio = sqrt(pow(IN.uv.x - 0.5f, 2) + pow(IN.uv.y - 0.5f, 2));
                
                alpha = 0;
                if(raio<=0.1){
                    alpha = 1;
                }else if(raio<=0.46415888f){
                    float ang = getangle(IN.uv, raio, pi);
                    float d = (hip - Decain * pow(raio, 3));
                    ang += (pi*4)*d + _Time*Speed;

                    float2 pos = float2(cos(ang)*raio+0.5f, sin(ang)*raio+0.5f);

                    if(abs(pos.x - 0.5f) + abs(4*(pos.y - 0.5f)) <= 0.3f){
                        alpha = 1;
                    }
                }

                // Aplica Alpha Clipping na sombra
                clip(alpha-1);
                
                return 0;
            }
            ENDHLSL
        }
    }
}