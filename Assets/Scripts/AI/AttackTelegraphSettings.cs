using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// How a melee enemy's attack warning (<see cref="AttackTelegraph"/>) looks, shared by every melee attacker.
    /// The timing is per enemy, on its AIMeleeAttacker (Telegraph Time). Loaded from Resources/AI/AttackTelegraphSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackTelegraphSettings", menuName = "AI/Attack Telegraph Settings")]
    public class AttackTelegraphSettings : ScriptableObject
    {
        private const string ResourcePath = "AI/AttackTelegraphSettings";

        [Tooltip("Master switch. Off = no warning is drawn (the slower windup still applies).")]
        [SerializeField] private bool showTelegraphs = true;

        [Header("Look (placeholders until the art team draws them)")]
        [Tooltip("Outline of the area that will be hit, shown for the whole windup.")]
        [SerializeField] private Sprite ringSprite;
        [Tooltip("Disc that grows from nothing to the full area by the moment the hit lands.")]
        [SerializeField] private Sprite fillSprite;
        [SerializeField] private Color ringColor = new Color(1f, 0.25f, 0.2f, 0.9f);
        [SerializeField] private Color fillColor = new Color(1f, 0.2f, 0.15f, 0.35f);

        [Header("Drawing order")]
        [Tooltip("Sorting order of the warning. 0 = with the characters: it sorts just behind its enemy (by Y), " +
            "so it's above the ground (order -1) and under a player standing in it.")]
        [SerializeField] private int sortingOrder = 0;
        [Tooltip("How far behind its enemy (world units, up the screen) the warning sorts.")]
        [SerializeField] private float sortBehindEnemy = 0.05f;

        private static AttackTelegraphSettings instance;

        public bool ShowTelegraphs => showTelegraphs;
        public Sprite RingSprite => ringSprite;
        public Sprite FillSprite => fillSprite;
        public Color RingColor => ringColor;
        public Color FillColor => fillColor;
        public int SortingOrder => sortingOrder;
        public float SortBehindEnemy => sortBehindEnemy;

        public static AttackTelegraphSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AttackTelegraphSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"AttackTelegraphSettings not found at Resources/{ResourcePath}. No attack warnings are drawn.");
                        instance = CreateInstance<AttackTelegraphSettings>();
                        instance.showTelegraphs = false;
                    }
                }
                return instance;
            }
        }
    }
}
