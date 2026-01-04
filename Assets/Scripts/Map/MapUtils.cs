using Game.Common;
using Game.Scene;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    
    [Obsolete]
    public class MapUtils : Singleton<MapUtils>
    {
        //[SerializeField] private List<MapIconSprite> iconSprites = new();

        public Sprite GetSprite(LevelType type)
        {
           
           
            Debug.LogWarning($"No sprite found for level type: {type}");
            return null; // or a default sprite
        }
    }
}