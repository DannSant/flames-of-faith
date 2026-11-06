using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    /// <summary>Look of a <see cref="SpriteOutlineRenderer"/>; the numbers come from a settings SO.</summary>
    [Serializable]
    public struct SpriteOutlineStyle
    {
        public Color color;
        [Tooltip("Ring width in sprite texels (art pixels). Keep it at 1 unless the sprites have more empty space around them.")]
        public float thickness;
        [Tooltip("Pulses per second, in radians (0 = steady).")]
        public float pulseSpeed;
        [Tooltip("Lowest alpha of the pulse (1 = no pulse).")]
        [Range(0f, 1f)] public float pulseMinAlpha;
        [Tooltip("Added to the source's sorting order. Positive draws the ring over neighbors, negative under them.")]
        public int sortingOrderOffset;
        [Tooltip("Texels at or above this alpha count as the body; the ring goes around them. Keep it above the " +
            "baked ground shadows and soft halos (drawn at up to half alpha), or the ring wraps them too and " +
            "stops matching the body (the Dreko Eye, Nov 2026). 0 uses the material's value.")]
        [Range(0f, 1f)] public float alphaThreshold;
    }

    /// <summary>
    /// A child SpriteRenderer that copies a source SpriteRenderer (sprite, flip, sorting, visibility,
    /// alpha) and draws it with an outline-only material, so the source keeps its own material and lighting.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpriteOutlineRenderer : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int ThicknessId = Shader.PropertyToID("_OutlineThickness");
        private static readonly int TexelSizeId = Shader.PropertyToID("_OutlineTexelSize");
        private static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");
        private static readonly int PulseMinAlphaId = Shader.PropertyToID("_PulseMinAlpha");
        private static readonly int PulsePhaseId = Shader.PropertyToID("_PulsePhase");
        private static readonly int SourceAlphaId = Shader.PropertyToID("_SourceAlpha");
        private static readonly int AlphaThresholdId = Shader.PropertyToID("_AlphaThreshold");

        private SpriteRenderer source;
        private SpriteRenderer outline;
        private SpriteOutlineStyle style;
        private MaterialPropertyBlock block;
        private float pulsePhase;

        // Last values pushed to the property block, so it's only rebuilt on a change.
        private Sprite lastSprite;
        private float lastSourceAlpha = -1f;

        public SpriteRenderer Source => source;
        public SpriteRenderer Outline => outline;

        /// <summary>Adds an outline child under <paramref name="source"/>, already in sync.</summary>
        public static SpriteOutlineRenderer Create(SpriteRenderer source, Material material, SpriteOutlineStyle style)
        {
            var go = new GameObject("CorruptionOutline");
            go.layer = source.gameObject.layer;
            go.transform.SetParent(source.transform, false);

            var outlineRenderer = go.AddComponent<SpriteOutlineRenderer>();
            outlineRenderer.source = source;
            outlineRenderer.outline = go.AddComponent<SpriteRenderer>();
            outlineRenderer.outline.sharedMaterial = material;
            // Desynchronize the pulse so a crowd doesn't blink in unison.
            outlineRenderer.pulsePhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            outlineRenderer.SetStyle(style);
            return outlineRenderer;
        }

        public void SetStyle(SpriteOutlineStyle newStyle)
        {
            style = newStyle;
            lastSprite = null; // forces the property block to be rebuilt
            Sync();
        }

        private void LateUpdate()
        {
            Sync();
        }

        /// <summary>Copies the source's state. Runs every LateUpdate; public for tests.</summary>
        public void Sync()
        {
            if (source == null || outline == null)
            {
                return;
            }

            bool visible = source.enabled && source.gameObject.activeInHierarchy && source.sprite != null;
            outline.enabled = visible;
            if (!visible)
            {
                return;
            }

            outline.sprite = source.sprite;
            outline.sortingLayerID = source.sortingLayerID;
            outline.sortingOrder = source.sortingOrder + style.sortingOrderOffset;
            outline.flipX = false;
            outline.flipY = false;
            // The outline material doesn't read the SpriteRenderer flip, so mirror through the transform.
            transform.localScale = new Vector3(source.flipX ? -1f : 1f, source.flipY ? -1f : 1f, 1f);

            float sourceAlpha = source.color.a;
            if (source.sprite != lastSprite || !Mathf.Approximately(sourceAlpha, lastSourceAlpha))
            {
                PushProperties(sourceAlpha);
            }
        }

        private void PushProperties(float sourceAlpha)
        {
            block ??= new MaterialPropertyBlock();
            outline.GetPropertyBlock(block);

            Texture texture = source.sprite.texture;
            Vector4 texel = texture != null ? new Vector4(1f / texture.width, 1f / texture.height, 0f, 0f) : new Vector4(0.01f, 0.01f, 0f, 0f);
            block.SetColor(ColorId, style.color);
            block.SetFloat(ThicknessId, style.thickness);
            block.SetVector(TexelSizeId, texel);
            block.SetFloat(PulseSpeedId, style.pulseSpeed);
            block.SetFloat(PulseMinAlphaId, style.pulseMinAlpha);
            block.SetFloat(PulsePhaseId, pulsePhase);
            block.SetFloat(SourceAlphaId, sourceAlpha);
            if (style.alphaThreshold > 0f)
            {
                block.SetFloat(AlphaThresholdId, style.alphaThreshold);
            }
            outline.SetPropertyBlock(block);

            lastSprite = source.sprite;
            lastSourceAlpha = sourceAlpha;
        }

        /// <summary>
        /// The shader's rule, for tests: a texel is on the ring when it isn't body (alpha at or under the threshold)
        /// but one of its 8 neighbors at <c>thickness</c> texels is.
        /// </summary>
        public static bool IsRingTexel(float alpha, float maxNeighborAlpha, float threshold)
        {
            return alpha <= threshold && maxNeighborAlpha >= threshold;
        }

        /// <summary>
        /// Which of a character's SpriteRenderers is its body: one on the root, else the one owned by a
        /// character visual, else the highest sorting order (the first one on a tie). -1 when empty.
        /// </summary>
        public static int SelectMainRenderer(IReadOnlyList<(bool onRoot, bool isCharacterVisual, int sortingOrder)> candidates)
        {
            if (candidates == null || candidates.Count == 0) return -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].onRoot) return i;
            }
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].isCharacterVisual) return i;
            }
            int best = 0;
            for (int i = 1; i < candidates.Count; i++)
            {
                if (candidates[i].sortingOrder > candidates[best].sortingOrder) best = i;
            }
            return best;
        }
    }
}
