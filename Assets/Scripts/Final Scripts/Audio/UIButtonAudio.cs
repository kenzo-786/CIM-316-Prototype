using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour
{
    [SerializeField] private string hoverSoundId = "ui_hover";
    [SerializeField] private string clickSoundId = "ui_confirm";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(PlayClick);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
            FeedbackEventBus.PlaySound(hoverSoundId, transform.position);
    }

    private void PlayClick()
    {
        FeedbackEventBus.PlaySound(clickSoundId, transform.position);
    }
}
