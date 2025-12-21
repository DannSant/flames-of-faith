using UnityEditor;
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

            var metas = importer.spritesheet;
            if (metas == null || metas.Length == 0)
                continue;

            for (int i = 0; i < metas.Length; i++)
            {
                metas[i].alignment = (int)SpriteAlignment.Custom;
                metas[i].pivot = new Vector2(0.5f, 0.4f); // 👈 adjust here
            }

            importer.spritesheet = metas; // ✅ THIS was missing
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        Debug.Log("Sprite pivots updated successfully.");
    }
}
#endif

