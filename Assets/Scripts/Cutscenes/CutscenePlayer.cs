using Game.Common;
using Game.Metaprogression;
using Game.UI.Cutscenes;
using System;
using UnityEngine;

namespace Game.Cutscenes
{
    /// <summary>
    /// Starts cutscenes by id, from anywhere: Play(id, onEnded). It finds the scene, pushes its pages to the window and
    /// records it as seen when it ends, finished or skipped. It lives in the Bootstrapper scene (prefab
    /// Prefabs/UI/Cutscenes/CutscenePlayer) and draws above the loading screen, so a scene can play during a transition.
    /// Input goes through the EventSystem of whatever scene is loaded (there is none in the Bootstrapper), so play it
    /// before the current scene is unloaded.
    /// onEnded always fires, also when nothing can play: callers wait on it inside a load, and a scene that never ends
    /// would hang the loading screen.
    /// </summary>
    public class CutscenePlayer : Singleton<CutscenePlayer>
    {
        [SerializeField] private CutsceneWindowUI window;

        private string playingId;
        private Action endedCallback;

        public bool IsPlaying => playingId != null;

        public void Play(string cutsceneId, Action onEnded)
        {
            if (IsPlaying)
            {
                Debug.LogWarning($"[Cutscene] '{cutsceneId}' asked to play while '{playingId}' is playing, skipping it.");
                onEnded?.Invoke();
                return;
            }

            var cutscene = CutsceneDatabaseProvider.Instance != null ? CutsceneDatabaseProvider.Instance.Get(cutsceneId) : null;
            var pages = CutsceneRules.GetPlayablePages(cutscene != null ? cutscene.Pages : null);
            if (pages.Count == 0 || window == null)
            {
                Debug.LogWarning($"[Cutscene] Can't play '{cutsceneId}': {(cutscene == null ? "no such scene" : pages.Count == 0 ? "it has no pages" : "no window")}. Skipping it.");
                onEnded?.Invoke();
                return;
            }

            playingId = cutsceneId;
            endedCallback = onEnded;
            try
            {
                window.Show(pages, HandleEnded);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Cutscene] '{cutsceneId}' failed to start, skipping it: {ex}");
                playingId = null;
                endedCallback = null;
                window.Abort();
                onEnded?.Invoke();
            }
        }

        private void HandleEnded()
        {
            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.MarkCutsceneSeen(playingId);
            }

            var callback = endedCallback;
            playingId = null;
            endedCallback = null;
            callback?.Invoke();
        }
    }
}
