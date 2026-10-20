using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Style
{
    /// <summary>
    /// A grid of icons of the UI standard (inventory, shop...): cells of one icon size, the standard spacing.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    public class UIStyleGrid : UIStyleBase
    {
        [SerializeField] private UIIconSize cellSize = UIIconSize.Small;

        public UIIconSize CellSize => cellSize;

        public void Configure(UIIconSize cellSize)
        {
            this.cellSize = cellSize;
        }

        public override bool Apply(UISettings settings)
        {
            var grid = GetComponent<GridLayoutGroup>();
            if (grid == null || settings == null) return false;

            bool changed = false;
            float side = settings.GetIconSize(cellSize);
            var cell = new Vector2(side, side);
            if (grid.cellSize != cell) { grid.cellSize = cell; changed = true; }

            var spacing = new Vector2(settings.Spacing, settings.Spacing);
            if (grid.spacing != spacing) { grid.spacing = spacing; changed = true; }
            return changed;
        }

        /// <summary>Length of n cells in a row or column with the standard spacing between them.</summary>
        public static float Span(int cells, float cellSize, float spacing)
        {
            return cells <= 0 ? 0f : cells * cellSize + (cells - 1) * spacing;
        }
    }
}
