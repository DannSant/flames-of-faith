namespace Game.Pickups
{
    /// <summary>
    /// Marker for anything that occupies a "pickup slot" in the world - a collectible a spawner
    /// should avoid dropping loot on top of (e.g. an item pickup and an experience token landing
    /// on the exact same spot). Implemented by BasePickup and by ExperienceToken.
    /// </summary>
    public interface IWorldPickup
    {
    }
}
