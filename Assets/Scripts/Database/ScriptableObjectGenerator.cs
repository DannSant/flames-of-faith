using UnityEditor;
using UnityEngine;

namespace Game.Database
{
    [System.Obsolete("Retired Oct 2026 (data-driven step 5): items are edited through the balance sheet tables (Items, ItemStats, ItemStatBonuses, ItemBehaviors) and Tools/Flames of Faith/Items/New Item. Kept until a play-test round confirms the sheet path, then deleted with the SQLite library.")]
    public class ScriptableObjectGenerator
    {
#if UNITY_EDITOR
        // Menu item removed (Oct 2026): generating from the retired DB would overwrite the sheet's values
        public static void GenerateEffectsSO()
        {            
            EffectLoader.GenerateAndSaveAllEffects("Assets/Resources/Effects");
        }
#endif
    }

}