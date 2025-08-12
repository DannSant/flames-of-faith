using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class EnemyHealthbar : MonoBehaviour
    {
        [SerializeField] private Slider healthbarSlider;

        public void Awake()
        {
            Hide();
        }

        public void SetHealth(float currentHealth, float maxHealth)
        {
            
            if (maxHealth <= 0) return; // Prevent division by zero
            healthbarSlider.value = currentHealth / maxHealth;
            Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

}