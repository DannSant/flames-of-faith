using Game.Metaprogression;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SystemsTester 
{
    [MenuItem("DevTools/Meta/Test Save Meta")]
    public static void TestSave()
    {
        MetaProgressionManager.Instance.SaveUnlockedEffects(
            new List<string> { "8064e3c0-a56e-4051-bcc4-53ff6e120290" }
        );
    }
}
