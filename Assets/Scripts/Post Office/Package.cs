using Photon.Pun;
using UnityEngine;
using TMPro;

public class Package : NetworkDraggable, IPunInstantiateMagicCallback
{
    [Header("Label Info")]
    public string recipientName;
    public string streetAddress;
    public string district;

    public TextMeshPro labelText;

    private string rawLabel;
    private float refreshTimer;
    public float refreshInterval = 0.8f;

    // Runs on every client when PackageSpawner creates this package, before Start
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        if (data == null || data.Length < 3) return;

        recipientName = (string)data[0];
        streetAddress = (string)data[1];
        district = (string)data[2];
    }

    void Start()
    {
        rawLabel = $"{recipientName}\n{streetAddress}\n{district}";
        RefreshLabel();
    }

    protected override void Update()
    {
        base.Update();

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

        ConditionEffect condition = ConditionEffect.Local;
        labelText.text = condition != null
            ? condition.ProcessLabel(rawLabel)
            : rawLabel;
    }
}
