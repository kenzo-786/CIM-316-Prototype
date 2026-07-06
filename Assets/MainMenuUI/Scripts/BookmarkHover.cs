using UnityEngine;
using UnityEngine.EventSystems;
public class BookmarkHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverDistance = 1000f;
    [SerializeField] private float moveSpeed = 100f;

    private RectTransform rect;
    private Vector2 startPosition;
    private Vector2 targetPosition; 

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        startPosition = rect.anchoredPosition;
        targetPosition = startPosition;
    }
    
    private void Update()
    {
        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            targetPosition,
            Time.deltaTime * moveSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPosition = startPosition + Vector2.down * hoverDistance;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPosition = startPosition;
    }
}
