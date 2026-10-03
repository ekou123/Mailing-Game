using Photon.Pun;
using UnityEngine;

// Base for things players carry around with the mouse (packages, crates).
// Whoever picks it up becomes its Photon owner, so their movement is what everyone sees,
// and nobody else can grab it until they let go.
// Prefab needs: PhotonView (Ownership Transfer = Takeover) observing a PhotonTransformView.
[RequireComponent(typeof(PhotonView))]
public abstract class NetworkDraggable : MonoBehaviourPun, IDraggable
{
    // How long to wait for the server to hand us ownership before assuming someone beat us to it
    private const float OwnershipTimeout = 1.5f;

    private Rigidbody rb;
    private bool prefabKinematic;
    private bool heldLocally;
    private float pickUpTime;
    private int remoteHolder; // actor number of another player holding this, 0 if nobody

    public Transform Transform => transform;
    public bool IsHeldByLocalPlayer => heldLocally;

    // Only the owner may sort, load or destroy this
    public bool IsMine => !Net.Online || photonView.IsMine;

    // Only trust the holder while they're still the owner (ownership reverts if they leave the room)
    private bool IsHeldByOther =>
        Net.Online && remoteHolder != 0 && remoteHolder == photonView.OwnerActorNr && !photonView.AmOwner;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) prefabKinematic = rb.isKinematic;
    }

    protected virtual void Update()
    {
        // Someone else's ownership request landed first
        if (heldLocally && Net.Online && !photonView.AmOwner && Time.time - pickUpTime > OwnershipTimeout)
            heldLocally = false;

        // Only the owner simulates physics; everyone else follows the synced transform.
        // Held objects are moved by hand, so gravity shouldn't pull on them either.
        bool kinematic = prefabKinematic || heldLocally || !IsMine;
        if (rb != null && rb.isKinematic != kinematic)
            rb.isKinematic = kinematic;
    }

    public bool TryPickUp()
    {
        if (IsHeldByOther) return false;

        heldLocally = true;
        pickUpTime = Time.time;

        if (Net.Online)
        {
            // Sent in this order so everyone applies the ownership change before the "held" flag
            if (!photonView.AmOwner) photonView.RequestOwnership();
            photonView.RPC(nameof(RPC_SetHeld), RpcTarget.Others, true);
        }
        return true;
    }

    public void OnDrop()
    {
        if (!heldLocally) return;
        heldLocally = false;

        if (Net.Online)
            photonView.RPC(nameof(RPC_SetHeld), RpcTarget.Others, false);
    }

    // Removes this for every player. Callers check IsMine first, because Photon only lets the owner do it.
    public void NetworkDestroy()
    {
        if (Net.Online) PhotonNetwork.Destroy(gameObject);
        else Destroy(gameObject);
    }

    // Public so Photon finds it on subclasses like Package and MailCrate
    [PunRPC]
    public void RPC_SetHeld(bool held, PhotonMessageInfo info)
    {
        int sender = info.Sender.ActorNumber;
        if (held) remoteHolder = sender;
        else if (remoteHolder == sender) remoteHolder = 0;
    }
}
