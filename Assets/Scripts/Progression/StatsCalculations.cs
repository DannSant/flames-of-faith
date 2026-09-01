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
    public static float CalculateAttackDelay(int attackSpeed, float baseAttackDelay, float scale)
    {

        float delay = baseAttackDelay / (1f + (attackSpeed * scale));
        return Mathf.Max(delay, MIN_ATTACK_DELAY);
    }

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

        float cooldown = baseDashCooldown / (1f + (dashCooldownStat * scale));
        return Mathf.Max(cooldown, MIN_ATTACK_DELAY);
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
}
