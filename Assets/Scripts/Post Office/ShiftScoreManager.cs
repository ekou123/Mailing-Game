using UnityEngine;
using TMPro;

public class ShiftScoreManager : MonoBehaviour
{
    public static ShiftScoreManager Instance { get; private set; }
    public TextMeshProUGUI scoreUI;

    int correct, wrong, missed;

    void Awake() => Instance = this;

    public void RegisterSort(bool wasCorrect)
    {
        if (wasCorrect) correct++; else wrong++;
        UpdateUI();
    }

    public void RegisterMissed()
    {
        missed++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreUI != null)
            scoreUI.text = $"✓ {correct}   ✗ {wrong}   missed {missed}";
    }
}