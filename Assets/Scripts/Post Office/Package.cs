using UnityEngine;
using TMPro;

public class Package : MonoBehaviour, IDraggable
{
    [Header("Label Info")]
    public string recipientName;
    public string streetAddress;
    public string district;

    public TextMeshPro labelText;

    private string rawLabel;
    private ConditionEffect playerCondition;
    private float refreshTimer;
    public float refreshInterval = 0.8f;

    public Transform Transform => transform;

    void Start()
    {
        rawLabel = $"{recipientName}\n{streetAddress}\n{district}";

        // Grab whatever condition effect the player currently has
        playerCondition = FindObjectOfType<ConditionEffect>();

        RefreshLabel();
    }

    void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            RefreshLabel();
        }
    }

    
    

    void RefreshLabel()
    {
        if (labelText == null) return;

        labelText.text = playerCondition != null
            ? playerCondition.ProcessLabel(rawLabel)
            : rawLabel;
    }

    public void OnPickUp() { }
    public void OnDrop() { }    
}