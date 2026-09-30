using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;

        if (promptRoot == null)
            promptRoot = gameObject;

        if (promptText == null)
            promptText = GetComponentInChildren<TextMeshProUGUI>(true);

        Hide();
    }

    public void Show(string message)
    {
        if (promptRoot != null)
            promptRoot.SetActive(true);

        if (promptText != null)
            promptText.text = message;
    }

    public void Hide()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }
}