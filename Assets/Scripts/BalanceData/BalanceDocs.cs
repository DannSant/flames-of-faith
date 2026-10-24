using System.Collections.Generic;
using System.Reflection;

namespace Game.BalanceData
{
    /// <summary>
    /// What each balance table and column means, for a new team member reading the sheet. Shown at the side of every
    /// tab (BalanceSync.gs writes it). A column's text is, in order: an entry here, else the [Tooltip] of the field it
    /// edits (so the Inspector and the sheet say the same thing), else nothing, which BalanceDocsTests rejects.
    /// Add the description of a new table here when you add it to <see cref="BalanceTables.All"/>.
    /// </summary>
    public static class BalanceDocs
    {
        private static readonly Dictionary<string, string> Tables = new()
        {
            ["Enemies"] = "One row per enemy type: how tough it is, how hard it hits and how fast it moves. Max health = Health Base + (wave - 1) x Health Per Wave + levels beaten x Health Per Level, and Corrupted enemies get extra on top. Damage is not a number here: it is a tier (see EnemyDamageTiers) that scales with how far into the run the player is.",
            ["EnemyBehaviors_ChaserLogic"] = "Decides when a chasing enemy walks toward the player and when it stops. Each row is one behavior asset that enemies share; a change affects every enemy using it.",
            ["EnemyBehaviors_DamageOnCollision"] = "Lets an enemy hurt the player by touching them. The damage itself comes from the enemy's Contact Damage Tier (Enemies tab).",
            ["EnemyBehaviors_FleeLogic"] = "Makes an enemy keep its distance from the player (the Corruptor runs away with this), strafing sideways as it goes.",
            ["EnemyBehaviors_FlipTowardPlayer"] = "Turns the enemy's sprite to face the player.",
            ["EnemyBehaviors_Knockback"] = "Pushes the player away when the enemy touches them.",
            ["EnemyBehaviors_MeleeAttacker"] = "A close-range attack: when the player is in range the enemy plays its attack and a hitbox appears next to it. Damage comes from the Contact Damage Tier.",
            ["EnemyBehaviors_MoveToObject"] = "A scripted walk toward a fixed object (DrekoEyeAdd walking to the altar).",
            ["EnemyBehaviors_MoveToSnapshot"] = "Walks toward where the player was a moment ago, not where they are now, so it can be dodged. Walking speed is the enemy's Move Speed.",
            ["EnemyBehaviors_ShootBurst"] = "Fires several volleys of projectiles in a row, each volley fanned out over an angle. Damage comes from the Projectile Damage Tier.",
            ["EnemyBehaviors_Shooter"] = "Fires one projectile at the player now and then while the player is in range. Damage comes from the Projectile Damage Tier.",
            ["Levels"] = "One row per level on the map. Taint Level is the level's base chance of spawning Corrupted enemies.",
            ["WaveLevels"] = "One row per level's wave list (wave database): which Corruptor ends its waves, and whether it is the game's final level.",
            ["Waves"] = "One row per wave of every level, numbered from 1 in the order they are played. To add a wave, add a row with the next number (and its enemies in WaveEnemies). Waves cannot be removed here.",
            ["WaveEnemies"] = "Which enemies can spawn in each wave and how likely each one is. Every spawn picks one entry at random by its Weight. A wave needs at least one row.",
            ["Experience"] = "How much experience enemies drop and how fast the player levels up. Per wave, the game works out how much XP one regular token must be worth so the player gains about Levels Per Wave Target levels from tokens alone.",
            ["ExperienceTokenTiers"] = "The token denominations. Every drop starts as tier 1; for each tier up to the enemy's XP Tier it then rolls that tier's Promote Chance and stops at the first failure. The number of tiers is fixed (each has a sprite), only the numbers can change.",
            ["EnemyDamage"] = "The rules behind enemy damage: armor, minimum damage and dodging. Enemy damage is sized against an expected player, never the real one, so a player with more health or armor than expected really does outscale enemies.",
            ["EnemyDamageCurve"] = "The expected player: the max health and armor we assume the player has after N levels beaten (in-between values are interpolated). Enemy damage is sized to this. Keep the rows sorted by Levels Beaten; rows can be added or removed.",
            ["EnemyDamageTiers"] = "How many hits the expected player survives from an attack of each damage tier. Row 1 is tier 1, row 2 is tier 2, and so on. An enemy's Contact / Projectile Damage Tier picks a row (fractions blend two rows; 0 = no damage).",
            ["DamageSources"] = "Every damage dealer of the player (arrows, fireballs, sword swings, dash trails...), one row each, named after its prefab. Final damage = Base Damage + the player's damage stat of the Weapon Class x Attack Scale, then the player's other bonuses.",
            ["Debuffs"] = "The elemental debuffs items apply: Bleed, Fire, Frost and Ley Charge. Duration = Base Duration + Mastow Affinity x Duration Per Mastow, and the same for Strength. Some columns only apply to one element (their notes say so); the others ignore them.",
            ["Corruption"] = "Grace and Corruption: the player's Grace range and how it changes damage, how often enemies spawn Corrupted and how much stronger they are, the Grace pickup, and the Corruptor phase at the end of each wave.",
            ["PlayerStats"] = "The numbers inside the player's stat formulas: movement, health regen, life steal, dash recovery, shop discount, pickup range, Retribution, and the level-up tier weights. The formulas' shapes are code; these are their parameters.",
            ["Pierce"] = "How the Pierce stat works: the extra enemies a projectile goes through, and the chance for an arrow to split on a hit.",
            ["Stats"] = "One row per player stat: the points a run starts with, whether level-up can offer it and how many points each level-up tier gives, its Stats pane tab, and how Corruption weakens it. Retired stats are not listed.",
            ["Classes"] = "One row per class: its dash. A class's dash Glyph can override the duration and charges with its own tier values.",
        };

