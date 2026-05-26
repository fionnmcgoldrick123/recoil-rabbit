// Renders a sprite as a flat solid colour, using the sprite texture only for its alpha mask.
// Forces point (nearest-neighbour) sampling so pixel-art sprites keep crisp, hard pixel edges.
// Set SpriteRenderer.color to control the tint and alpha.
Shader "Custom/SpriteSilhouette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS  : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 posCS  : SV_POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            // Unity auto-creates this sampler state from the name:
            // "point" = nearest-neighbour filter, "clamp" = clamp wrap.
            // Overrides the texture asset's own filter setting -- guarantees hard pixel edges.
            SamplerState sampler_point_clamp;
            float4 _Color;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                OUT.uv    = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Point-sampled alpha only -- crisp pixel edges, no RGB from the sprite
                float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp, IN.uv).a;
                return float4(IN.color.rgb, alpha * IN.color.a);
            }
            ENDHLSL
        }
    }
}