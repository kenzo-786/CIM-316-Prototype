using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    private const string MasterVolumeKey = "Settings.MasterVolume";
    private const string MusicVolumeKey = "Settings.MusicVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";
    private const string FullscreenKey = "Settings.Fullscreen";
    private const string ScreenShakeKey = "Settings.ScreenShake";
    private const string DamageNumbersKey = "Settings.DamageNumbers";
    private const string TargetFrameRateKey = "Settings.TargetFrameRate";

    [SerializeField] private GameSettingsData settings = new GameSettingsData();

    public static GameSettingsManager Instance { get; private set; }
    public GameSettingsData Settings => settings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        Apply();
    }

    public void SetMasterVolume(float value)
    {
        settings.masterVolume = Mathf.Clamp01(value);
        ApplyAndSave();
    }

    public void SetMusicVolume(float value)
    {
        settings.musicVolume = Mathf.Clamp01(value);
        ApplyAndSave();
    }

    public void SetSfxVolume(float value)
    {
        settings.sfxVolume = Mathf.Clamp01(value);
        ApplyAndSave();
    }

    public void SetFullscreen(bool value)
    {
        settings.fullscreen = value;
        ApplyAndSave();
    }

    public void SetScreenShake(bool value)
    {
        settings.screenShake = value;
        ApplyAndSave();
    }

    public void SetDamageNumbers(bool value)
    {
        settings.damageNumbers = value;
        ApplyAndSave();
    }

    public void SetTargetFrameRate(int value)
    {
        settings.targetFrameRate = value <= 0 ? -1 : Mathf.Max(30, value);
        ApplyAndSave();
    }

    public void ApplyAndSave()
    {
        Apply();
        Save();
    }

    public void Apply()
    {
        AudioListener.volume = settings.masterVolume;
        Screen.fullScreen = settings.fullscreen;
        Application.targetFrameRate = settings.targetFrameRate;
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, settings.masterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, settings.musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, settings.sfxVolume);
        PlayerPrefs.SetInt(FullscreenKey, settings.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(ScreenShakeKey, settings.screenShake ? 1 : 0);
        PlayerPrefs.SetInt(DamageNumbersKey, settings.damageNumbers ? 1 : 0);
        PlayerPrefs.SetInt(TargetFrameRateKey, settings.targetFrameRate);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        settings.masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, settings.masterVolume);
        settings.musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, settings.musicVolume);
        settings.sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, settings.sfxVolume);
        settings.fullscreen = PlayerPrefs.GetInt(FullscreenKey, settings.fullscreen ? 1 : 0) == 1;
        settings.screenShake = PlayerPrefs.GetInt(ScreenShakeKey, settings.screenShake ? 1 : 0) == 1;
        settings.damageNumbers = PlayerPrefs.GetInt(DamageNumbersKey, settings.damageNumbers ? 1 : 0) == 1;
        settings.targetFrameRate = PlayerPrefs.GetInt(TargetFrameRateKey, settings.targetFrameRate);
    }
}
