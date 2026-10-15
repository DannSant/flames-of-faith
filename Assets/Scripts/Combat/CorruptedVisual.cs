using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Shows the shared Corrupted VFX on top of its GameObject while the Corrupted state is on.
    /// The prefab comes from CorruptionSettings unless overridden here. Added at runtime when missing.
    /// </summary>
    public class CorruptedVisual : MonoBehaviour
    {
        [Tooltip("Optional. Leave empty to use the prefab from CorruptionSettings.")]
        [SerializeField] private GameObject vfxPrefabOverride;
        [Tooltip("Optional. Where the VFX is attached. Defaults to this transform.")]
        [SerializeField] private Transform anchor;

        private GameObject vfxInstance;

        public static CorruptedVisual GetOrAdd(GameObject target)
        {
            if (!target.TryGetComponent(out CorruptedVisual visual))
            {
                visual = target.AddComponent<CorruptedVisual>();
            }
            return visual;
        }

        /// <summary>
        /// Moves the VFX to another transform, now and for later instances. Used on the overworld map,
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
        }

        public void SetCorrupted(bool isCorrupted)
        {
            if (isCorrupted && vfxInstance == null)
            {
                var prefab = vfxPrefabOverride != null ? vfxPrefabOverride : CorruptionSettings.Instance.CorruptedVfxPrefab;
                if (prefab == null)
                {
                    return;
                }
                var parent = anchor != null ? anchor : transform;
                vfxInstance = Instantiate(prefab, parent.position, Quaternion.identity, parent);
            }

            if (vfxInstance != null)
            {
                vfxInstance.SetActive(isCorrupted);
            }
        }
    }
}
