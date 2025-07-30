using Game.Scene;
using UnityEngine;

public static class LevelTreeDebugger
{
    public static void PrintTree(LevelTree tree)
    {
        if (tree == null || tree.Root == null)
        {
            Debug.Log("LevelTree is empty or null.");
            return;
        }

        Debug.Log("Printing LevelTree:");
        PrintNode(tree.Root, 0);
    }

    private static void PrintNode(LevelNode node, int depth)
    {
        if (node == null) return;

        string indent = new string(' ', depth * 4); // 4 spaces per level
        string nodeLabel =  node.LevelData?.DisplayName ?? "NULL";
        Debug.Log($"{indent}- {nodeLabel}");

        foreach (var child in node.Children)
        {
            PrintNode(child, depth + 1);
        }
    }
}
