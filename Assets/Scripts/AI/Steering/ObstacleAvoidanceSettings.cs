using UnityEngine;

namespace Game.AI.Steering
{
    /// <summary>
    /// Tunable values for enemy obstacle avoidance (ObstacleSteering), shared by every movement behavior.
    /// Loaded from Resources/AI/ObstacleAvoidanceSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "ObstacleAvoidanceSettings", menuName = "AI/Obstacle Avoidance Settings")]
    public class ObstacleAvoidanceSettings : ScriptableObject
    {
        private const string ResourcePath = "AI/ObstacleAvoidanceSettings";

        [Tooltip("Master switch. Off = every enemy moves in a straight line, as before.")]
        [SerializeField] private bool avoidanceEnabled = true;
        [Tooltip("Layers enemies steer around. Only obstacles (trees): enemies, the player and level borders are ignored.")]
        [SerializeField] private LayerMask obstacleMask = 1 << 8;

        [Header("Sensing")]
        [Tooltip("How far beyond its own collider an enemy starts reacting to an obstacle (world units).")]
        [SerializeField] private float lookAhead = 0.8f;
        [Tooltip("Added to the enemy's collider radius, so it keeps a little gap while going around.")]
        [SerializeField] private float radiusPadding = 0.05f;

        [Header("Choosing a direction")]
        [Tooltip("Directions tried around the enemy. More = smoother turns, slightly more work.")]
        [SerializeField] private int sampleCount = 16;
        [Tooltip("How strongly a nearby obstacle pushes directions toward it down. 2 = touching an obstacle " +
            "makes heading straight into it the worst choice.")]
        [SerializeField] private float dangerWeight = 2f;
        [Tooltip("How fast an obstacle's danger fades for directions that don't point straight at it. " +
            "Higher = the enemy turns less and hugs the obstacle more.")]
        [SerializeField] private float dangerSharpness = 2f;
        [Tooltip("Preference for the side the enemy already chose to go around on, so it doesn't flip-flop.")]
        [SerializeField] private float sideBiasWeight = 0.5f;
        [Tooltip("Seconds the enemy keeps the chosen side after it last needed to steer.")]
        [SerializeField] private float sideCommitTime = 1f;

        [Header("Stuck recovery")]
        [Tooltip("Stuck when it covered less than this fraction of the distance its speed should give over a check window.")]
        [Range(0f, 1f)]
        [SerializeField] private float stuckSpeedRatio = 0.25f;
        [Tooltip("Length of the stuck check window (seconds). When stuck near an obstacle, it switches sides.")]
        [SerializeField] private float stuckCheckWindow = 0.5f;

        [Header("Debug")]
        [Tooltip("Draws the wanted (white) and steered (cyan) directions in the Scene view.")]
        [SerializeField] private bool debugDraw = false;

        public bool AvoidanceEnabled => avoidanceEnabled;
        public LayerMask ObstacleMask => obstacleMask;
        public float LookAhead => lookAhead;
        public float RadiusPadding => radiusPadding;
        public bool DebugDraw => debugDraw;

        public SteeringParams ToParams()
        {
            return new SteeringParams
            {
                sampleCount = sampleCount,
                dangerWeight = dangerWeight,
                dangerSharpness = dangerSharpness,
                sideBiasWeight = sideBiasWeight,
                sideCommitTime = sideCommitTime,
                stuckSpeedRatio = stuckSpeedRatio,
                stuckCheckWindow = stuckCheckWindow,
            };
        }

        private static ObstacleAvoidanceSettings instance;

        public static ObstacleAvoidanceSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ObstacleAvoidanceSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"ObstacleAvoidanceSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<ObstacleAvoidanceSettings>();
                    }
                }
                return instance;
            }
        }
    }
}
