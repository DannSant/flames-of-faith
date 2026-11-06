using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld.Render
{
    public class OverworldEdgeView : MonoBehaviour
    {
        private LineRenderer line;

        public string FromNodeId { get; private set; }
        public string ToNodeId { get; private set; }

        public void Initialize(string fromId, string toId, Vector3 fromPos, Vector3 toPos)
        {
            FromNodeId = fromId;
            ToNodeId = toId;

            line = GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, fromPos);
            line.SetPosition(1, toPos);
        }


        /// <summary>The edge as drawn, from its from-node to its to-node (the pilgrim walks along it).</summary>
        public List<Vector2> Points
        {
            get
            {
                var points = new List<Vector2>();
                if (line == null) return points;
                for (int i = 0; i < line.positionCount; i++)
                {
                    Vector3 p = line.GetPosition(i);
                    points.Add(line.useWorldSpace ? (Vector2)p : (Vector2)line.transform.TransformPoint(p));
                }
                return points;
            }
        }

        public void SetVisible(bool visible)
        {
            line.enabled = visible;
        }

    }

}