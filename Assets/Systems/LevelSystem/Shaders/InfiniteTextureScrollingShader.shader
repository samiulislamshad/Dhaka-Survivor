Shader "Custom/ScrollingSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ScrollSpeedX ("Scroll Speed X", Float) = 1.0
        _ScrollSpeedY ("Scroll Speed Y", Float) = 0.0
        
        [Header(Sprite Settings)]
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            fixed4 _Color;
            float _ScrollSpeedX;
            float _ScrollSpeedY;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Scroll the UV coordinates over time
                float2 scrolledUV = IN.texcoord;
                scrolledUV.x += _Time.y * _ScrollSpeedX;
                scrolledUV.y += _Time.y * _ScrollSpeedY;
                
                // Wrap UVs to create seamless loop
                scrolledUV = frac(scrolledUV);
                
                fixed4 c = tex2D(_MainTex, scrolledUV) * IN.color;
                
                #ifdef ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, scrolledUV);
                c.a = lerp(c.a, alpha.r, _EnableExternalAlpha);
                #endif

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}