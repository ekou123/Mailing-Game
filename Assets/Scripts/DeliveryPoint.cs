using System;
using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

// Where a district's mail gets handed in. Drop a crate for this district on it to deliver it.
// Needs: PhotonView, and a collider on the Bin layer.
[RequireComponent(typeof(PhotonView))]
public class DeliveryPoint : MonoBehaviourPun, IDropTarget
{
    public string district;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private float messageDuration = 3f;

    // Raised on the master client (or offline) for every crate handed in. Hook pay, Heat, etc. in here later.
    public static event Action<DeliveryPoint, CrateContents> CrateDelivered;

    private Coroutine messageRoutine;

    void Start() => ShowDistrictName();

    public void Receive(IDraggable obj)
    {
        if (obj is not MailCrate crate || !crate.IsMine) return;

        if (crate.Contents.district != district)
        {
            ShowMessage($"Wrong stop!\nThis crate is for {crate.Contents.district}");
            return;
        }

        string contents = crate.Contents.Encode();
        crate.NetworkDestroy();

        if (Net.Online) photonView.RPC(nameof(RPC_Delivered), RpcTarget.All, contents);
        else RPC_Delivered(contents);
    }

    [PunRPC]
    void RPC_Delivered(string encoded)
    {
        CrateContents contents = CrateContents.Decode(encoded);

        ShowMessage(contents.misrouted > 0
            ? $"Delivered {contents.CorrectlyAddressed} parcels\n{contents.misrouted} went to the wrong address"
            : $"Delivered {contents.parcels} parcels");

        if (Net.IsAuthority)
            CrateDelivered?.Invoke(this, contents);
    }

    void ShowMessage(string message)
    {
        if (label == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    IEnumerator MessageRoutine(string message)
    {
        label.text = message;
        yield return new WaitForSeconds(messageDuration);
        ShowDistrictName();
        messageRoutine = null;
    }

    void ShowDistrictName()
    {
        if (label != null) label.text = district;
    }
}
