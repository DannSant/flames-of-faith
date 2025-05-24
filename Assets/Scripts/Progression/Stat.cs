using UnityEngine;

namespace Game.Progression
{
    [System.Serializable]
    public class Stat
    {
        [SerializeField] private StatType statType;
        [SerializeField] private int initialValue;
        [SerializeField] private bool allowLevelUpgrades;
        
    }

}