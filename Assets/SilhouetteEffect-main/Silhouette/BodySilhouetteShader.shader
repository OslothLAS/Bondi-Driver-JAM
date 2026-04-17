Shader "Custom/BodySilhouette"
{
    Properties
    {
        _Color ("Silhouette Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.00001, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        // Pass 1: Write Depth & Stencil
        Pass
        {
            Name "DepthStencilPass"
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

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
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }

        // Pass 2: Silhouette
        Pass
        {
            Name "Silhouette"
            Cull Front
            ZTest Greater
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil {
    Ref 1
    Comp NotEqual
    Pass Keep
    ZFail Keep
}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            fixed4 _Color;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz += normalize(v.normal) * _OutlineThickness;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
    FallBack Off
}