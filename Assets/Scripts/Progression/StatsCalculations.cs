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

    public static float CalculateDashCooldown(int dashCooldownStat, float baseDashCooldown, float scale=0.01f)
    {

        float cooldown = baseDashCooldown / (1f + (dashCooldownStat * scale));
        return Mathf.Max(cooldown, MIN_ATTACK_DELAY);
    }
}
