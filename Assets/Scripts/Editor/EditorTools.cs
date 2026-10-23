using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class EditorTools : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Sprites/Batch Set Pivot (Isometric)")]
    static void SetPivot()
    {
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
                continue;

            // TextureImporter.spritesheet no longer works in Unity 6.3; sprite rects go through the data provider
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            var rects = provider.GetSpriteRects();
            if (rects == null || rects.Length == 0)
                continue;

            foreach (var rect in rects)
            {
                rect.alignment = SpriteAlignment.Custom;
                rect.pivot = new Vector2(0.5f, 0.4f); // 👈 adjust here
            }

            provider.SetSpriteRects(rects);
            provider.Apply();
            importer.SaveAndReimport();
        }

        Debug.Log("Sprite pivots updated successfully.");
    }
}
#endif

