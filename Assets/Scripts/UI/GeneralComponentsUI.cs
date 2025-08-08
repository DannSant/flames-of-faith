using Game.Common;
using UnityEngine;

namespace Game.UI
{
    public class GeneralComponentsUI : Singleton<GeneralComponentsUI>
    {
        [SerializeField]
        private GeneralTooltipPaneUI generalTooltipPaneUI;

        public GeneralTooltipPaneUI GeneralTooltipPaneUI => generalTooltipPaneUI;

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
