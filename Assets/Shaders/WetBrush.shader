Shader "Hidden/WetBrush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
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
            float4 _UV; // x,y = position, z = brush size

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

            // Simpel pseudo-random noise
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(127.1, 311.7))) * 43758.5453);
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 existing = tex2D(_MainTex, i.uv);
                float dist = distance(i.uv, _UV.xy);

                // Blød brush falloff fra center
                float brush = saturate(1.0 - dist / _UV.z);

                // Noise baseret på UV position
                float noise = rand(i.uv * 1000.0);

                // Kanten er speckled, midten er solid
                float speckle = saturate(brush * 1.5 - noise * (1.0 - brush) * 1.5);

                float mask = max(existing.r, speckle);
                return float4(mask, mask, mask, 1);
            }
            ENDCG
        }
    }
}