using System;
using UnityEngine;
using static UnityEngine.LightAnchor;

namespace Game.Overworld
{
    public enum MapDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Serializable]
    public class ConnectionDefinition
    {
        [Header("Target")]
        public string toNodeId;

        [Header("Direction")]
        public MapDirection direction; // Up, Down, Left, Right (for arrows / input)

        [Header("Reveal Rules")]
        public bool revealOnClear = true;

        [Header("Randomization")]
        public bool canBeDisabledByRng = false;

        [Range(0f, 1f)]
        public float enableChance = 1f; // only used if canBeDisabledByRng

        public string rngGroupId; // optional: "A", "SidePath1", etc.

        [Header("Safety")]
        public bool required; // never disabled
        public bool countsTowardMinimumPaths;
    }
}
