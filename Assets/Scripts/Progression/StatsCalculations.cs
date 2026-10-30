using UnityEngine;

public static class StatsCalculations 
{
    private const float MIN_ATTACK_DELAY = 0.01f;
    private const float MIN_DASH_COOLDOWN = 0.1f;
    /**
         * Calculates the attack delay based on the attack speed and base attack delay.
         * The formula is: delay = baseAttackDelay / (1 + (attackSpeed * scale))
         *  How it works:
            At AttackSpeed = 0:
            delay = 1.5 / (1 + 0) = 1.5

            At AttackSpeed = 100:
            delay = 1.5 / 1.75 = 0.857

            At AttackSpeed = 500:
            delay = 1.5 / 4.75 = 0.316

            At AttackSpeed = 999:
            delay = 1.5 / 8.49 = 0.176 -> clamped to 0.01 if needed
            You can adjust the scale value or curve shape if you want:
            Smaller scale -> slower improvement
            Larger scale -> faster ramp-up
         * 
         * @param attackSpeed The current attack speed of the player.
         * @param baseAttackDelay The base attack delay of the weapon.
         * @param scale A scaling factor to adjust the effect of attack speed on delay.
         * @return The calculated attack delay, ensuring it does not go below MIN_DELAY.
         */
    public static float CalculateAttackDelay(float attackSpeed, float baseAttackDelay, float scale)
    {
        float delay = baseAttackDelay / SafeSpeedDivisor(attackSpeed * scale);
        return Mathf.Max(delay, MIN_ATTACK_DELAY);
    }

    /**
         * The Attack Speed points that count, for CalculateAttackDelay. Same shape as
         * CalculateShopDiscountMultiplier: every point counts up to softCap, each one counts for
         * pastSoftCapRate up to hardCap, and nothing counts past it. Negative points pass through.
         *  How it works (with default softCap=30, hardCap=60, pastSoftCapRate=0.5):
            At attackSpeed = 20: effective = 20
            At attackSpeed = 40: effective = 30 + (10 * 0.5) = 35
            At attackSpeed = 60: effective = 30 + (30 * 0.5) = 45
            At attackSpeed = 999: clamped to 60 -> same as above, 45
         *
         * @param attackSpeed The player's current Attack Speed stat total.
         * @param softCap The stat value at which each point starts counting for less.
         * @param hardCap The stat value beyond which points count for nothing.
         * @param pastSoftCapRate What each point between the caps counts for.
         * @return The effective Attack Speed points.
         */
    public static float CalculateEffectiveAttackSpeed(int attackSpeed, int softCap = 30, int hardCap = 60, float pastSoftCapRate = 0.5f)
    {
        if (attackSpeed <= softCap) return attackSpeed;

        int pointsPastSoftCap = Mathf.Min(attackSpeed, Mathf.Max(softCap, hardCap)) - softCap;
        return softCap + pointsPastSoftCap * pastSoftCapRate;
    }

    /**
         * Speed of a primary attack's animation: base cooldown / current cooldown, so the hit or shot
         * lands inside the cooldown at any Attack Speed. Never below 1: with negative Attack Speed the
         * cooldown is longer, so nothing gets cut off.
         */
    public static float CalculateAttackAnimationSpeed(float baseAttackDelay, float attackDelay)
    {
        return Mathf.Max(1f, baseAttackDelay / Mathf.Max(attackDelay, MIN_ATTACK_DELAY));
    }

    // 1 + bonus, but never below 0.1: with enough negative stat the divisor used to hit 0 or go
    // negative, which turned a huge slowdown into an (almost) instant attack/dash. Now negative
    // stats slow things down by at most 10x.
    private static float SafeSpeedDivisor(float bonus)
    {
        return Mathf.Max(MIN_SPEED_DIVISOR, 1f + bonus);
    }

    private const float MIN_SPEED_DIVISOR = 0.1f;

    /**
     * Calculates the move speed based on the move speed stat and base move speed.
         * The formula is: speed = baseMoveSpeed + (moveSpeed * scale)
         * 
         * @param moveSpeed The current move speed of the player.
         * @param baseMoveSpeed The base move speed of the player.
         * @param scale A scaling factor to adjust the effect of move speed on the final speed.
         * @return The calculated move speed.
         */
    public static float CalculateMoveSpeed(int moveSpeed, float baseMoveSpeed=5f, float scale=.01f)
    {
        float speed = baseMoveSpeed + (moveSpeed * scale);
        return speed;
    }

