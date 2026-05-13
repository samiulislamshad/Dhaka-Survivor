Shader "Custom/ScrollingSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Scrolling Settings)]
        _ScrollSpeedX ("Scroll Speed X", Float) = 1.0
        _ScrollSpeedY ("Scroll Speed Y", Float) = 0.0
        _SpeedMultiplier ("Speed Multiplier", Float) = 1.0
        _ScrollDirection ("Scroll Direction", Vector) = (-1,0,0,0)
        
        [Header(Sprite Settings)]
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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
            fixed _EnableExternalAlpha;
            fixed4 _Color;
            float _ScrollSpeedX;
            float _ScrollSpeedY;
            float _SpeedMultiplier;
            float4 _ScrollDirection;

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
                // Calculate scroll using multiple speed controls
                float2 scrolledUV = IN.texcoord;
                
                // Method 1: Using individual X/Y speeds with multiplier
                float finalSpeedX = _ScrollSpeedX * _SpeedMultiplier;
                float finalSpeedY = _ScrollSpeedY * _SpeedMultiplier;
                
                // Method 2: Alternatively use direction vector (normalized)
                float2 directionScroll = normalize(_ScrollDirection.xy) * length(float2(_ScrollSpeedX, _ScrollSpeedY)) * _SpeedMultiplier;
                
                // Use the individual speeds (you can switch to directionScroll if preferred)
                scrolledUV.x += _Time.y * finalSpeedX;
                scrolledUV.y += _Time.y * finalSpeedY;
                
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