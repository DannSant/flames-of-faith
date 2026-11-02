using System.Collections.Generic;
using UnityEngine;

namespace Game.Cutscenes
{
    /// <summary>The cutscene system's pure rules: what can play, what counts as seen, and what the Chronicle lists.</summary>
    public static class CutsceneRules
    {
        /// <summary>The pages with something to show. None means the scene is empty: it logs and is skipped.</summary>
        public static List<CutscenePage> GetPlayablePages(IReadOnlyList<CutscenePage> pages)
        {
            var playable = new List<CutscenePage>();
            if (pages == null) return playable;

            foreach (var page in pages)
            {
                if (page != null && (page.image != null || !string.IsNullOrWhiteSpace(page.text)))
                {
                    playable.Add(page);
                }
            }
            return playable;
        }

        public static bool HasSeen(IReadOnlyCollection<string> seen, string cutsceneId)
        {
            if (seen == null || string.IsNullOrEmpty(cutsceneId)) return false;
            foreach (string id in seen)
            {
                if (id == cutsceneId) return true;
            }
            return false;
        }

        /// <summary>Adds the id to the seen list. Returns false when there's nothing to add.</summary>
        public static bool AddSeen(List<string> seen, string cutsceneId)
        {
            if (seen == null || string.IsNullOrEmpty(cutsceneId) || HasSeen(seen, cutsceneId)) return false;
            seen.Add(cutsceneId);
            return true;
        }

        /// <summary>
        /// What the Chronicle lists: the seen scenes that still exist, in the order they were seen. Ids in a save with
        /// no asset in this build are left out, never an error.
        /// </summary>
        public static List<CutsceneData> SelectSeen(IEnumerable<CutsceneData> cutscenes, IReadOnlyCollection<string> seen)
        {
            var selected = new List<CutsceneData>();
            if (cutscenes == null || seen == null) return selected;

            foreach (string id in seen)
            {
                foreach (var cutscene in cutscenes)
                {
                    if (cutscene != null && cutscene.CutsceneId == id)
                    {
                        selected.Add(cutscene);
                        break;
                    }
                }
            }
            return selected;
        }
    }

    /// <summary>
    /// Hold to skip. It can't fire by accident: the control has to be released once after the scene opens (a button
    /// already held when it started doesn't count), letting go early starts over, and it fires only once.
    /// </summary>
    public class SkipHold
    {
        private readonly float holdTime;
        private float held;
        private bool armed;
        private bool fired;

        public SkipHold(float holdTime)
        {
            this.holdTime = Mathf.Max(0.01f, holdTime);
        }

        /// <summary>How far the hold is, 0..1.</summary>
        public float Progress => Mathf.Clamp01(held / holdTime);

        /// <summary>One frame. Returns true on the frame the hold completes.</summary>
        public bool Tick(bool pressed, float deltaTime)
        {
            if (!pressed)
            {
                armed = true;
                held = 0f;
                return false;
            }
            if (!armed || fired) return false;

            held += deltaTime;
            fired = held >= holdTime;
            return fired;
        }
    }
}
