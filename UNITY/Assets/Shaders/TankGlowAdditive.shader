Shader "WarOfTanks/TankGlowAdditive"
{
    // Additive, unlit sprite glow. WebGL-safe (Built-in pipeline, basic CG, no HDR).
    // Reads the SpriteRenderer's vertex color as the glow tint and the texture's
    // alpha as the glow intensity falloff.
    Properties
    {
        _MainTex ("Glow Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,4)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One One   // additive

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed a = tex2D(_MainTex, i.uv).a;        // intensity falloff
                fixed3 rgb = i.color.rgb * (a * i.color.a * _Intensity);
                return fixed4(rgb, 1);                     // alpha unused (additive)
            }
            ENDCG
        }
    }
    Fallback Off
}
