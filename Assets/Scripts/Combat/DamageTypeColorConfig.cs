using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(fileName = "DamageTypeColorConfig", menuName = "Combat/Damage Type Color Config")]
    public class DamageTypeColorConfig : ScriptableObject
    {
        [SerializeField] private Color meleeColor = new Color32(0xE7, 0x4C, 0x3C, 0xFF);
        [SerializeField] private Color rangedColor = new Color32(0x2E, 0xCC, 0x71, 0xFF);
        [SerializeField] private Color magicColor = new Color32(0x93, 0x59, 0xE8, 0xFF);

        public Color GetColor(WeaponClass damageType)
        {
            return damageType switch
            {
                WeaponClass.Melee => meleeColor,
                WeaponClass.Ranged => rangedColor,
                WeaponClass.Magic => magicColor,
                _ => Color.white
            };
        }
    }
}
