using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 30f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    }
}