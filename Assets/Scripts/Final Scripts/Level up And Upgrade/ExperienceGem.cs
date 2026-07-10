using UnityEngine;

public class ExperienceGem : MonoBehaviour, IPoolable
{
    [SerializeField] private float magnetSpeed = 12f;
    [SerializeField] private float collectDistance = 0.2f;

    private int xpValue;
    private bool magnetizing;
    private Transform target;
    private PlayerExperience playerExperience;
    private PoolHandle poolHandle;

    private void Awake()
    {
        poolHandle = GetComponent<PoolHandle>();
    }

    private void Update()
    {
        if (!magnetizing || target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            magnetSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) <= collectDistance)
        {
            playerExperience.AddXp(xpValue);
            poolHandle.ReturnToPool();
        }
    }

    public void Initialize(int value)
    {
        xpValue = value;
        magnetizing = false;
        target = null;
        playerExperience = null;
    }

    public void MagnetizeTo(Transform player, PlayerExperience experience)
    {
        target = player;
        playerExperience = experience;
        magnetizing = true;
    }

    public void OnSpawnedFromPool() { }

    public void OnReturnedToPool()
    {
        magnetizing = false;
        target = null;
        playerExperience = null;
    }
}
