using UnityEngine;

namespace Game.AI {
    public enum EnemyType
    {
        SlimeChaser,
        SlimeRoamer,
        FlameShooter,
        LightDevourerAttacker,
        DrekoEye,
        SlimeExplosive,
        Boss,
        // Corruptors: always Corrupted, spawned at the end of every wave. One per act.
        CorruptorAct1
    }
}