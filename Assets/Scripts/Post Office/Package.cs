using Photon.Pun;
using UnityEngine;
using TMPro;

public class Package : MonoBehaviourPun, IDraggable, IPunInstantiateMagicCallback
{
    [Header("Label Info")]
    public string recipientName;
    public string streetAddress;
    public string district;

    public TextMeshPro labelText;

    private string rawLabel;
    private float refreshTimer;
    public float refreshInterval = 0.8f;

    private Rigidbody rb;
    private bool prefabKinematic;

    public Transform Transform => transform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) prefabKinematic = rb.isKinematic;
    }

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

    void Update()
    {
        // Only the owner simulates physics; everyone else follows the synced transform
        if (rb != null && PhotonNetwork.IsConnected)
            rb.isKinematic = prefabKinematic || !photonView.IsMine;

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

    public void OnPickUp()
    {
        // Take ownership so our drag movement is what gets synced to other players
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
            photonView.RequestOwnership();
    }

    public void OnDrop() { }
}
