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

        // --- Settings ----------------------------------------------------------------------

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
        }
    }
}
