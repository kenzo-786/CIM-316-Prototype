using UnityEngine;

public class TutorialProgress
{
    private const string CompletedKey = "TutorialCompleted";

    public static bool IsCompleted =>
        PlayerPrefs.GetInt(CompletedKey, 0) == 1;

    public static void MarkCompleted()
    {
        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(CompletedKey);
        PlayerPrefs.Save();
    }
}
