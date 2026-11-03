using Game.Control;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// The class whose voice a player component's sounds use (GameSoundTable Class Clips),
    /// read from the PlayerClassManager on the player. None when there is none: the shared clips play.
    /// </summary>
    public static class PlayerVoice
    {
        public static CharacterClass Of(Component speaker)
        {
            if (speaker == null) return CharacterClass.None;
            var classManager = speaker.GetComponentInParent<PlayerClassManager>();
            return classManager != null ? classManager.CharacterClass : CharacterClass.None;
        }
    }
}
