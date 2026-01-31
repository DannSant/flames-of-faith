using Game.Common;
using Game.Control;
using Game.Scene;
using Game.Waves;
using UnityEngine;

namespace Game.Progression
{
    public class ExperienceToken : MonoBehaviour, ISceneCleanupHandler
    {
       
        [SerializeField] private int testExtraExperience = 8;

        private int xpAmount = 1;
       
        

        public void SetAmount(int amount)
        {
            xpAmount = amount;
        }



        

        private void OnEnable()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCompleteEnded += HandleWaveComplete;
            }
        }

        private void OnDisable()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCompleteEnded -= HandleWaveComplete;
            }
        }


       

        private void OnTriggerEnter2D(Collider2D other)
        {           
            
            var playerXP = other.GetComponent<PlayerExperience>();
            if (playerXP != null)
            {
               
                playerXP.AddExperience(xpAmount + testExtraExperience);
                Destroy(gameObject);
            }
            
        }

        private void HandleWaveComplete()
        {
            Destroy(gameObject);
        }

        public void Cleanup()
        {
           Destroy(gameObject);
        }
    }
}
