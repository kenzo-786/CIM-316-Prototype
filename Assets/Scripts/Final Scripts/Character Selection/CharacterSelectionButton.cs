using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CharacterSelectionButton : MonoBehaviour
{
    [SerializeField] private PlayerCharacterData characterData;
    [SerializeField] private string tutorialSceneName = "TutorialScene";
    [SerializeField] private string gameplaySceneName = "ImplementScene";
    [SerializeField] private bool alwaysShowTutorialDuringDevelopment = true;

    [Header("Button Pop")]
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float popSpeed = 0.08f;

    private Button button;
    private Vector3 originalScale;
    private Coroutine popCoroutine;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;

        button.onClick.AddListener(SelectCharacter);
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(PlayPop);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayPop);
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SelectCharacter);
        }
    }

    private void PlayPop()
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopButton());
    }

    private IEnumerator PopButton()
    {
        Vector3 enlargedScale = originalScale * popScale;
        float time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                originalScale,
                enlargedScale,
                time / popSpeed
            );

            yield return null;
        }

        time = 0f;

        while (time < popSpeed)
        {
            time += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                enlargedScale,
                originalScale,
                time / popSpeed
            );

            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void SelectCharacter()
    {
        if (characterData == null)
        {
            Debug.LogError("CharacterSelectionButton has no Character Data.", this);
            return;
        }

        SelectedCharacter.Set(characterData);

        bool shouldShowTutorial =
            alwaysShowTutorialDuringDevelopment ||
            !TutorialProgress.IsCompleted;

        SceneManager.LoadScene(
            shouldShowTutorial
                ? tutorialSceneName
                : gameplaySceneName
        );
    }
}
