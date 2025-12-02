using Game.Database;
using Game.Effects;
using Game.Progression;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Database
{
    public class EffectDatabaseWindow : EditorWindow
    {
        private Vector2 scroll;
        private Vector2 editScroll;
        private List<EffectRow> rows;
        private EffectRow selectedRow;
        private Sprite iconSprite;
        private List<StatModifier> statModifiers = new List<StatModifier>();
        private List<EffectBehavior> behaviors = new List<EffectBehavior>();

        [MenuItem("Tools/Effects/Effect Database")]
        public static void Open()
        {
            var w = GetWindow<EffectDatabaseWindow>("Effects DB");
            w.Show();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            EffectDatabase.Initialize(DBEditorHelper.DbPath);
            rows = EffectDatabase.GetAllEffects();           
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Effect Database", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh")) Refresh();
            if (GUILayout.Button("New Effect")) 
            { 
                selectedRow = new EffectRow();
                iconSprite = null;
                statModifiers = new List<StatModifier>();
                behaviors = new List<EffectBehavior>();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            // left list (unchanged)
            DrawListPanel();

            // right editor inside scroll view
            editScroll = GUILayout.BeginScrollView(editScroll, GUILayout.ExpandHeight(true));
            DrawEditPanel();
            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();
        }
        private void DrawListPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(260));
            GUILayout.Label("Effects", EditorStyles.boldLabel);

            scroll = GUILayout.BeginScrollView(scroll);

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    GUILayout.BeginHorizontal("box");
                    if (GUILayout.Button($"{row.name}", GUILayout.Height(40)))
                    {
                        selectedRow = new EffectRow
                        {
                            id = row.id,
                            effectID = row.effectID,
                            name = row.name,
                            description = row.description,
                            scalingValue = row.scalingValue,
                            priceBuy = row.priceBuy,
                            priceSell = row.priceSell,
                            iconKey = row.iconKey,
                            statModifiersJson = row.statModifiersJson,
                            behaviorsJson = row.behaviorsJson
                        };
                        SelectEffect(row);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void SelectEffect(EffectRow row)
        {
            selectedRow = row;

            // Try to load the sprite based on IconKey
            if (!string.IsNullOrEmpty(selectedRow.iconKey))
            {
                // If your icons are in a Resources folder:
                iconSprite = Resources.Load<Sprite>($"Icons/{selectedRow.iconKey}");
               
            }
            else
            {
                iconSprite = null;
            }

            //Try to generate a list based on the JSON statModifiers field
            if (!string.IsNullOrEmpty(selectedRow.statModifiersJson))
            {
                try
                {
                    statModifiers = Newtonsoft.Json.JsonConvert
                                    .DeserializeObject<List<StatModifier>>(selectedRow.statModifiersJson);
                }
                catch
                {
                    statModifiers = new List<StatModifier>();
                }
            }
            else
            {
                statModifiers = new List<StatModifier>();
            }

            if(!string.IsNullOrEmpty(selectedRow.behaviorsJson))
            {
                try
                {
                    var behaviorsStringList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(selectedRow.behaviorsJson);
                    behaviors = new List<EffectBehavior>();
                    foreach (var behaviorID in behaviorsStringList)
                    {
                        var behavior = Resources.Load<EffectBehavior>($"Effects/EffectBehaviors/{behaviorID}");
                        if (behavior != null)
                        {
                            behaviors.Add(behavior);
                        }
                    }
                }
                catch
                {
                    behaviors = new List<EffectBehavior>();
                }
            }
            else
            {
                behaviors = new List<EffectBehavior>();
            }

            Repaint();
        }
        private void DrawEditPanel()
        {
            GUILayout.BeginVertical("box");

            if (selectedRow == null)
            {
                GUILayout.Label("Select or Create an effect");
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label("Edit Effect", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            /*Basic data*/
            selectedRow.effectID = EditorGUILayout.TextField("Effect ID", selectedRow.effectID);
            selectedRow.name = EditorGUILayout.TextField("Name", selectedRow.name);

            GUILayout.Label("Description");
            selectedRow.description = EditorGUILayout.TextArea(selectedRow.description, GUILayout.Height(60));

            EditorGUILayout.Space();

            /*Sprite icon*/
            Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(
               "Icon",
               iconSprite,
               typeof(Sprite),
               false
            );
            if (newSprite != iconSprite)
            {
                iconSprite = newSprite;

                if (iconSprite != null)
                {
                    // Store the sprite *name* in the DB row
                    selectedRow.iconKey = iconSprite.name;
                }
                else
                {
                    // If cleared, clear the key
                    selectedRow.iconKey = null;
                }
            }
            EditorGUILayout.SelectableLabel(selectedRow.iconKey, EditorStyles.textField);

            /*Numbers data*/
            selectedRow.scalingValue = EditorGUILayout.FloatField("Scaling Value", selectedRow.scalingValue);
            selectedRow.priceBuy = EditorGUILayout.IntField("Buy Price", selectedRow.priceBuy);
            selectedRow.priceSell = EditorGUILayout.IntField("Sell Price", selectedRow.priceSell);
            selectedRow.unlockedByDefault = EditorGUILayout.IntField("Unlocked by default", selectedRow.unlockedByDefault);

            EditorGUILayout.Space();

            /*Stat modifiers data*/
            GUILayout.Label("Stat Modifiers", EditorStyles.boldLabel);
            // Draw the list
            for (int i = 0; i < statModifiers.Count; i++)
            {
                var mod = statModifiers[i];
                EditorGUILayout.BeginHorizontal("box");

                mod.stat = (StatType)EditorGUILayout.EnumPopup( mod.stat, GUILayout.Width(120));
                mod.type = (ModifierType)EditorGUILayout.EnumPopup( mod.type, GUILayout.Width(120));
                mod.value = EditorGUILayout.FloatField( mod.value, GUILayout.Width(60));
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    statModifiers.RemoveAt(i);
                    GUI.backgroundColor = Color.white;
                    break;

                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Modifier"))
            {
                statModifiers.Add(new StatModifier());
            }
            EditorGUILayout.SelectableLabel(selectedRow.statModifiersJson, EditorStyles.textArea);
            EditorGUILayout.Space();

            /*Behaviors data*/
            GUILayout.Label("Behaviors", EditorStyles.boldLabel);
            // Draw the list
            for (int i = 0; i < behaviors.Count; i++)
            {
                var behavior = behaviors[i];
                EditorGUILayout.BeginHorizontal("box");
                behaviors[i] = (EffectBehavior)EditorGUILayout.ObjectField(behavior, typeof(EffectBehavior), false);

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    behaviors.RemoveAt(i);
                    GUI.backgroundColor = Color.white;
                    break;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Behavior"))
            {
                behaviors.Add(null);
            }
            EditorGUILayout.SelectableLabel(selectedRow.behaviorsJson, EditorStyles.textArea);

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) SaveSelectedRow();
            if (selectedRow.id > 0 && GUILayout.Button("Delete")) DeleteRow();
            if (GUILayout.Button("Test Load")) TestLoadEffect();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        private void SaveSelectedRow()
        {
            if (string.IsNullOrEmpty(selectedRow.effectID))
            {
                selectedRow.effectID = System.Guid.NewGuid().ToString();
            }

            // Convert modifiers to JSON
            selectedRow.statModifiersJson =
                Newtonsoft.Json.JsonConvert.SerializeObject(statModifiers, Formatting.None);

            // Convert behaviors to JSON (list of IDs)
            var behaviorIDs = new List<string>();
            foreach (var behavior in behaviors)
            {
                if (behavior != null)
                {
                    behaviorIDs.Add(behavior.BehaviorId);
                }
            }
            selectedRow.behaviorsJson =
                Newtonsoft.Json.JsonConvert.SerializeObject(behaviorIDs, Formatting.None);

            // Save to DB
            if (selectedRow.id == 0)
            {
                EffectDatabase.InsertEffect(selectedRow);
            }
            else
            {
                EffectDatabase.UpdateEffect(selectedRow);
            }

            Refresh();
        }

        private void DeleteRow()
        {
            if (EditorUtility.DisplayDialog("Delete Effect", "Are you sure?", "Yes", "No"))
            {
                EffectDatabase.DeleteEffect(selectedRow.id);
                selectedRow = null;
                Refresh();
            }
        }

        private void TestLoadEffect()
        {
            var so = EffectLoader.CreateEffectSO(selectedRow);
            Debug.Log("Loaded SO ? " + so.EffectName);
        }

    }
}
