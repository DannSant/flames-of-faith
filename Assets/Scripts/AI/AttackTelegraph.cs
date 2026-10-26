using UnityEngine;
using UnityEngine.Rendering;

namespace Game.AI
{
    /// <summary>
    /// A melee enemy's attack warning (tester feedback, 2026-10-26): a ring on the ground around the area the
    /// hit will land on, and a disc inside it that grows to fill it by the moment the hit lands. Created at
    /// runtime by AIMeleeAttacker as a child of the enemy; the look is on <see cref="AttackTelegraphSettings"/>.
    /// It sorts as a group just behind its enemy, so it's above the ground and under a player standing in it.
    /// </summary>
    public class AttackTelegraph : MonoBehaviour
    {
        private SpriteRenderer ring;
        private SpriteRenderer fill;
        private Transform area;
        private float duration;
        private float elapsed;
        private float diameter;
        private bool showing;

        public bool IsShowing => showing;

        /// <summary>Builds the (hidden) warning under <paramref name="enemy"/>.</summary>
        public static AttackTelegraph Create(Transform enemy, AttackTelegraphSettings settings)
        {
            var root = new GameObject("AttackTelegraph");
            root.transform.SetParent(enemy, false);
            root.transform.localPosition = new Vector3(0f, settings.SortBehindEnemy, 0f);

            var group = root.AddComponent<SortingGroup>();
            group.sortingOrder = settings.SortingOrder;

            var telegraph = root.AddComponent<AttackTelegraph>();
            telegraph.area = new GameObject("Area").transform;
            telegraph.area.SetParent(root.transform, false);
            telegraph.ring = CreateLayer(telegraph.area, "Ring", settings.RingSprite, settings.RingColor, 1);
            telegraph.fill = CreateLayer(telegraph.area, "Fill", settings.FillSprite, settings.FillColor, 0);
            root.SetActive(false);
            return telegraph;
        }

        private static SpriteRenderer CreateLayer(Transform parent, string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>
        /// Shows the warning for an area of <paramref name="radius"/> centered at <paramref name="worldCenter"/>,
        /// filling over <paramref name="windup"/> seconds.
        /// </summary>
        public void Show(Vector2 worldCenter, float radius, float windup)
        {
            gameObject.SetActive(true);
            area.position = new Vector3(worldCenter.x, worldCenter.y, area.position.z);
            diameter = radius * 2f;
            ring.transform.localScale = ScaleFor(ring.sprite, diameter);
            duration = windup;
            elapsed = 0f;
            showing = true;
            ApplyFill();
        }

        public void Hide()
        {
            showing = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!showing) return;
            elapsed += Time.deltaTime;
            ApplyFill();
        }

        private void ApplyFill() => fill.transform.localScale = ScaleFor(fill.sprite, diameter * FillFraction(elapsed, duration));

        /// <summary>How much of the area the growing disc covers: 0 when the windup starts, 1 when the hit lands.</summary>
        public static float FillFraction(float elapsed, float windup) => windup <= 0f ? 1f : Mathf.Clamp01(elapsed / windup);

        // Scales a sprite of any size and pixels-per-unit to the wanted width in world units
        private static Vector3 ScaleFor(Sprite sprite, float worldWidth)
        {
            float spriteWidth = sprite != null ? sprite.bounds.size.x : 1f;
            float scale = spriteWidth > 0f ? worldWidth / spriteWidth : worldWidth;
            return new Vector3(scale, scale, 1f);
        }
    }
}
