Shader "Custom/URP_BondiXRay_Final"
{
    Properties
    {
        _MainColor ("Color Normal", Color) = (1,1,1,1)
        _XRayColor ("Color Silueta (Detras)", Color) = (0,1,0,1)
    }

    SubShader
    {
        // Ponemos la cola en Transparent para asegurarnos de que se dibuje DESPUÉS de los edificios
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        // --- PASADA 1: LA SILUETA (RAYOS X) ---
        // Se dibuja solo si hay algo adelante
        Pass
        {
            Name "XRayPass"
            Tags { "LightMode" = "SRPDefaultUnlit" } // Pasada extra que URP reconoce

            ZTest Greater      // Solo si está tapado
            ZWrite Off         // No bloquea a otros objetos
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

        // --- PASADA 2: EL DIBUJO NORMAL ---
        // Se dibuja normalmente cuando está a la vista
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            ZTest LEqual       // Comportamiento normal (frente o igual)
            ZWrite On          // Escribe profundidad

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            float4 _MainColor;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target {
                return _MainColor;
            }
            ENDHLSL
        }
    }
}