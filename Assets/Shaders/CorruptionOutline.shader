// Draws only a ring around a sprite's opaque pixels (the outline), unlit, with a slow pulse.
// Used by SpriteOutlineRenderer on a child SpriteRenderer that copies the real sprite, so the real
// sprite keeps its own (2D lit) material. Values come per renderer from a MaterialPropertyBlock.
// The ring is drawn outside the sprite's silhouette, so it needs that much empty space in the sprite
// mesh: 1 texel fits the default "Extrude Edges" of 1 on Tight meshes.
Shader "Game/Sprites/CorruptionOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.72, 0.3, 1, 1)
        _OutlineThickness ("Thickness (texels)", Range(0.5, 4)) = 1
        _OutlineTexelSize ("Texel Size (set by script)", Vector) = (0.01, 0.01, 0, 0)
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
        _PulseSpeed ("Pulse Speed", Float) = 2.5
        _PulseMinAlpha ("Pulse Min Alpha", Range(0, 1)) = 0.55
        _PulsePhase ("Pulse Phase", Float) = 0
        _SourceAlpha ("Source Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float3 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _OutlineColor;
            float _OutlineThickness;
            float4 _OutlineTexelSize;
            float _AlphaThreshold;
            float _PulseSpeed;
            float _PulseMinAlpha;
            float _PulsePhase;
            float _SourceAlpha;
        CBUFFER_END

        Varyings OutlineVert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS);
            output.uv = input.uv;
            return output;
        }

        half SampleAlpha(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
        }

        half4 OutlineFrag(Varyings input) : SV_Target
        {
            // Inside the silhouette: nothing, the real sprite is drawn there.
            clip(_AlphaThreshold - SampleAlpha(input.uv));

            float2 d = _OutlineTexelSize.xy * _OutlineThickness;
            half n = SampleAlpha(input.uv + float2(d.x, 0));
            n = max(n, SampleAlpha(input.uv - float2(d.x, 0)));
            n = max(n, SampleAlpha(input.uv + float2(0, d.y)));
            n = max(n, SampleAlpha(input.uv - float2(0, d.y)));
            n = max(n, SampleAlpha(input.uv + d));
            n = max(n, SampleAlpha(input.uv - d));
            n = max(n, SampleAlpha(input.uv + float2(d.x, -d.y)));
            n = max(n, SampleAlpha(input.uv + float2(-d.x, d.y)));

            // Outside and not next to the silhouette: nothing either.
            clip(n - _AlphaThreshold);

            half pulse = lerp(_PulseMinAlpha, 1.0h, 0.5h + 0.5h * sin(_Time.y * _PulseSpeed + _PulsePhase));
            return half4(_OutlineColor.rgb, _OutlineColor.a * pulse * _SourceAlpha);
        }
        ENDHLSL

        // 2D Renderer
        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            ENDHLSL
        }

        // Universal (3D) Renderer, so the material also previews and works outside the 2D Renderer
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            ENDHLSL
        }
    }
}
