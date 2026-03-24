using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            if (collision.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
