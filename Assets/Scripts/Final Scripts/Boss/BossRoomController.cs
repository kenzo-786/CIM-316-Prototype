using System;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [SerializeField] private BossHealthTarget bossTarget;
    [SerializeField] private DoorController exitDoor;
    [SerializeField] private bool openExitOnBossDefeated = true;

    private bool cleared;

    public event Action OnBossRoomCleared;

    private void Awake()
    {
        if (bossTarget == null)
            bossTarget = GetComponentInChildren<BossHealthTarget>();

        if (exitDoor == null)
            exitDoor = GetComponentInChildren<DoorController>();
    }

    private void OnEnable()
    {
        if (bossTarget != null)
            bossTarget.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        if (bossTarget != null)
            bossTarget.OnBossDefeated -= HandleBossDefeated;
    }

    private void HandleBossDefeated()
    {
        if (cleared)
            return;

        cleared = true;

        if (openExitOnBossDefeated && exitDoor != null)
            exitDoor.OpenAndUnlock();

        OnBossRoomCleared?.Invoke();
    }
}
