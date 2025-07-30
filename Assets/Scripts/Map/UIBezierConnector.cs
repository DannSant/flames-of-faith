using UnityEngine;
using UnityEngine.UI;

namespace Game.Map
{

    [RequireComponent(typeof(CanvasRenderer))]
    public class UIBezierConnector : Graphic
    {
        public RectTransform startPoint;
        public RectTransform endPoint;

        public float thickness = 5f;
        public Color lineColor = Color.white;

        [Range(0, 1)]
        public float curvature = 0.5f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (startPoint == null || endPoint == null)
                return;

            Vector2 start = WorldToLocal(startPoint.position);
            Vector2 end = WorldToLocal(endPoint.position);

            // Compute control points
            Vector2 direction = end - start;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
            float controlOffset = Vector2.Distance(start, end) * curvature;

            Vector2 control1 = start + direction * 0.25f + perpendicular * controlOffset;
            Vector2 control2 = start + direction * 0.75f + perpendicular * controlOffset;

            const int segments = 20;
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                points[i] = CalculateBezierPoint(t, start, control1, control2, end);
            }

            // Draw quad strip
            for (int i = 0; i < segments; i++)
            {
                Vector2 p0 = points[i];
                Vector2 p1 = points[i + 1];

                Vector2 dir = (p1 - p0).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

                UIVertex v0 = UIVertex.simpleVert;
                v0.color = lineColor;
                v0.position = p0 + normal;

                UIVertex v1 = UIVertex.simpleVert;
                v1.color = lineColor;
                v1.position = p0 - normal;

                UIVertex v2 = UIVertex.simpleVert;
                v2.color = lineColor;
                v2.position = p1 + normal;

                UIVertex v3 = UIVertex.simpleVert;
                v3.color = lineColor;
                v3.position = p1 - normal;

                int idx = vh.currentVertCount;
                vh.AddVert(v0);
                vh.AddVert(v1);
                vh.AddVert(v2);
                vh.AddVert(v3);
                vh.AddTriangle(idx, idx + 1, idx + 2);
                vh.AddTriangle(idx + 2, idx + 1, idx + 3);
            }
        }

        Vector2 WorldToLocal(Vector3 worldPos)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out local
            );
            return local;
        }

        Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
        }

        public void Refresh()
        {
            SetVerticesDirty();
        }
    }

}