using Game.Audio;
using Game.Common;
using System;
using UnityEngine;

namespace Game.GameSettings
{
    // Single mutation point for app-level settings (audio/graphics/language).
    // UI panels should always go through this instance rather than touching
    // AudioManager/QualitySettings/Screen directly, so future settings entry
    // points (e.g. a pause-menu settings panel) can reuse it as-is.
    public class SettingsManager : Singleton<SettingsManager>
    {
        private const string HasSavedSettingsKey = "Settings.HasSavedSettings";
        private const string MasterVolumeKey = "Settings.MasterVolume";
        private const string MusicVolumeKey = "Settings.MusicVolume";
        private const string SfxVolumeKey = "Settings.SfxVolume";
        private const string ResolutionWidthKey = "Settings.ResolutionWidth";
        private const string ResolutionHeightKey = "Settings.ResolutionHeight";
        private const string FullscreenModeKey = "Settings.FullscreenMode";
        private const string QualityLevelKey = "Settings.QualityLevel";
        private const string VsyncEnabledKey = "Settings.VsyncEnabled";
        private const string LanguageKeyKey = "Settings.LanguageKey";
        private const string ShowTutorialHintsKey = "Settings.ShowTutorialHints";
        private const string AutoAttackEnabledKey = "Settings.AutoAttackEnabled";

        private const float defaultMasterVolume = 1f;
        private const float defaultMusicVolume = 0.7f;
        private const float defaultSfxVolume = 1f;
        private const string defaultLanguageKey = "en";

        private float masterVolume;
        private float musicVolume;
        private float sfxVolume;
        private int resolutionWidth;
        private int resolutionHeight;
        private FullScreenMode fullscreenMode;
        private int qualityLevel;
        private bool vsyncEnabled;
        private string languageKey;
        private bool showTutorialHints;
        private bool autoAttackEnabled;

        [Header("Developer Tools")]
        [SerializeField] private bool developerCheatsEnabled = true;

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;
        public int ResolutionWidth => resolutionWidth;
        public int ResolutionHeight => resolutionHeight;
        public FullScreenMode FullscreenMode => fullscreenMode;
        public int QualityLevel => qualityLevel;
        public bool VsyncEnabled => vsyncEnabled;
        public string LanguageKey => languageKey;
        public bool ShowTutorialHints => showTutorialHints;

        public event Action<bool> OnShowTutorialHintsChanged;
        public bool AutoAttackEnabled => autoAttackEnabled;
        public event Action<bool> OnAutoAttackChanged;
        public bool DeveloperCheatsEnabled => developerCheatsEnabled;

        protected override void Awake()
        {
            base.Awake();
            LoadState();
            ApplyResolution();
            ApplyQuality();
            ApplyVsync();
        }

        private void Start()
        {
            // Applied here (not Awake) so it runs after every singleton's Awake,
            // avoiding a race with AudioManager.Awake() resetting music volume.
            ApplyAudio();
        }

        public void LoadState()
        {
            bool hasSavedSettings = PlayerPrefs.GetInt(HasSavedSettingsKey, 0) == 1;

            if (!hasSavedSettings)
            {
                masterVolume = defaultMasterVolume;
                musicVolume = defaultMusicVolume;
                sfxVolume = defaultSfxVolume;
                resolutionWidth = Screen.currentResolution.width;
                resolutionHeight = Screen.currentResolution.height;
                fullscreenMode = Screen.fullScreenMode;
                qualityLevel = QualitySettings.GetQualityLevel();
                vsyncEnabled = true;
                languageKey = defaultLanguageKey;
                showTutorialHints = true;
                autoAttackEnabled = true;
                return;
            }

            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume);
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
            resolutionWidth = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.currentResolution.width);
            resolutionHeight = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.currentResolution.height);
            fullscreenMode = (FullScreenMode)PlayerPrefs.GetInt(FullscreenModeKey, (int)Screen.fullScreenMode);
            qualityLevel = PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
            vsyncEnabled = PlayerPrefs.GetInt(VsyncEnabledKey, 1) == 1;
            languageKey = PlayerPrefs.GetString(LanguageKeyKey, defaultLanguageKey);
            showTutorialHints = PlayerPrefs.GetInt(ShowTutorialHintsKey, 1) == 1;
            autoAttackEnabled = PlayerPrefs.GetInt(AutoAttackEnabledKey, 1) == 1;
        }

        public void SaveState()
        {
            PlayerPrefs.SetInt(HasSavedSettingsKey, 1);
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.SetInt(ResolutionWidthKey, resolutionWidth);
            PlayerPrefs.SetInt(ResolutionHeightKey, resolutionHeight);
            PlayerPrefs.SetInt(FullscreenModeKey, (int)fullscreenMode);
            PlayerPrefs.SetInt(QualityLevelKey, qualityLevel);
            PlayerPrefs.SetInt(VsyncEnabledKey, vsyncEnabled ? 1 : 0);
            PlayerPrefs.SetString(LanguageKeyKey, languageKey);
            PlayerPrefs.SetInt(ShowTutorialHintsKey, showTutorialHints ? 1 : 0);
            PlayerPrefs.SetInt(AutoAttackEnabledKey, autoAttackEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ResetState()
        {
            PlayerPrefs.SetInt(HasSavedSettingsKey, 0);
            LoadState();
            SaveState();
            ApplyAudio();
            ApplyResolution();
            ApplyQuality();
            ApplyVsync();
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyAudio();
            SaveState();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            ApplyAudio();
            SaveState();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            ApplyAudio();
            SaveState();
        }

        public void SetResolution(int width, int height)
        {
            resolutionWidth = width;
            resolutionHeight = height;
            ApplyResolution();
            SaveState();
        }

        public void SetFullscreenMode(bool isFullscreen)
        {
            fullscreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            ApplyResolution();
            SaveState();
        }

        public void SetQualityLevel(int index)
        {
            qualityLevel = index;
            ApplyQuality();
            SaveState();
        }

        public void SetVsync(bool enabled)
        {
            vsyncEnabled = enabled;
            ApplyVsync();
            SaveState();
        }

        public void SetLanguage(string key)
        {
            languageKey = key;
            SaveState();
        }

        public void SetShowTutorialHints(bool enabled)
        {
            showTutorialHints = enabled;
            SaveState();
            OnShowTutorialHintsChanged?.Invoke(showTutorialHints);
        }

        public void SetAutoAttackEnabled(bool enabled)
        {
            if (autoAttackEnabled == enabled) return;
            autoAttackEnabled = enabled;
            SaveState();
            OnAutoAttackChanged?.Invoke(autoAttackEnabled);
        }

        private void ApplyAudio()
        {
            if (AudioManager.Instance == null)
            {
                return;
            }

            AudioManager.Instance.SetMasterVolume(masterVolume);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSfxVolume(sfxVolume);
        }

        private void ApplyResolution()
        {
            Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreenMode);
        }

        private void ApplyQuality()
        {
            QualitySettings.SetQualityLevel(qualityLevel, true);
        }

        private void ApplyVsync()
        {
            QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        }
    }
}
