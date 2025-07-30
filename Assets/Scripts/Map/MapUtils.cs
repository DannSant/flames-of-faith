using Game.Common;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    [System.Serializable]
    public struct MapIconSprite
    {
        public Sprite sprite;
        public LevelType levelType;
    }

    public class MapUtils : Singleton<MapUtils>
    {
        [SerializeField] private List<MapIconSprite> iconSprites = new();

        public Sprite GetSprite(LevelType type)
        {
            foreach (var icon in iconSprites)
            {
                if (icon.levelType == type)
                {
                    return icon.sprite;
                }
            }
            Debug.LogWarning($"No sprite found for level type: {type}");
            return null; // or a default sprite
        }
    }
}