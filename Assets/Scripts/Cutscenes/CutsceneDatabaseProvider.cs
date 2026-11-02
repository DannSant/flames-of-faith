using Game.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cutscenes
{
    /// <summary>Every CutsceneData in Resources/Cutscenes, by id.</summary>
    public class CutsceneDatabaseProvider : Singleton<CutsceneDatabaseProvider>
    {
        public const string ResourceFolder = "Cutscenes";

        private readonly List<CutsceneData> cutscenes = new();
        private readonly Dictionary<string, CutsceneData> byId = new();

        public IReadOnlyList<CutsceneData> Cutscenes => cutscenes;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            foreach (var cutscene in Resources.LoadAll<CutsceneData>(ResourceFolder))
            {
                if (string.IsNullOrEmpty(cutscene.CutsceneId) || byId.ContainsKey(cutscene.CutsceneId))
                {
                    Debug.LogError($"[Cutscene] {cutscene.name} has an empty or repeated cutsceneId ('{cutscene.CutsceneId}'), ignoring it.");
                    continue;
                }
                byId[cutscene.CutsceneId] = cutscene;
                cutscenes.Add(cutscene);
            }
        }

        /// <summary>The scene with this id, or null.</summary>
        public CutsceneData Get(string cutsceneId) =>
            !string.IsNullOrEmpty(cutsceneId) && byId.TryGetValue(cutsceneId, out var cutscene) ? cutscene : null;
    }
}
