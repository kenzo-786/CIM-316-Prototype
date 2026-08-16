using UnityEngine;

public class CreditsPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform creditsText;
    [SerializeField] private CreditsScroller creditsScroller;
    [SerializeField] private Vector2 startingPosition = new Vector2(0, -800);

    private void OnEnable()
    {
        creditsText.anchoredPosition = startingPosition;

        if (creditsScroller != null)
            creditsScroller.enabled = true;
    }

    private void OnDisable()
    {
        if (creditsScroller != null)
            creditsScroller.enabled = false;

        creditsText.anchoredPosition = startingPosition;
    }
}