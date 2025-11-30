using Game.Database;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Database
{
    public class EffectDatabaseWindow : EditorWindow
    {
        private Vector2 scroll;
        private List<EffectRow> rows;
        private EffectRow selectedRow;
        private Sprite iconSprite;

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
            if (GUILayout.Button("New Effect")) selectedRow = new EffectRow();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            DrawListPanel();
            DrawEditPanel();

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
                    if (GUILayout.Button($"{row.effectID}\n{row.name}", GUILayout.Height(40)))
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

            selectedRow.effectID = EditorGUILayout.TextField("Effect ID", selectedRow.effectID);
            selectedRow.name = EditorGUILayout.TextField("Name", selectedRow.name);

            GUILayout.Label("Description");
            selectedRow.description = EditorGUILayout.TextArea(selectedRow.description, GUILayout.Height(60));

            EditorGUILayout.Space();

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

            selectedRow.scalingValue = EditorGUILayout.FloatField("Scaling Value", selectedRow.scalingValue);
            selectedRow.priceBuy = EditorGUILayout.IntField("Buy Price", selectedRow.priceBuy);
            selectedRow.priceSell = EditorGUILayout.IntField("Sell Price", selectedRow.priceSell);

            EditorGUILayout.Space();
            GUILayout.Label("JSON: Stat Modifiers");
            selectedRow.statModifiersJson = EditorGUILayout.TextArea(selectedRow.statModifiersJson, GUILayout.Height(50));

            GUILayout.Label("JSON: Behaviors");
            selectedRow.behaviorsJson = EditorGUILayout.TextArea(selectedRow.behaviorsJson, GUILayout.Height(50));

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
