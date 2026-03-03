using UnityEngine;
using TMPro;

public class ConditionEffect : MonoBehaviour
{
    // Override this in each condition to process a label string
    public virtual string ProcessLabel(string originalText)
    {
        return originalText;
    }
}