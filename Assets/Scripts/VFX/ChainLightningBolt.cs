using UnityEngine;

namespace Game.VFX
{
    [RequireComponent(typeof(LineRenderer))]
    public class ChainLightningBolt : MonoBehaviour
    {
        [SerializeField] private int segmentCount = 8;
        [SerializeField] private float jitterAmount = 0.25f;
        [SerializeField] private float refreshInterval = 0.03f;
        [SerializeField] private float lifetime = 0.2f;

        private LineRenderer lineRenderer;
        private Vector3 from;
        private Vector3 to;
        private float refreshTimer;
        private float lifeTimer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void Init(Vector3 fromPoint, Vector3 toPoint)
        {
            from = fromPoint;
            to = toPoint;
            lineRenderer.positionCount = segmentCount + 1;
            GenerateJitteredPoints();
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshInterval)
            {
                refreshTimer = 0f;
                GenerateJitteredPoints();
            }
        }

        private void GenerateJitteredPoints()
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
