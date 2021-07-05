Shader "SpaceTrader/PixelizationEffect" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _ScaleX ("Scale X", Float) = 2
        _ScaleY ("Scale Y", Float) = 2

        [Toggle(MULTISAMPLE)] _Multisample ("Multisample", Float) = 1
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local __ MULTISAMPLE_ON

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _ScaleX;
            float _ScaleY;

            struct vertex_input {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct fragment_input {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fragment_input vert(vertex_input input) {
                fragment_input output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(fragment_input input) : SV_Target {
                float2 pixel_size = 1 / float2(_ScreenParams.x, _ScreenParams.y);
                float2 block_size = pixel_size * float2(_ScaleX, _ScaleY);

                float x = floor(input.uv.x / block_size.x) * block_size.x;
                float y = floor(input.uv.y / block_size.y) * block_size.y;
                float2 position = float2(x, y);

                fixed4 avg_color = tex2D(_MainTex, position + block_size / 2);

#ifdef MULTISAMPLE_ON
                avg_color += tex2D(_MainTex, position + float2(block_size.x / 4, block_size.y / 4));
                avg_color += tex2D(_MainTex, position + float2(block_size.x / 2, block_size.y / 4));
                avg_color += tex2D(_MainTex, position + float2((block_size.x / 4) * 3, block_size.y / 4));
                avg_color += tex2D(_MainTex, position + float2(block_size.x / 4, block_size.y / 2));
                avg_color += tex2D(_MainTex, position + float2((block_size.x / 4) * 3, block_size.y / 2));
                avg_color += tex2D(_MainTex, position + float2(block_size.x / 4, (block_size.y / 4) * 3));
                avg_color += tex2D(_MainTex, position + float2(block_size.x / 2, (block_size.y / 4) * 3));
                avg_color += tex2D(_MainTex, position + float2((block_size.x / 4) * 3, (block_size.y / 4) * 3));
                avg_color /= 9;
#endif

                return avg_color;
            }
            ENDCG
        }
    }
}
