Shader "Hidden/Brush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WetMask ("Wet Mask", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _WetMask;
            float4 _UV;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 existing = tex2D(_MainTex, i.uv);
                float dist = distance(i.uv, _UV.xy);
                float brush = saturate(1.0 - dist / _UV.z);

                // Læs hvor vådt dette område er
                float wetness = tex2D(_WetMask, i.uv).r;

                // Vådt område gør børsten mindre effektiv
                // 0.3 = coating reducerer børsteeffekt med 70% på våde områder
                float resistance = 1.0 - wetness * 0.7;
                float effectiveBrush = brush * resistance;

                float mask = min(existing.r, 1.0 - effectiveBrush);
                return float4(mask, mask, mask, 1);
            }
            ENDCG
        }
    }
}