using Game.Combat;
using Game.Common;
using Game.Control;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The Corrupted outline: which sprite gets it, the outline copying its source, and the settings
    /// (Outline mode on, material with a shader that compiles).
    /// </summary>
    public class CorruptionOutlineTests
    {
        private const string ShaderName = "Game/Sprites/CorruptionOutline";
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created)
            {
                if (o != null) Object.DestroyImmediate(o);
            }
            created.Clear();
        }

        private T Track<T>(T o) where T : Object
        {
            created.Add(o);
            return o;
        }

        private Sprite MakeSprite()
        {
            var texture = Track(new Texture2D(16, 8));
            return Track(Sprite.Create(texture, new Rect(0, 0, 16, 8), new Vector2(0.5f, 0f), 16));
        }

        private SpriteRenderer MakeRenderer(string name, Transform parent = null, int order = 0)
        {
            var go = parent == null ? Track(new GameObject(name)) : new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = MakeSprite();
            r.sortingOrder = order;
            return r;
        }

        private Material MakeMaterial() => Track(new Material(Shader.Find(ShaderName)));

        private static SpriteOutlineStyle Style(int offset = 1) => new SpriteOutlineStyle
        {
            color = new Color(0.7f, 0.3f, 1f, 1f), thickness = 1f, pulseSpeed = 2f, pulseMinAlpha = 0.5f, sortingOrderOffset = offset,
        };

        // --- Which sprite is the body ------------------------------------------------------

        [Test]
        public void SelectMainRenderer_RootThenCharacterVisualThenHighestOrder()
        {
            Assert.AreEqual(1, SpriteOutlineRenderer.SelectMainRenderer(new[] { (false, true, 50), (true, false, 0) }));
            Assert.AreEqual(2, SpriteOutlineRenderer.SelectMainRenderer(new[] { (false, false, 0), (false, false, 99), (false, true, 10) }));
            Assert.AreEqual(1, SpriteOutlineRenderer.SelectMainRenderer(new[] { (false, false, 0), (false, false, 10), (false, false, 10) }));
            Assert.AreEqual(-1, SpriteOutlineRenderer.SelectMainRenderer(new (bool, bool, int)[0]));
        }

        [Test]
        public void FindMainSpriteRenderer_PicksThePlayerBodyAndSkipsOutlines()
        {
            // Like the player: no sprite on the root, an aim marker child and the CharacterVisual body.
            var root = Track(new GameObject("Player")).transform;
            MakeRenderer("Triangle", root, order: 20);
            var body = MakeRenderer("CharacterVisual", root, order: 10);
            body.gameObject.AddComponent<CharacterVisual>();

            Assert.AreSame(body, CorruptedVisual.FindMainSpriteRenderer(root));

            // An outline under the body is never picked as the body itself.
            SpriteOutlineRenderer.Create(body, MakeMaterial(), Style());
            Assert.AreSame(body, CorruptedVisual.FindMainSpriteRenderer(root));
        }

        [Test]
        public void FindMainSpriteRenderer_PrefersTheRootSprite()
        {
            var enemy = MakeRenderer("Enemy");
            MakeRenderer("Shadow", enemy.transform, order: 50);
            Assert.AreSame(enemy, CorruptedVisual.FindMainSpriteRenderer(enemy.transform));
        }

        // --- The outline copies its source -------------------------------------------------

        [Test]
        public void Outline_CopiesSpriteSortingFlipVisibilityAndAlpha()
        {
            var source = MakeRenderer("Enemy", order: 3);
            var outline = SpriteOutlineRenderer.Create(source, MakeMaterial(), Style(offset: 1));

            Assert.AreSame(source.transform, outline.transform.parent);
            Assert.AreSame(source.sprite, outline.Outline.sprite);
            Assert.AreEqual(4, outline.Outline.sortingOrder, "Drawn one order above the source");
            Assert.IsTrue(outline.Outline.enabled);

            var next = MakeSprite();
            source.sprite = next;
            source.flipX = true;
            source.color = new Color(1f, 1f, 1f, 0.25f);
            outline.Sync();
            Assert.AreSame(next, outline.Outline.sprite);
            Assert.AreEqual(-1f, outline.transform.localScale.x, "flipX is mirrored through the scale");
            Assert.IsFalse(outline.Outline.flipX);

            var block = new MaterialPropertyBlock();
            outline.Outline.GetPropertyBlock(block);
            Assert.AreEqual(0.25f, block.GetFloat("_SourceAlpha"), 1e-4f, "The source's alpha fades the outline");
            Assert.AreEqual(1f / 16f, block.GetVector("_OutlineTexelSize").x, 1e-6f, "Texel size from the sprite texture");

            source.enabled = false;
            outline.Sync();
            Assert.IsFalse(outline.Outline.enabled, "Hidden with its source");
        }

        // --- The ring follows the body, not the baked shadow (Dreko Eye, Nov 2026) ---------

        /// <summary>Ring texels along one row of alphas, with the shader's rule and a 1 texel thickness.</summary>
        private static List<int> RingTexels(float[] row, float threshold)
        {
            var ring = new List<int>();
            for (int i = 0; i < row.Length; i++)
            {
                float left = i > 0 ? row[i - 1] : 0f;
                float right = i < row.Length - 1 ? row[i + 1] : 0f;
                if (SpriteOutlineRenderer.IsRingTexel(row[i], Mathf.Max(left, right), threshold)) ring.Add(i);
            }
            return ring;
        }

        [Test]
        public void Ring_WrapsTheBodyButNotItsHalfTransparentShadow()
        {
            // Empty, opaque body, a soft edge, a baked ground shadow at under half alpha, empty.
            var row = new[] { 0f, 1f, 1f, 1f, 0.7f, 0.4f, 0.4f, 0.4f, 0f, 0f };
            float threshold = LoadSettings().CorruptedOutlineStyle.alphaThreshold;

            CollectionAssert.AreEqual(new[] { 0, 5 }, RingTexels(row, threshold),
                "The ring sits right next to the body (and its soft edge) on both sides");
            CollectionAssert.Contains(RingTexels(row, 0.1f), 8,
                "The old threshold (0.1) counted the shadow as body, so the ring went around the shadow");
        }

        [Test]
        public void Outline_PushesTheStyleThreshold()
        {
            var source = MakeRenderer("Enemy");
            var style = Style();
            style.alphaThreshold = 0.6f;
            var outline = SpriteOutlineRenderer.Create(source, MakeMaterial(), style);

            var block = new MaterialPropertyBlock();
            outline.Outline.GetPropertyBlock(block);
            Assert.AreEqual(0.6f, block.GetFloat("_AlphaThreshold"), 1e-4f);
        }

        [Test]
        public void EveryEnemyPrefab_OutlinesItsAnimatedBody()
        {
            // Enemy bodies moved to sorting order 10 (Light Devourer, Nov 2026): the outline must still pick
            // the sprite the Animator drives, never the health bar, a marker or a VFX.
            int checkedCount = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies" }))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var animator = prefab.GetComponentInChildren<Animator>(true);
                if (animator == null || prefab.GetComponentInChildren<SpriteRenderer>(true) == null) continue;

                var body = CorruptedVisual.FindMainSpriteRenderer(prefab.transform);
                Assert.IsNotNull(body, prefab.name);
                Assert.AreSame(animator.gameObject, body.gameObject,
                    $"{prefab.name}: the outline copies {body.name}, not the animated body {animator.name}");
                checkedCount++;
            }
            Assert.Greater(checkedCount, 5, "Expected the enemy prefabs under Assets/Prefabs/Enemies");
        }

        // --- Settings ----------------------------------------------------------------------

        private static CorruptionSettings LoadSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CorruptionSettings>("Assets/Resources/Combat/CorruptionSettings.asset");
            Assert.IsNotNull(settings, "Missing CorruptionSettings.asset");
            return settings;
        }

        [Test]
        public void Settings_UseTheOutline()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CorruptionSettings>("Assets/Resources/Combat/CorruptionSettings.asset");
            Assert.IsNotNull(settings, "Missing CorruptionSettings.asset");
            Assert.AreEqual(CorruptedVisualMode.Outline, settings.CorruptedVisualMode);

            var material = settings.CorruptedOutlineMaterial;
            Assert.IsNotNull(material, "CorruptionSettings has no outline material");
            Assert.IsNotNull(material.shader, "The outline material has no shader");
            Assert.AreEqual(ShaderName, material.shader.name);
            Assert.IsFalse(ShaderUtil.ShaderHasError(material.shader), "The outline shader has compile errors");

            var style = settings.CorruptedOutlineStyle;
            Assert.Greater(style.color.b, style.color.g, "The outline should be purple");
            Assert.Greater(style.color.r, style.color.g, "The outline should be purple");
            Assert.Greater(style.thickness, 0f);
            Assert.Greater(style.color.a, 0f);
            Assert.That(style.alphaThreshold, Is.GreaterThan(0.5f).And.LessThan(0.9f),
                "Above the baked ground shadows (up to half alpha), below the body's own soft edge");
        }
    }
}
