using UnityEngine;

namespace Game.Progression
{
    public class ExperienceDropper : MonoBehaviour
    {
        [SerializeField] private GameObject experienceTokenPrefab;       
        [SerializeField] private int xpAmount = 1;
        [SerializeField] private float dropChance = 0.8f;

        public void TryDrop()
        {
            if (experienceTokenPrefab == null)
            {
                Debug.LogWarning("ExperienceToken prefab not assigned.");
                return;
            }

            if (Random.value <= dropChance)
            {
                var token = Instantiate(experienceTokenPrefab, transform.position, Quaternion.identity);
                var experience = token.GetComponent<ExperienceToken>();

                if (experience != null)
                {
                    experience.SetAmount(xpAmount);
                }
            }
        }
    }

}