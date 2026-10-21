using UnityEngine;

namespace Game.VFX
{
    /// <summary>
    /// One Ley Charge strike: a jagged bolt drawn once, fading out over its lifetime.
    /// It used to redraw a new zigzag every 0.03 s for 0.2 s, so one strike read as 6-7 strikes.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class ChainLightningBolt : MonoBehaviour
    {
        [SerializeField] private int segmentCount = 8;
        [SerializeField] private float jitterAmount = 0.25f;
        [Tooltip("Seconds the bolt takes to fade out, from full to nothing.")]
        [SerializeField] private float lifetime = 0.2f;

        private LineRenderer lineRenderer;
        private Color startColor;
        private Color endColor;
        private float lifeTimer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            startColor = lineRenderer.startColor;
            endColor = lineRenderer.endColor;
        }

        public void Init(Vector3 fromPoint, Vector3 toPoint)
        {
            lineRenderer.positionCount = segmentCount + 1;
            DrawJaggedLine(fromPoint, toPoint);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            float alpha = GetFadeAlpha(lifeTimer, lifetime);
            lineRenderer.startColor = WithAlpha(startColor, startColor.a * alpha);
            lineRenderer.endColor = WithAlpha(endColor, endColor.a * alpha);
        }

        /// <summary>1 when the bolt appears, falling linearly to 0 at the end of its lifetime.</summary>
        public static float GetFadeAlpha(float elapsed, float lifetime)
        {
            if (lifetime <= 0f) return 0f;
            return Mathf.Clamp01(1f - elapsed / lifetime);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private void DrawJaggedLine(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector3 point = Vector3.Lerp(from, to, t);
                if (i != 0 && i != segmentCount)
                {
                    point += perpendicular * Random.Range(-jitterAmount, jitterAmount);
                }
                lineRenderer.SetPosition(i, point);
            }
        }
    }
}
