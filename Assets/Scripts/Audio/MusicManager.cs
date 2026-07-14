using System.Collections.Generic;
using Game.Common;
using Game.Overworld;
using Game.Scene;
using UnityEngine;

namespace Game.Audio
{
    public class MusicManager : Singleton<MusicManager>
    {
        private readonly Dictionary<AudioClip, float> savedPositions = new Dictionary<AudioClip, float>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            MainSceneController mainSceneController = MainSceneController.Instance;
            if (mainSceneController != null)
            {
                mainSceneController.OnGameplayUISetupRequested += HandleGameplayLoaded;
                mainSceneController.OnLevelSelectorUISetupRequested += HandleLevelSelectorLoaded;
                mainSceneController.OnGameplayStateResetRequested += HandleNewRunStarted;
            }
        }

        private void OnDestroy()
        {
            MainSceneController mainSceneController = MainSceneController.Instance;
            if (mainSceneController != null)
            {
                mainSceneController.OnGameplayUISetupRequested -= HandleGameplayLoaded;
                mainSceneController.OnLevelSelectorUISetupRequested -= HandleLevelSelectorLoaded;
                mainSceneController.OnGameplayStateResetRequested -= HandleNewRunStarted;
            }
        }

        private void HandleGameplayLoaded()
        {
            LevelData currentLevel = GameSession.Instance.currentLevel;
            PlayTrack(currentLevel != null ? currentLevel.MusicClip : null);
        }

        private void HandleLevelSelectorLoaded()
        {
            RunMapGraph currentAct = MapRunController.Instance.CurrentAct;
            PlayTrack(currentAct != null ? currentAct.mapMusic : null);
        }

        private void HandleNewRunStarted()
        {
            savedPositions.Clear();
        }

        public void PlayTrack(AudioClip newClip)
        {
            if (newClip == null)
            {
                return;
            }

            AudioManager audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                return;
            }

            AudioClip previousClip = audioManager.CurrentMusicClip;
            if (previousClip == newClip && audioManager.IsMusicPlaying)
            {
                return;
            }

            if (previousClip != null)
            {
                savedPositions[previousClip] = audioManager.MusicTime;
            }

            float startTime = savedPositions.TryGetValue(newClip, out float savedTime) ? savedTime : 0f;
            audioManager.PlayMusic(newClip, startTime);
        }
    }
}
