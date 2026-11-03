namespace Game.Audio
{
    /// <summary>
    /// Every one-off game sound, played through AudioManager.Play. Each has one entry in GameSoundTable.
    /// Add new values at the end only: the table's entries store them by index.
    /// </summary>
    public enum GameSound
    {
        PlayerHurt,
        Dash,
        PickupXp,
        PickupGrace,
        PickupHealth,
        PickupItem,
        LevelUp,
        WaveStart,
        WaveEnd,
        UiClick,
        UiMove,
        UiBack,
        ShopBuy,
        ShopError,
        LowHealthHeartbeat,
        CorruptorAppears,
        CorruptorKilled,
        CorruptorEscaped,
        BossPhaseChange,
        BossDeath,
        PlayerDodge,
        ShieldAbsorb,
    }
}
