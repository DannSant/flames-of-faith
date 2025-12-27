using Game.Currency;
using Game.Effects;
using Game.Progression;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public Effect effectToTest;
    private PlayerProgression playerProgression;
    private EffectStore effectStore;
    private CurrencyWallet currencyWallet;
    private PlayerExperience playerExperience;
    private void Start()
    {
        playerProgression = FindAnyObjectByType<PlayerProgression>();
        effectStore = FindAnyObjectByType<EffectStore>();
        currencyWallet = FindAnyObjectByType<CurrencyWallet>();
        playerExperience = FindAnyObjectByType<PlayerExperience>();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        TestFeatures();
#endif
    }

    private void TestFeatures()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //effectStore.AddEffect(effectToTest);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currencyWallet.AddCurrency(100);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerExperience.AddExperience(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            playerProgression.UpdateStat(StatType.MaxHealth, 10);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            playerProgression.UpdateStat(StatType.MoveSpeed, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            playerProgression.UpdateStat(StatType.DashCooldown, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            playerProgression.UpdateStat(StatType.MaxGrace, 1);
        }
    }
}
