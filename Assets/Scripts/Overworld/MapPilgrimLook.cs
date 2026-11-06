using UnityEngine;

namespace Game.Overworld
{
    /// <summary>
    /// How the played class's pilgrim looks on the map, pushed by gameplay (PlayerManager) to the map renderer:
    /// its animator controller (from the class data, so new character art arrives with it), its first sprite,
    /// the size it has in a level, and where its feet are from the sprite's pivot.
    /// </summary>
    public class MapPilgrimLook
    {
        public RuntimeAnimatorController controller;
        public Sprite sprite;
        /// <summary>The body's scale in a level (the CharacterVisual's lossy scale).</summary>
        public Vector3 scale = Vector3.one;
        /// <summary>The feet (the body collider's center, IsoGround) from the sprite's position, in world units.</summary>
        public Vector2 feetOffset;
    }
}
