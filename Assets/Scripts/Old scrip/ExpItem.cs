using UnityEngine;

public class ExpItem : MonoBehaviour
{
    public float expValue = 10f;
    public float moveSpeed = 8f;
    private Transform player;
    private bool isFollowing = false;

    void Start()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        float range = player.GetComponent<PlayerController>().collectionRange;

        if (distance <= range)
        {
            isFollowing = true;
        }

        if (isFollowing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            if (distance < 0.2f)
            {
                player.GetComponent<PlayerController>().AddExperience(expValue);
                Destroy(gameObject);
            }
        }
    }

}
