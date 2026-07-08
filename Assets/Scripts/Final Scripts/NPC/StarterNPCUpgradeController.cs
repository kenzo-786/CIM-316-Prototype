using System.Collections.Generic;
using UnityEngine;

public class StarterNPCUpgradeController : MonoBehaviour
{
    [SerializeField] private UpgradeData[] starterUpgradePool;
    [SerializeField] private int choiceCount = 2;
    [SerializeField] private UpgradeSelectionUI upgradeSelectionUI;
    [SerializeField] private PlayerUpgradeManager playerUpgradeManager;
    [SerializeField] private DoorController starterDoor;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange;
    private bool choiceUsed;

    private void Start()
    {
        if (starterDoor != null)
            starterDoor.CloseAndLock();
    }

    private void Update()
    {
        if (!playerInRange || choiceUsed) return;

        if (Input.GetKeyDown(interactKey))
            ShowStarterChoices();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void ShowStarterChoices()
    {
        choiceUsed = true;

        List<UpgradeData> choices = GetRandomChoices();

        Time.timeScale = 0f;

        upgradeSelectionUI.Show(choices, upgrade =>
        {
            playerUpgradeManager.ApplyUpgrade(upgrade);
            upgradeSelectionUI.Hide();

            if (starterDoor != null)
                starterDoor.OpenAndUnlock();

            Time.timeScale = 1f;
        });
    }

    private List<UpgradeData> GetRandomChoices()
    {
        List<UpgradeData> available = new List<UpgradeData>(starterUpgradePool);
        List<UpgradeData> selected = new List<UpgradeData>();

        int amount = Mathf.Min(choiceCount, available.Count);

        for (int i = 0; i < amount; i++)
        {
            int index = Random.Range(0, available.Count);
            selected.Add(available[index]);
            available.RemoveAt(index);
        }

        return selected;
    }
}
