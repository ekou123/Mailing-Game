using TMPro;
using UnityEngine;

public class WorldInteractionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private Transform _cam;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);

        gameObject.SetActive(false);
    }

    private void Start()
    {
        _cam = Camera.main != null ? Camera.main.transform : null;
    }

    private void LateUpdate()
    {
        if (_cam == null)
        {
            _cam = Camera.main != null ? Camera.main.transform : null;
            return;
        }

        transform.forward = _cam.position - transform.position;
    }

    public void Show(string text)
    {
        if (label != null)
            label.text = text;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
