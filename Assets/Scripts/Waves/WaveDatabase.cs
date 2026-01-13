using System.Collections.Generic;
using UnityEngine;

namespace Game.Waves {
    [CreateAssetMenu(fileName = "WaveDatabase", menuName = "Waves/Wave Database")]
    public class WaveDatabase : ScriptableObject
    {
        [Tooltip("List of all wave data, in order.")]
        public List<WaveData> waves = new();

        public bool lastWave = false;
      
    }
}