    public static float CalculateDashCooldown(int dashCooldownStat, float baseDashCooldown, float scale=0.08f)
    {
        float cooldown = baseDashCooldown / SafeSpeedDivisor(dashCooldownStat * scale);
        return Mathf.Max(cooldown, MIN_ATTACK_DELAY);
    }

    /**
         * Seconds between each 1 HP healed by Health Regen: base / (1 + (points - 1) / divisor).
         * 0 or less = no regen (infinity). Defaults: 1 point = 5 s, 3 = 3.1 s, 5 = 2.2 s.
         */
    public static float CalculateHealthRegenInterval(int regenStat, float baseInterval = 5f, float divisor = 2.25f)
    {
        if (regenStat <= 0) return Mathf.Infinity;
        return baseInterval / (1f + (regenStat - 1f) / Mathf.Max(0.01f, divisor));
    }

    /** Chance (0-1) that a hit heals, from Life Steal. 0 points = never. */
    public static float CalculateLifestealChance(int lifestealStat, float baseChance = 0.2f, float chancePerPoint = 0.01f)
    {
        if (lifestealStat <= 0) return 0f;
        return Mathf.Clamp01(baseChance + chancePerPoint * lifestealStat);
    }

    /** Health healed by one Life Steal proc: points × healPerPoint, clamped to [min, max]. */
    public static float CalculateLifestealHeal(int lifestealStat, float healPerPoint = 0.1f, float minHeal = 1f, float maxHeal = 20f)
    {
        if (lifestealStat <= 0) return 0f;
        return Mathf.Clamp(lifestealStat * healPerPoint, minHeal, maxHeal);
    }

    /** Distance at which pickups fly to the player: base + points × perPoint (never below 0). */
    public static float CalculatePickupRange(int pickupRangeStat, float baseRange = 2f, float rangePerPoint = 0.1f)
    {
        return Mathf.Max(0f, baseRange + pickupRangeStat * rangePerPoint);
    }

    /**
         * Chance (0-100) of each level-up upgrade tier. Each Luck point moves 1 from tier 1 to the
         * other tiers, split evenly; the result is normalized to 100.
         */
    public static float[] CalculateUpgradeTierChances(int luck, float[] baseChances)
    {
        if (baseChances == null || baseChances.Length == 0) return new float[0];

        // Same as the original UpgradeManager roll: tier 1 loses `luck` (not below 0) and every
        // other tier gains luck / (tiers - 1), even once tier 1 is empty.
        var chances = (float[])baseChances.Clone();
        float luckPoints = Mathf.Max(0, luck);
        chances[0] = Mathf.Max(0f, chances[0] - luckPoints);
        if (chances.Length > 1)
        {
            float perTier = luckPoints / (chances.Length - 1);
            for (int i = 1; i < chances.Length; i++) chances[i] += perTier;
        }

        float total = 0f;
        foreach (float c in chances) total += Mathf.Max(0f, c);
        if (total <= 0f) return chances;
        for (int i = 0; i < chances.Length; i++) chances[i] = Mathf.Max(0f, chances[i]) / total * 100f;
        return chances;
    }

    /**
         * Calculates the shop price multiplier based on the ShopItemDiscount stat.
         * The formula is piecewise: 1% discount per point up to softCap, then 0.5%
         * per point up to hardCap, then no further benefit past hardCap.
         *  How it works (with default softCap=50, hardCap=80):
            At discountStat = 0:
            discount% = 0 -> multiplier = 1.0

            At discountStat = 50:
            discount% = 50 * 1 = 50 -> multiplier = 0.5

            At discountStat = 80:
            discount% = 50 + (30 * 0.5) = 65 -> multiplier = 0.35

            At discountStat = 999:
            clamped to 80 -> same as above, multiplier = 0.35
         *
         * @param discountStat The player's current ShopItemDiscount stat total.
         * @param softCap The stat value at which the discount rate slows down.
         * @param hardCap The stat value beyond which no further discount is applied.
         * @return The multiplier to apply to a shop item's base price (e.g. 0.65 = 35% off).
         */
    public static float CalculateShopDiscountMultiplier(int discountStat, int softCap = 50, int hardCap = 80)
    {
        int stat = Mathf.Clamp(discountStat, 0, hardCap);
        float discountPercent = stat <= softCap
            ? stat * 1f
            : softCap + (stat - softCap) * 0.5f;

        return 1f - (discountPercent / 100f);
    }

