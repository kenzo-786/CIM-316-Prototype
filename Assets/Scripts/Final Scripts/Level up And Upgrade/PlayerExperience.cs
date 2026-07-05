using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int baseXpToLevel = 10;
    [SerializeField] private float levelXpGrowth = 1.35f;

    public event Action<int, int, int> OnXpChanged;
    public event Action<int> OnLevelUp;

    public int Level { get; private set; }
    public int CurrentXp { get; private set; }
    public int XpToNextLevel { get; private set; }

    private void Awake()
    {
        Level = startingLevel;
        XpToNextLevel = baseXpToLevel;
        OnXpChanged?.Invoke(CurrentXp, XpToNextLevel, Level);
    }

    public void AddXp(int amount)
    {
        if (amount <= 0) return;

        CurrentXp += amount;

        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            Level++;
            XpToNextLevel = Mathf.CeilToInt(baseXpToLevel * Mathf.Pow(levelXpGrowth, Level - 1));
            OnLevelUp?.Invoke(Level);
        }

        OnXpChanged?.Invoke(CurrentXp, XpToNextLevel, Level);
    }
}
