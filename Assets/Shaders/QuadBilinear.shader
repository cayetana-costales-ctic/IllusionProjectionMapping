Shader "Custom/QuadBilinear"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _P0 ("P0", Vector) = (0,0,0,0)
        _P1 ("P1", Vector) = (1,0,0,0)
        _P2 ("P2", Vector) = (0,1,0,0)
        _P3 ("P3", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off ZWrite On ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _P0;
            float4 _P1;
            float4 _P2;
            float4 _P3;

            struct appv {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 viewPos : TEXCOORD0;
            };

            v2f vert (appv v)
            {
                v2f o;
                float4 clip = UnityObjectToClipPos(v.vertex);
                float2 viewport = clip.xy / clip.w * 0.5 + 0.5;
                o.viewPos = viewport;
                o.pos = clip;
                return o;
            }

            float Wedge2D(float2 a, float2 b)
            {
                return a.x * b.y - a.y * b.x;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p0 = _P0.xy;
                float2 p1 = _P1.xy;
                float2 p2 = _P2.xy;
                float2 p3 = _P3.xy;

                float2 q = i.viewPos - p0;
                float2 b1 = p1 - p0;
                float2 b2 = p2 - p0;
                float2 b3 = p0 - p1 - p2 + p3;

                float A = Wedge2D(b2, b3);
                float B = Wedge2D(b3, q) - Wedge2D(b1, b2);
                float C = Wedge2D(b1, q);

                float2 uv;
                float eps = 1e-6;

                if (abs(A) < eps)
                    uv.y = -C / B;
                else {
                    float discrim = B * B - 4.0 * A * C;
                    discrim = max(discrim, 0.0);
                    float sqrtD = sqrt(discrim);
                    uv.y = 0.5 * (-B + sqrtD) / A;
                }

                float2 denom = b1 + uv.y * b3;
                if (abs(denom.x) > abs(denom.y))
                    uv.x = (q.x - b2.x * uv.y) / denom.x;
                else
                    uv.x = (q.y - b2.y * uv.y) / denom.y;

                uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;

                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
