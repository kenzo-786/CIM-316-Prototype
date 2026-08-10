using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private enum TutorialStep
    {
        Movement,
        Weapon,
        DefeatTarget,
        Combat,
        Harvest,
        Exit,
        Complete
    }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerExperience playerExperience;

    [Header("Tutorial")]
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private DoorController exitDoor;
    [SerializeField] private ObjectPool xpGemPool;

    [Header("Enemies")]
    [SerializeField] private EnemyData tutorialDummyData;
    [SerializeField] private EnemyData tutorialChaserData;
    [SerializeField] private Transform dummySpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Timing")]
    [SerializeField] private float requiredMovementDistance = 3f;
    [SerializeField] private float phaseDelay = 1f;
    [SerializeField] private float gemFloorDelay = 0.8f;
    [SerializeField] private int tutorialXpAmount = 1;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "ImplementScene";

    private TutorialStep currentStep;
    private EnemyBase currentEnemy;
    private Vector2 previousPlayerPosition;
    private float travelledDistance;
    private int xpBeforeHarvest;
    private int levelBeforeHarvest;
    private bool loadingGameplay;

    private void Awake()
    {
        if (player != null)
        {
            if (playerMovement == null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }

            if (weaponController == null)
            {
                weaponController = player.GetComponent<PlayerWeaponController>();
            }

            if (playerExperience == null)
            {
                playerExperience = player.GetComponent<PlayerExperience>();
            }
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (weaponController != null)
        {
            weaponController.SetCombatActive(false);
            weaponController.OnAttackPerformed += HandleAttackPerformed;
        }

        if (playerExperience != null)
        {
            playerExperience.OnXpChanged += HandleXpChanged;
        }

        if (exitDoor != null)
        {
            exitDoor.CloseAndLock();
        }

        previousPlayerPosition = player != null
            ? player.position
            : Vector2.zero;

        currentStep = TutorialStep.Movement;

        tutorialUI.Show(
            "MOVE",
            "Use W, A, S and D to move around the room."
        );
    }

    private void OnDestroy()
    {
        if (weaponController != null)
        {
            weaponController.OnAttackPerformed -= HandleAttackPerformed;
        }

        if (playerExperience != null)
        {
            playerExperience.OnXpChanged -= HandleXpChanged;
        }

        ClearEnemySubscription();
    }

    private void Update()
    {
        if (currentStep != TutorialStep.Movement || player == null)
        {
            return;
        }

        Vector2 currentPosition = player.position;

        travelledDistance += Vector2.Distance(
            previousPlayerPosition,
            currentPosition
        );

        previousPlayerPosition = currentPosition;

        if (travelledDistance >= requiredMovementDistance)
        {
            StartCoroutine(BeginWeaponTutorial());
        }
    }

    private IEnumerator BeginWeaponTutorial()
    {
        currentStep = TutorialStep.Weapon;

        tutorialUI.Show(
            "WEAPON TRAINING",
            "Preparing a practice target."
        );

        yield return new WaitForSeconds(phaseDelay);

        SpawnEnemy(tutorialDummyData, dummySpawnPoint);

        tutorialUI.Show(
            "FIRE",
            GetFiringInstruction()
        );

        if (weaponController != null)
        {
            weaponController.SetCombatActive(true);
        }
    }

    private string GetFiringInstruction()
    {
        PlayerCharacterData character = SelectedCharacter.CharacterData;

        if (character == null)
        {
            return "Aim toward the target and fire.";
        }

        switch (character.firingMode)
        {
            case PlayerFiringMode.BuildAStationaryMouseAim:
                return "Stop moving and aim at the target with the mouse.";

            case PlayerFiringMode.BuildBMoveAndShootMouseAim:
                return "Aim with the mouse. You can move while firing.";

            case PlayerFiringMode.BuildCStationaryAutoTarget:
                return "Stop moving. Your weapon will automatically target the enemy.";

            default:
                return "Aim toward the target and fire.";
        }
    }

    private void HandleAttackPerformed(Vector2 direction)
    {
        if (currentStep != TutorialStep.Weapon)
        {
            return;
        }

        currentStep = TutorialStep.DefeatTarget;

        tutorialUI.Show(
            "PRACTICE TARGET",
            "Keep attacking until the target is destroyed."
        );
    }

    private void SpawnEnemy(EnemyData data, Transform spawnPoint)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("Tutorial enemy data or prefab is missing.", this);
            return;
        }

        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : transform.position;

        GameObject enemyObject = Instantiate(
            data.prefab,
            position,
            Quaternion.identity
        );

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            Debug.LogError("Tutorial enemy prefab has no EnemyBase.", enemyObject);
            Destroy(enemyObject);
            return;
        }

        enemy.Initialize(data, player);
        SetCurrentEnemy(enemy);
    }

    private void SetCurrentEnemy(EnemyBase enemy)
    {
        ClearEnemySubscription();

        currentEnemy = enemy;

        if (currentEnemy != null)
        {
            currentEnemy.OnEnemyDied += HandleEnemyDied;
        }
    }

    private void ClearEnemySubscription()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnEnemyDied -= HandleEnemyDied;
            currentEnemy = null;
        }
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        Vector3 deathPosition = enemy.transform.position;

        ClearEnemySubscription();
        DespawnPlayerProjectiles();

        if (currentStep == TutorialStep.Weapon ||
            currentStep == TutorialStep.DefeatTarget)
        {
            if (weaponController != null)
            {
                weaponController.SetCombatActive(false);
            }

            StartCoroutine(BeginCombatTutorial());
            return;
        }

        if (currentStep == TutorialStep.Combat)
        {
            if (weaponController != null)
            {
                weaponController.SetCombatActive(false);
            }

            StartCoroutine(BeginXpHarvest(deathPosition));
        }
    }

    private IEnumerator BeginCombatTutorial()
    {
        currentStep = TutorialStep.Combat;

        tutorialUI.Show(
            "ENEMY INCOMING",
            "Avoid its attacks and defeat it."
        );

        yield return new WaitForSeconds(phaseDelay);

        SpawnEnemy(tutorialChaserData, enemySpawnPoint);

        if (weaponController != null)
        {
            weaponController.SetCombatActive(true);
        }
    }

    private IEnumerator BeginXpHarvest(Vector3 deathPosition)
    {
        currentStep = TutorialStep.Harvest;

        xpBeforeHarvest = playerExperience != null
            ? playerExperience.CurrentXp
            : 0;

        levelBeforeHarvest = playerExperience != null
            ? playerExperience.Level
            : 1;

        tutorialUI.Show(
            "ROOM CLEARED",
            "Experience is collected after the room is cleared."
        );

        yield return new WaitForSeconds(gemFloorDelay);

        GameObject gemObject = xpGemPool != null
            ? xpGemPool.Get(deathPosition, Quaternion.identity)
            : null;

        ExperienceGem gem = gemObject != null
            ? gemObject.GetComponent<ExperienceGem>()
            : null;

        if (gem != null && playerExperience != null)
        {
            gem.Initialize(Mathf.Max(1, tutorialXpAmount));
            gem.MagnetizeTo(player, playerExperience);
        }
        else if (playerExperience != null)
        {
            playerExperience.AddXp(Mathf.Max(1, tutorialXpAmount));
        }
        else
        {
            OpenTutorialExit();
        }
    }

    private void HandleXpChanged(int currentXp, int xpToNextLevel, int level)
    {
        if (currentStep != TutorialStep.Harvest)
        {
            return;
        }

        if (currentXp != xpBeforeHarvest || level > levelBeforeHarvest)
        {
            OpenTutorialExit();
        }
    }

    private void OpenTutorialExit()
    {
        currentStep = TutorialStep.Exit;

        if (exitDoor != null)
        {
            exitDoor.OpenAndUnlock();
        }

        tutorialUI.Show(
            "TUTORIAL COMPLETE",
            "Enter the green door to begin the run."
        );
    }

    private void DespawnPlayerProjectiles()
    {
        EraserProjectile[] projectiles =
            FindObjectsOfType<EraserProjectile>();

        foreach (EraserProjectile projectile in projectiles)
        {
            if (projectile != null &&
                projectile.gameObject.activeInHierarchy)
            {
                PooledProjectileUtility.Despawn(
                    projectile.gameObject
                );
            }
        }
    }

    public void CompleteTutorialAndLoadGame()
    {
        if (loadingGameplay)
        {
            return;
        }

        loadingGameplay = true;
        currentStep = TutorialStep.Complete;

        TutorialProgress.MarkCompleted();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void SkipTutorial()
    {
        CompleteTutorialAndLoadGame();
    }

    public void ResetTutorialProgress()
    {
        TutorialProgress.Reset();
    }
}
