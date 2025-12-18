

using Game.Effects;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Game.Database;


namespace Game.Database
{
    [Serializable]
    public class StatModifierList
    {
        public List<StatModifier> list;
    }

    [Serializable]
    public class BehaviorIDList
    {
        public List<string> ids;
    }
    public static class EffectLoader
    {
        public static Effect CreateEffectSO(EffectRow row)
        {
            // Create runtime SO instance
            Effect effect = ScriptableObject.CreateInstance<Effect>();

            effect.EffectID = row.effectID;
            effect.EffectName = row.name;
            effect.Description = row.description;
            effect.BuyPrice = row.priceBuy;
            effect.SellPrice = row.priceSell;

            // Load icon
            if (!string.IsNullOrEmpty(row.iconKey))
                effect.EffectIcon = Resources.Load<Sprite>($"Icons/{row.iconKey}");

            // Deserialize stat modifiers
            effect.StatModifiers = JsonUtility.FromJson<StatModifierList>(row.statModifiersJson).list;

            // Deserialize behaviors
            var behaviorIDs = JsonUtility.FromJson<BehaviorIDList>(row.behaviorsJson).ids;

            effect.Behaviors = behaviorIDs
                .Select(id => Resources.Load<EffectBehavior>($"Effects/EffectBehaviors/{id}"))
                .Where(b => b != null)
                .ToList();

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
