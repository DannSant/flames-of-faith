using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// Small helpers for the UI revamp tools that edit existing prefabs: find-or-create children by name
    /// (so every tool can be run again), anchor/stretch rects, and add the standard style components.
    /// </summary>
    public static class UIBuildHelpers
    {
        public const int UILayer = 5;

        public static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        public static RectTransform Find(Transform parent, string name)
        {
            var child = parent.Find(name);
            return child as RectTransform;
        }

        /// <summary>The child with this name, created (with a RectTransform on the UI layer) if missing.</summary>
        public static RectTransform FindOrCreate(Transform parent, string name)
        {
            var existing = Find(parent, name);
            if (existing != null) return existing;

            var go = new GameObject(name, typeof(RectTransform));
            go.layer = UILayer;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Image FindOrCreateImage(Transform parent, string name)
        {
            var rect = FindOrCreate(parent, name);
            GetOrAdd<CanvasRenderer>(rect.gameObject);
            return GetOrAdd<Image>(rect.gameObject);
        }

        public static TextMeshProUGUI FindOrCreateText(Transform parent, string name)
        {
            var rect = FindOrCreate(parent, name);
            GetOrAdd<CanvasRenderer>(rect.gameObject);
            var text = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
            text.raycastTarget = false;
            return text;
        }

        public static void DestroyChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        /// <summary>Anchors a rect at one point with a pivot, offset and fixed size.</summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        /// <summary>Stretches a rect between two anchors, inset by left/bottom/right/top.</summary>
        public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        public static void StretchFull(RectTransform rect, float inset = 0f)
        {
            Stretch(rect, Vector2.zero, Vector2.one, inset, inset, inset, inset);
        }

        public static UIStyleText StyleText(TMP_Text text, UIFontSize size, bool outlined, UIColorRole color, UISettings settings, bool bold = false)
        {
            var style = GetOrAdd<UIStyleText>(text.gameObject);
            style.Configure(size, outlined, color, bold);
            style.Apply(settings);
            return style;
        }

        public static UIStyleIcon StyleIcon(GameObject go, UIIconSize size, UISettings settings)
        {
            var style = GetOrAdd<UIStyleIcon>(go);
            style.Configure(size);
            style.Apply(settings);
            return style;
        }

        public static UIStyleFrame StyleFrame(Image image, UIFrameKind kind, UISettings settings)
        {
            var style = GetOrAdd<UIStyleFrame>(image.gameObject);
            style.Configure(kind);
            style.Apply(settings);
            return style;
        }

        /// <summary>
        /// Gives a scene prefab instance its prefab's root layout (anchors, pivot, size, position).
        /// Reverting can't do it: Unity keeps an instance root's layout as "default overrides" that a revert
        /// leaves in place, so a restyled prefab would keep showing at its old spot in the scene.
        /// </summary>
        public static void MatchPrefabRootLayout(GameObject instance)
        {
            var target = instance.transform as RectTransform;
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instance)?.transform as RectTransform;
            if (target == null || source == null) return;

            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.sizeDelta = source.sizeDelta;
            target.anchoredPosition = source.anchoredPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
            // Recorded as instance overrides right away: only marking it dirty let a later revert on the same
            // instance (in the same pass) reload it and drop these values
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            EditorUtility.SetDirty(target);
        }

        public static readonly string[] LayoutProperties = { "m_AnchorMin", "m_AnchorMax", "m_AnchoredPosition", "m_SizeDelta", "m_Pivot" };

        /// <summary>
        /// Reverts the layout overrides (anchors, position, size, pivot) a scene holds on the children of this
        /// prefab's instances. A removed layout group leaves its children's layout as scene overrides (one pinned
        /// the wave results title to the bottom); a restyler owns that layout, so only those properties are
        /// reverted, never the whole instance, and never the instance root (snap it after this).
        /// </summary>
        public static bool RevertChildLayoutOverrides(UnityEngine.SceneManagement.Scene scene, string prefabPath)
        {
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;
                    if (AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject)) != prefabPath) continue;

                    foreach (var rect in transform.GetComponentsInChildren<RectTransform>(true))
                    {
                        if (rect == transform) continue;
                        var so = new SerializedObject(rect);
                        foreach (string property in LayoutProperties)
                        {
                            var p = so.FindProperty(property);
                            if (p == null || !p.prefabOverride) continue;
                            PrefabUtility.RevertPropertyOverride(p, InteractionMode.AutomatedAction);
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
