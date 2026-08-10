using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text instructionText;

    public void Show(string title, string instruction)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (instructionText != null)
        {
            instructionText.text = instruction;
        }
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }
}
