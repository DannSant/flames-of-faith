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
    private void Start()
    {
        playerProgression = FindAnyObjectByType<PlayerProgression>();
        effectStore = FindAnyObjectByType<EffectStore>();
        currencyWallet = FindAnyObjectByType<CurrencyWallet>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            effectStore.AddEffect(effectToTest);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currencyWallet.AddCurrency(100);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //Debug.Log("Increasing armor");
            playerProgression.UpdateStat(StatType.MeleeDamage, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            playerProgression.UpdateStat(StatType.AttackSpeed, 1);
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
