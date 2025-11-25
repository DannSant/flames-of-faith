using Game.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapDebugUtils
{
    public static string MapToString(List<MapLayer> map)
    {
        if (map == null)
            return "Map is NULL";

        string result = "";

        foreach (var layer in map)
        {
            result += $"Layer {layer.ToString()}  \n";
        }

        return result;
    }
}