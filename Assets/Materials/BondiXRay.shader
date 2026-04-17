Shader "Custom/URP_BondiXRay_Full_Relief"
{
    Properties
    {
        [MainTexture] _BaseMap("Textura de Blender", 2D) = "white" {}
        _BaseColor("Tinte Normal", Color) = (1,1,1,1)
        
        [NoScaleOffset] _BumpMap("Normal Map (Relieve)", 2D) = "bump" {}
        _BumpScale("Intensidad Relieve", Range(0, 2)) = 1.0

        _XRayColor("Color Silueta (Detras)", Color) = (0,1,0,0.5)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        // --- PASADA 1: LA SILUETA (RAYOS X) ---
        Pass
        {
            Name "XRayPass"
            ZTest Greater      
            ZWrite Off         
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            float4 _XRayColor;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target {
                return _XRayColor;
            }
            ENDHLSL
        }

        // --- PASADA 2: DIBUJO NORMAL CON RELIEVE ---
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            ZTest LEqual       
            ZWrite On          

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Importamos librerías de iluminación de URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD3;
                float4 tangentWS  : TEXCOORD4; // El w guarda el signo del bitangente
            };

            float4 _BaseColor;
            float4 _BaseMap_ST;
            float _BumpScale;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                
                // Transformamos Normal y Tangente al Espacio de Mundo (WS)
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target {
                // 1. Muestreo de la textura base
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // 2. Cálculo del Normal Map (Relieve)
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);
                
                // 3. Crear matriz TBN para pasar de Tangent Space a World Space
                float3 sgn = IN.tangentWS.w * GetOddNegativeScale();
                float3 bitangentWS = cross(IN.normalWS, IN.tangentWS.xyz) * sgn;
                half3x3 tangentToWorld = half3x3(IN.tangentWS.xyz, bitangentWS, IN.normalWS);
                
                float3 normalWS = normalize(mul(normalTS, tangentToWorld));

                // 4. Iluminación básica (Lambert) para que se note el relieve
                Light mainLight = GetMainLight();
                float3 lightColor = mainLight.color * saturate(dot(normalWS, mainLight.direction));
                
                // Resultado final: Color * Luz + un poco de luz ambiental
                float3 finalRGB = texColor.rgb * (lightColor + 0.2); 
                
                return float4(finalRGB, texColor.a);
            }
            ENDHLSL
        }
    }
}