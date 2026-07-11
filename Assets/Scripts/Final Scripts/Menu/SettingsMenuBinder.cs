using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenuBinder : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Toggle damageNumbersToggle;

    [SerializeField] private TMP_Dropdown frameRateDropdown;

    private bool suppressEvents;

    private void OnEnable()
    {
        Refresh();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Refresh()
    {
        GameSettingsManager manager = GameSettingsManager.Instance;

        if (manager == null)
            return;

        suppressEvents = true;

        GameSettingsData settings = manager.Settings;

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = settings.masterVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = settings.musicVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = settings.sfxVolume;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = settings.fullscreen;

        if (screenShakeToggle != null)
            screenShakeToggle.isOn = settings.screenShake;

        if (damageNumbersToggle != null)
            damageNumbersToggle.isOn = settings.damageNumbers;

        if (frameRateDropdown != null)
            frameRateDropdown.value = FrameRateToIndex(settings.targetFrameRate);

        suppressEvents = false;
    }

    private void Subscribe()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (screenShakeToggle != null)
            screenShakeToggle.onValueChanged.AddListener(SetScreenShake);

        if (damageNumbersToggle != null)
            damageNumbersToggle.onValueChanged.AddListener(SetDamageNumbers);

        if (frameRateDropdown != null)
            frameRateDropdown.onValueChanged.AddListener(SetFrameRateFromDropdown);
    }

    private void Unsubscribe()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);

        if (screenShakeToggle != null)
            screenShakeToggle.onValueChanged.RemoveListener(SetScreenShake);

        if (damageNumbersToggle != null)
            damageNumbersToggle.onValueChanged.RemoveListener(SetDamageNumbers);

        if (frameRateDropdown != null)
            frameRateDropdown.onValueChanged.RemoveListener(SetFrameRateFromDropdown);
    }

    private void SetMasterVolume(float value)
    {
        if (!suppressEvents && GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetMasterVolume(value);
    }

    private void SetMusicVolume(float value)
    {
        if (!suppressEvents && GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetMusicVolume(value);
    }

    private void SetSfxVolume(float value)
    {
        if (!suppressEvents && GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetSfxVolume(value);
    }

    private void SetFullscreen(bool value)
    {
        if (!suppressEvents && GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetFullscreen(value);
    }

    private void SetScreenShake(bool value)
    {
        if (!suppressEvents && GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetScreenShake(value);
    }

    private void SetDamageNumbers(bool value)
    {
        if (!suppressEvents && GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetDamageNumbers(value);
    }

    private void SetFrameRateFromDropdown(int index)
    {
        if (suppressEvents || GameSettingsManager.Instance == null)
            return;

        GameSettingsManager.Instance.SetTargetFrameRate(IndexToFrameRate(index));
    }

    private int IndexToFrameRate(int index)
    {
        if (index == 0)
            return 30;

        if (index == 1)
            return 60;

        if (index == 2)
            return 120;

        return -1;
    }

    private int FrameRateToIndex(int frameRate)
    {
        if (frameRate <= 30)
            return 0;

        if (frameRate <= 60)
            return 1;

        if (frameRate <= 120)
            return 2;

        return 3;
    }
}
