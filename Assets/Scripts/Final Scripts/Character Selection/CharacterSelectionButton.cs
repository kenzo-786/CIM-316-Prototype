using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CharacterSelectionButton : MonoBehaviour
{
    [SerializeField] private PlayerCharacterData characterData;
    [SerializeField] private string gameplaySceneName = "Game";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(SelectCharacter);
    }

    private void SelectCharacter()
    {
        SelectedCharacter.Set(characterData);
        SceneManager.LoadScene(gameplaySceneName);
    }
}
