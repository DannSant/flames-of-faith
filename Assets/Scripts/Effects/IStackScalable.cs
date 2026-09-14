namespace Game.Effects
{
    /// <summary>
    /// Implemented by components on an effect-spawned prefab that respond to the effect being
    /// stacked. The spawning <see cref="EffectBehavior"/> pushes the resolved scaling in right
    /// after Instantiate - the component can't look it up itself, because at that point Start
    /// hasn't run and its EffectStore reference is still null.
    ///
    /// Implementers read only the targets they support and ignore the rest; an unscaled target
    /// returns 1, so no filtering is needed.
    /// </summary>
    public interface IStackScalable
    {
        void ApplyStackScaling(StackScaling scaling);
    }
}
