using UnityEditor;
using UnityEngine;

namespace Game.Database
{
    public class ScriptableObjectGenerator
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Effects/Generate ScriptableObjects")]
        public static void GenerateEffectsSO()
        {            
            EffectLoader.GenerateAndSaveAllEffects("Assets/Resources/Effects");
        }
#endif
    }

}