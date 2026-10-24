using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Which child layout overrides the scene guard ignores: values the parent's layout group drives. A child whose
    /// layout group is gone (the wave results title) is not driven, so its overrides still fail the guard.
    /// </summary>
    public class SceneInstanceLayoutTests
    {
        private GameObject parent;
        private RectTransform child;

        [SetUp]
        public void SetUp()
        {
            parent = new GameObject("Parent", typeof(RectTransform));
            child = new GameObject("Child", typeof(RectTransform)).GetComponent<RectTransform>();
            child.SetParent(parent.transform, false);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(parent);

        [Test]
        public void WithoutALayoutGroup_NothingIsDriven()
        {
            Assert.IsFalse(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_AnchorMin", "y"));
            Assert.IsFalse(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_AnchoredPosition", "x"));
        }

        [Test]
        public void ALineGroup_DrivesAnchorsAndPosition_AndOnlyTheSizeItControls()
        {
            var column = parent.AddComponent<VerticalLayoutGroup>();
            column.childControlWidth = false;
            column.childControlHeight = true;

            Assert.IsTrue(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_AnchorMax", "x"));
            Assert.IsTrue(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_AnchoredPosition", "y"));
            Assert.IsFalse(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_SizeDelta", "x"));
            Assert.IsTrue(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_SizeDelta", "y"));
            Assert.IsFalse(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_Pivot", "x"));
        }

        [Test]
        public void AChildThatIgnoresLayout_IsNotDriven()
        {
            parent.AddComponent<GridLayoutGroup>();
            child.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            Assert.IsFalse(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_AnchorMin", "x"));
            Assert.IsFalse(SceneInstanceLayout.IsDrivenByLayoutGroup(child, "m_SizeDelta", "x"));
        }
    }
}
