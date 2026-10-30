using Game.Effects;

namespace Game.RunEncounters
{
    /// <summary>What an event action changed; each kind fills its own placeholder in the result text.</summary>
    public enum EventResultKind
    {
        Health,
        Grace,
        Chronos,
        Favors,
        Item
    }

    /// <summary>
    /// What one action actually did: the signed change (health after armor and the 1 HP floor, Grace after its
    /// clamp, Golden Chronos no more than the player had) or the item given. Zero / no item = it gave nothing.
    /// </summary>
    public readonly struct EventActionResult
    {
        public readonly EventResultKind kind;
        public readonly float amount;
        public readonly Effect item;

        public EventActionResult(EventResultKind kind, float amount, Effect item = null)
        {
            this.kind = kind;
            this.amount = amount;
            this.item = item;
        }

        public static EventActionResult Of(EventResultKind kind, float amount) => new(kind, amount);
        public static EventActionResult Item(Effect item) => new(EventResultKind.Item, 0f, item);
        public static EventActionResult Nothing(EventResultKind kind) => new(kind, 0f);

        /// <summary>True when the action changed something (an optional [sentence] about it shows).</summary>
        public bool Gave => kind == EventResultKind.Item ? item != null : amount != 0f;
    }
}
