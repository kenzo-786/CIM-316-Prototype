using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float fireRate = 0.2f;
    public float bulletDamage = 10f;
    public float bulletSizeMultiplier = 1f;
    public bool isAutoShooting = false;

    [Header("Stats & Progression")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int level = 1;
    public float experience = 0;
    public float expToLevelUp = 100f;
    public float collectionRange = 3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePos;
    private float nextFireTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            isAutoShooting = !isAutoShooting;
            Debug.Log("Auto-Shooting: " + isAutoShooting);
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (Camera.main != null)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        bool shootInput = Input.GetMouseButton(0) || isAutoShooting;

        if (shootInput && Time.time >= nextFireTime && Time.timeScale > 0)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.transform.localScale *= bulletSizeMultiplier;

        Bullet bScript = bullet.GetComponent<Bullet>();
        if (bScript != null) bScript.damage = bulletDamage;

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null) bulletRb.linearVelocity = firePoint.right * bulletSpeed;
    }

    public void AddExperience(float amount)
    {
        experience += amount;
        if (experience >= expToLevelUp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        experience -= expToLevelUp;
        expToLevelUp = Mathf.Floor(expToLevelUp * 1.2f);

        currentHealth = maxHealth;

        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
        {
            ui.ShowUpgradeScreen();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Debug.Log("Game over");
        }
    }
}
