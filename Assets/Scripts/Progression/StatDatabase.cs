using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression
{
    [CreateAssetMenu(fileName = "StatDatabase", menuName = "Progression/Stat Database")]
    public class StatDatabase : ScriptableObject
    {
        [SerializeField] private List<StatData> stats = new();

        public List<StatData> Stats => stats;
    }
}
