using UnityEngine;

public class DebugXpHotkey : MonoBehaviour
{
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private int xpAmount = 10;
    [SerializeField] private KeyCode key = KeyCode.L;

    private void Update()
    {
        if (Input.GetKeyDown(key))
            playerExperience.AddXp(xpAmount);
    }
}