        // Columns that are not a field with a tooltip: keys and the list elements. "Table.Column"; "EnemyBehaviors_*.Column"
        // is every table with that prefix, "*.Column" every table. A settings table's rows are looked up the same way.
        private static readonly Dictionary<string, string> Columns = new()
        {
            ["Enemies.Enemy"] = "The enemy type (its name in the game).",
            ["EnemyBehaviors_*.Behavior"] = "The name of the behavior asset. Enemies share these assets, so a change affects every enemy using it.",
            ["Levels.Level"] = "The name of the level asset.",
            ["WaveLevels.Level"] = "The level's wave list (wave database).",
            ["Waves.Level"] = "The level's wave list.",
            ["Waves.Wave"] = "The wave's number inside its level, from 1. A new wave must take the next number.",
            ["WaveEnemies.Level"] = "The level's wave list.",
            ["WaveEnemies.Wave"] = "The wave's number inside its level (it must exist in Waves).",
            ["WaveEnemies.Enemy"] = "An enemy type that can spawn in this wave.",
            ["WaveEnemies.Weight"] = "How likely this enemy is compared to the other rows of the same wave: weight 3 next to weight 1 spawns three times as often.",
            ["ExperienceTokenTiers.Settings"] = "The experience settings asset. Always the same.",
            ["ExperienceTokenTiers.Tier"] = "The token's tier number (1 = the regular token).",
            ["ExperienceTokenTiers.Promote Chance"] = "Chance (0 to 1) that a token of the tier below becomes this tier. Tier 1 is always the start, so its chance is not used.",
            ["ExperienceTokenTiers.Multiplier"] = "How much more than a regular token this tier is worth (2 = double).",
            ["EnemyDamageCurve.Settings"] = "The enemy damage settings asset. Always the same.",
            ["EnemyDamageCurve.Levels Beaten"] = "Combat and boss levels the player has beaten at this point of the run.",
            ["EnemyDamageCurve.Max Health"] = "Max health we expect the player to have at this point.",
            ["EnemyDamageCurve.Armor"] = "Armor we expect the player to have at this point.",
            ["EnemyDamageTiers.Settings"] = "The enemy damage settings asset. Always the same.",
            ["EnemyDamageTiers.Hits To Kill"] = "Hits the expected player survives from an attack of this tier (the player dies on the Nth hit).",
            ["EnemyBehaviors_FleeLogic.Strafe Flip Interval X"] = "Shortest time in seconds before the enemy switches its strafe side.",
            ["EnemyBehaviors_FleeLogic.Strafe Flip Interval Y"] = "Longest time in seconds before the enemy switches its strafe side (each switch picks a random time between X and Y).",
            ["DamageSources.Damage Source"] = "The name of the damage source (the prefab it belongs to).",
            // Every Setting / Value / Notes table
            ["*.Setting"] = "The setting's name.",
            ["*.Value"] = "The number to edit.",
            ["*.Notes"] = "What the setting does (read-only, filled by Unity).",
            ["Debuffs.Debuff"] = "The name of the debuff asset (Bleed has a second, likelier version used by some items).",
            ["Stats.Stat"] = "The stat.",
            ["Stats.Tier 1"] = "Points a tier 1 level-up choice gives (the most common tier).",
            ["Stats.Tier 2"] = "Points a tier 2 level-up choice gives.",
            ["Stats.Tier 3"] = "Points a tier 3 level-up choice gives.",
            ["Stats.Tier 4"] = "Points a tier 4 level-up choice gives (the rarest tier).",
            ["Classes.Class"] = "The class.",
            // Setting rows (their text goes in the Notes column)
            ["PlayerStats.Level-up Tier 1 Weight"] = "How likely a tier 1 level-up choice is, against the other tiers' weights. Each Luck point moves 1 from tier 1, split evenly over tiers 2 to 4.",
            ["PlayerStats.Level-up Tier 2 Weight"] = "How likely a tier 2 level-up choice is, against the other tiers' weights.",
            ["PlayerStats.Level-up Tier 3 Weight"] = "How likely a tier 3 level-up choice is, against the other tiers' weights.",
            ["PlayerStats.Level-up Tier 4 Weight"] = "How likely a tier 4 level-up choice is, against the other tiers' weights.",
        };

        public static string TableDescription(string table) => Tables.TryGetValue(table, out var text) ? text : string.Empty;

        /// <summary>The note of a column: an entry here, else the tooltip of <paramref name="field"/> (may be null).</summary>
        public static string ColumnNote(string table, string column, FieldInfo field)
        {
            if (Columns.TryGetValue(table + "." + column, out var text)) return text;
            int underscore = table.IndexOf('_');
            if (underscore > 0 && Columns.TryGetValue(table.Substring(0, underscore) + "_*." + column, out text)) return text;
            if (Columns.TryGetValue("*." + column, out text)) return text;
            return CellCodec.TooltipOf(field);
        }

        /// <summary>The table names that have a description (for the guard test).</summary>
        public static IEnumerable<string> DescribedTables => Tables.Keys;

        /// <summary>The "Table.Column" keys with a written note (for the guard test).</summary>
        public static IEnumerable<string> DocumentedColumns => Columns.Keys;
    }
}
