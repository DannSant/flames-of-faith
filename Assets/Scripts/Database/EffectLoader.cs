

using Game.Effects;
using UnityEngine;
using UnityEditor;


namespace Game.Database
{
    public static class EffectLoader
    {
        /// <summary>
        /// Row -> runtime SO, for the DB window's Test Load button. Delegates to the same mapper
        /// the asset generator uses: this used to be a second hand-written copy, which had already
        /// drifted (it dropped fields, and parsed the Newtonsoft-written JSON with JsonUtility,
        /// so it threw on any real row).
        /// </summary>
        public static Effect CreateEffectSO(EffectRow row)
        {
            Effect effect = ScriptableObject.CreateInstance<Effect>();
            effect.InitializeFromData(row);
            return effect;
        }
        public static void GenerateAndSaveAllEffects(string saveFolder)
        {
#if UNITY_EDITOR
            EffectDatabase.Initialize(DBEditorHelper.DbPath);
            var allEffects = EffectDatabase.GetAllEffects();

            if (!AssetDatabase.IsValidFolder(saveFolder))
                AssetDatabase.CreateFolder("Assets", "GeneratedEffects");

            foreach (var data in allEffects)
            {
                string path = $"{saveFolder}/{data.name}.asset";
                Effect effect = AssetDatabase.LoadAssetAtPath<Effect>(path);

                if (effect == null)
                {
                    effect = ScriptableObject.CreateInstance<Effect>();
                    AssetDatabase.CreateAsset(effect, path);
                }

                effect.InitializeFromData(data);
                EditorUtility.SetDirty(effect);

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }



}
