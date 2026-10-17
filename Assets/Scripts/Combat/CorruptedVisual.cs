using Game.Common;
using Game.Control;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Marks its GameObject while the Corrupted state is on: a purple outline around its sprite, the
    /// shared Corrupted VFX on top, or both (CorruptionSettings.CorruptedVisualMode). The VFX prefab comes
    /// from CorruptionSettings unless overridden here. Added at runtime when missing.
    /// </summary>
    public class CorruptedVisual : MonoBehaviour
    {
        [Tooltip("Optional. Leave empty to use the prefab from CorruptionSettings.")]
        [SerializeField] private GameObject vfxPrefabOverride;
        [Tooltip("Optional. Where the VFX is attached and whose sprite gets the outline. Defaults to this transform.")]
        [SerializeField] private Transform anchor;
        [Tooltip("Optional. The sprite to outline. Found under the anchor when empty (see SpriteOutlineRenderer.SelectMainRenderer).")]
        [SerializeField] private SpriteRenderer outlineTarget;

        private GameObject vfxInstance;
        private SpriteOutlineRenderer outlineInstance;
        private bool isCorrupted;

        public static CorruptedVisual GetOrAdd(GameObject target)
        {
            if (!target.TryGetComponent(out CorruptedVisual visual))
            {
                visual = target.AddComponent<CorruptedVisual>();
            }
            return visual;
        }

        /// <summary>
        /// Moves the visual to another transform, now and for later instances. Used on the overworld map,
        /// where the player is hidden in place and a separate marker is what moves.
        /// </summary>
        public void SetAnchor(Transform newAnchor)
        {
            anchor = newAnchor;
            if (vfxInstance != null)
            {
                vfxInstance.transform.SetParent(anchor != null ? anchor : transform, false);
                vfxInstance.transform.localPosition = Vector3.zero;
            }

            // The outline belongs to a sprite: rebuild it on the new anchor's sprite.
            outlineTarget = null;
            if (outlineInstance != null)
            {
                Destroy(outlineInstance.gameObject);
                outlineInstance = null;
            }
            if (isCorrupted)
            {
                SetCorrupted(true);
            }
        }

        public void SetCorrupted(bool isCorrupted)
        {
            this.isCorrupted = isCorrupted;
            var settings = CorruptionSettings.Instance;
            var mode = settings != null ? settings.CorruptedVisualMode : CorruptedVisualMode.Vfx;

            SetVfx(isCorrupted && mode != CorruptedVisualMode.Outline);
            SetOutline(isCorrupted && mode != CorruptedVisualMode.Vfx, settings);
        }

        private void SetVfx(bool show)
        {
            if (show && vfxInstance == null)
            {
                var prefab = vfxPrefabOverride != null ? vfxPrefabOverride
                    : CorruptionSettings.Instance != null ? CorruptionSettings.Instance.CorruptedVfxPrefab : null;
                if (prefab == null)
                {
                    return;
                }
                var parent = anchor != null ? anchor : transform;
                vfxInstance = Instantiate(prefab, parent.position, Quaternion.identity, parent);
            }

            if (vfxInstance != null)
            {
                vfxInstance.SetActive(show);
            }
        }

        private void SetOutline(bool show, CorruptionSettings settings)
        {
            if (show && outlineInstance == null)
            {
                var material = settings != null ? settings.CorruptedOutlineMaterial : null;
                var target = outlineTarget != null ? outlineTarget : FindMainSpriteRenderer(anchor != null ? anchor : transform);
                if (material == null || target == null)
                {
                    return;
                }
                outlineTarget = target;
                outlineInstance = SpriteOutlineRenderer.Create(target, material, settings.CorruptedOutlineStyle);
            }

            if (outlineInstance != null)
            {
                outlineInstance.gameObject.SetActive(show);
            }
        }

        /// <summary>The body sprite under <paramref name="root"/> (outline renderers excluded).</summary>
        public static SpriteRenderer FindMainSpriteRenderer(Transform root)
        {
            var renderers = new List<SpriteRenderer>();
            foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (r.GetComponent<SpriteOutlineRenderer>() == null) renderers.Add(r);
            }

            var candidates = new List<(bool, bool, int)>(renderers.Count);
            foreach (var r in renderers)
            {
                candidates.Add((r.transform == root, r.GetComponent<CharacterVisual>() != null, r.sortingOrder));
            }

            int index = SpriteOutlineRenderer.SelectMainRenderer(candidates);
            return index >= 0 ? renderers[index] : null;
        }
    }
}
