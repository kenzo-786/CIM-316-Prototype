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

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectCharacter);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SelectCharacter);
        }
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
