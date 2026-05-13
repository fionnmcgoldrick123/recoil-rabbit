Shader "Unlit/PaletteSwapper"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // --- Base palette (original colours in the sprite) ---
        // Dark:      #210613  →  (0.129, 0.024, 0.075)
        // Highlight: #F63090  →  (0.965, 0.188, 0.565)
        // Light:     #FFFFF5  →  (1.000, 1.000, 0.961)
        _BaseColor1 ("Base Dark",      Color) = (0.129, 0.024, 0.075, 1)
        _BaseColor2 ("Base Highlight", Color) = (0.965, 0.188, 0.565, 1)
        _BaseColor3 ("Base Light",     Color) = (1.000, 1.000, 0.961, 1)

        // --- Replacement palette (set by PaletteSwapperManager at runtime) ---
        _RepColor1 ("Replacement Dark",      Color) = (0, 0, 0, 1)
        _RepColor2 ("Replacement Highlight", Color) = (0, 0, 0, 1)
        _RepColor3 ("Replacement Light",     Color) = (0, 0, 0, 1)

        // How closely a pixel must match a base colour to be swapped.
        // Increase if colours are not swapping; decrease if wrong pixels swap.
        _Tolerance ("Match Tolerance", Range(0.01, 0.25)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

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
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            fixed4 _BaseColor1;
            fixed4 _BaseColor2;
            fixed4 _BaseColor3;

            fixed4 _RepColor1;
            fixed4 _RepColor2;
            fixed4 _RepColor3;

            float _Tolerance;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                o.color  = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // Discard fully transparent pixels so the outline stays clean.
                clip(col.a - 0.01);

                // Check each base colour and swap to its replacement.
                if      (distance(col.rgb, _BaseColor1.rgb) < _Tolerance)
                    col.rgb = _RepColor1.rgb;
                else if (distance(col.rgb, _BaseColor2.rgb) < _Tolerance)
                    col.rgb = _RepColor2.rgb;
                else if (distance(col.rgb, _BaseColor3.rgb) < _Tolerance)
                    col.rgb = _RepColor3.rgb;

                return col;
            }
            ENDCG
        }
    }
}