    /**
         * Chance (0-1) to avoid all of an incoming hit, from the Evasion stat. Two linear rates:
         * perPointBeforeSoftCap for the first softCapPoints, then perPointAfterSoftCap, never
         * above maxChance. With the defaults: 1 point = 0.5%, 50 = 25%, 100+ = 30%.
         *
         * @param evasionStat The player's current Evasion stat total. 0 or less = no chance.
         * @return The probability that a hit is dodged.
         */
    public static float CalculateEvasionChance(int evasionStat, int softCapPoints = 50,
        float perPointBeforeSoftCap = 0.005f, float perPointAfterSoftCap = 0.001f, float maxChance = 0.3f)
    {
        if (evasionStat <= 0) return 0f;

        int beforeSoftCap = Mathf.Min(evasionStat, Mathf.Max(0, softCapPoints));
        int afterSoftCap = evasionStat - beforeSoftCap;
        float chance = beforeSoftCap * perPointBeforeSoftCap + afterSoftCap * perPointAfterSoftCap;

        return Mathf.Clamp(chance, 0f, Mathf.Clamp01(maxChance));
    }

    /**
         * Extra enemies a projectile pierces from the Pierce stat: points × perPoint, rounded down
         * (default 0.25 = one more enemy every 4 points). 0 or less = none.
         */
    public static int CalculateExtraPierce(int pierceStat, float extraPiercePerPoint = 0.25f)
    {
        if (pierceStat <= 0 || extraPiercePerPoint <= 0f) return 0;
        // Small epsilon so 4 × 0.25 can't land on 0.99999 and round down to 0.
        return Mathf.FloorToInt(pierceStat * extraPiercePerPoint + 0.0001f);
    }

    /**
         * Chance (0-1) for a linear arrow to split when it hits, from the Pierce stat:
         * points × chancePerPoint, never above maxChance.
         */
    public static float CalculateSplitChance(int pierceStat, float chancePerPoint = 0.03f, float maxChance = 0.4f)
    {
        if (pierceStat <= 0) return 0f;
        return Mathf.Clamp(pierceStat * chancePerPoint, 0f, Mathf.Clamp01(maxChance));
    }

    /**
         * Piecewise reflect-percent curve for the Retribution stat: tier1Rate% per point up to
         * tier1Cap, then tier2Rate% per point up to tier2Cap, then tier3Rate% per point up to the
         * hard cap. Same shape as CalculateShopDiscountMultiplier, extended to three tiers.
         *
         * @return The fraction (0-1) of incoming damage to reflect back at the attacker.
         */
    public static float CalculateRetributionDamagePercent(
        int retributionStat,
        float tier1Cap = 35f, float tier2Cap = 50f, float hardCap = 70f,
        float tier1Rate = 1f, float tier2Rate = 0.2f, float tier3Rate = 0.01f)
    {
        float stat = Mathf.Clamp(retributionStat, 0, hardCap);

        float percent;
        if (stat <= tier1Cap)
        {
            percent = stat * tier1Rate;
        }
        else if (stat <= tier2Cap)
        {
            percent = tier1Cap * tier1Rate + (stat - tier1Cap) * tier2Rate;
        }
        else
        {
            percent = tier1Cap * tier1Rate + (tier2Cap - tier1Cap) * tier2Rate + (stat - tier2Cap) * tier3Rate;
        }

        return percent / 100f;
    }

    /**
         * Piecewise chance curve for Retribution's Grace-on-hit proc: tier1Rate% per point up to
         * tier1Cap, then tier2Rate% per point up to the hard cap.
         *
         * @return The fraction (0-1) chance to generate Grace on a landed weapon hit.
         */
    public static float CalculateRetributionGraceChance(
        int retributionStat,
        float tier1Cap = 25f, float hardCap = 50f,
        float tier1Rate = 1f, float tier2Rate = 0.5f)
    {
        float stat = Mathf.Clamp(retributionStat, 0, hardCap);

        float percent = stat <= tier1Cap
            ? stat * tier1Rate
            : tier1Cap * tier1Rate + (stat - tier1Cap) * tier2Rate;

        return percent / 100f;
    }

    /// <summary>
    /// Main stat points earned going from <paramref name="previousLevel"/> to <paramref name="newLevel"/>
    /// at +1 every <paramref name="levelsPerPoint"/> levels. Counts by level number rather than by
    /// level-ups seen, so the cadence holds whatever level it starts observing from.
    /// </summary>
    public static int CalculateMainStatPointsEarned(int previousLevel, int newLevel, int levelsPerPoint)
    {
        if (levelsPerPoint <= 0 || newLevel <= previousLevel) return 0;
        return newLevel / levelsPerPoint - previousLevel / levelsPerPoint;
    }
}
