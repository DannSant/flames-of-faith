using System.Collections.Generic;
using System.Linq;
using Game.GameSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("Volume")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle vsyncToggle;

        [Header("Language")]
        [SerializeField] private TMP_Dropdown languageDropdown;

        private static readonly string[] languageOptions = { "English" };

        private List<(int width, int height)> availableResolutions;

        private void OnEnable()
        {
            PopulateResolutionOptions();
            PopulateQualityOptions();
            PopulateLanguageOptions();
            RefreshFromSettingsManager();
            WireCallbacks();
        }

        private void OnDisable()
        {
            UnwireCallbacks();
        }

        private void PopulateResolutionOptions()
        {
            availableResolutions = Screen.resolutions
                .Select(r => (r.width, r.height))
                .Distinct()
                .OrderBy(r => r.width).ThenBy(r => r.height)
                .ToList();

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(availableResolutions.Select(r => $"{r.width} x {r.height}").ToList());
        }

        private void PopulateQualityOptions()
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
        }

        private void PopulateLanguageOptions()
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(languageOptions.ToList());
        }

        private void RefreshFromSettingsManager()
        {
            if (SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager settings = SettingsManager.Instance;

            masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            musicVolumeSlider.SetValueWithoutNotify(settings.MusicVolume);
            sfxVolumeSlider.SetValueWithoutNotify(settings.SfxVolume);

            int resolutionIndex = availableResolutions.FindIndex(r =>
                r.width == settings.ResolutionWidth && r.height == settings.ResolutionHeight);
            resolutionDropdown.SetValueWithoutNotify(Mathf.Max(resolutionIndex, 0));

            fullscreenToggle.SetIsOnWithoutNotify(settings.FullscreenMode != FullScreenMode.Windowed);
            qualityDropdown.SetValueWithoutNotify(settings.QualityLevel);
            vsyncToggle.SetIsOnWithoutNotify(settings.VsyncEnabled);

            // Only "English" exists today, so this always resolves to index 0.
            languageDropdown.SetValueWithoutNotify(0);
        }

        private void WireCallbacks()
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        private void UnwireCallbacks()
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
            vsyncToggle.onValueChanged.RemoveListener(OnVsyncChanged);
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }

        private void OnMasterVolumeChanged(float value)
        {
            SettingsManager.Instance?.SetMasterVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            SettingsManager.Instance?.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            SettingsManager.Instance?.SetSfxVolume(value);
        }

        private void OnResolutionChanged(int index)
        {
            (int width, int height) resolution = availableResolutions[index];
            SettingsManager.Instance?.SetResolution(resolution.width, resolution.height);
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            SettingsManager.Instance?.SetFullscreenMode(isFullscreen);
        }

        private void OnQualityChanged(int index)
        {
            SettingsManager.Instance?.SetQualityLevel(index);
        }

        private void OnVsyncChanged(bool enabled)
        {
            SettingsManager.Instance?.SetVsync(enabled);
        }

        private void OnLanguageChanged(int index)
        {
            // TODO: map dropdown index to a language key once more languages are added.
            SettingsManager.Instance?.SetLanguage("en");
        }
    }
}
