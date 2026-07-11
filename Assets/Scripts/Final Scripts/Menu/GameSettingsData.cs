using UnityEngine;

[System.Serializable]
public class GameSettingsData 
{
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    public bool fullscreen = true;
    public bool screenShake = true;
    public bool damageNumbers = true;

    public int targetFrameRate = 60;
}
