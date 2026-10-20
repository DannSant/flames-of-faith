using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Style
{
    /// <summary>
    /// Gives a text or image one of the shared UISettings colors and nothing else (size and font are left
    /// alone). For elements in windows that haven't been restyled yet; restyled texts use UIStyleText.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class UIStyleColor : UIStyleBase
    {
        [SerializeField] private UIColorRole color = UIColorRole.Text;

        public UIColorRole ColorRole => color;

        public void Configure(UIColorRole color)
        {
            this.color = color;
        }

        public override bool Apply(UISettings settings)
        {
            var graphic = GetComponent<Graphic>();
            if (graphic == null || settings == null || color == UIColorRole.None) return false;

            Color target = settings.GetColor(color);
            target.a = graphic.color.a;
            if (graphic.color == target) return false;
            graphic.color = target;
            return true;
        }
    }
}
