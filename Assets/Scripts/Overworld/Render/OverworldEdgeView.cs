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


        public void SetVisible(bool visible)
        {
            line.enabled = visible;
        }

    }

}