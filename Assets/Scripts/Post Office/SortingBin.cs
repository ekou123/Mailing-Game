using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

// Drop packages in to sort them. Click it with empty hands to bag the contents into a crate
// (happens automatically when it's full). The master client keeps the count; everyone else mirrors it.
// Needs: PhotonView, and a collider on the Bin layer.
[RequireComponent(typeof(PhotonView))]
public class SortingBin : MonoBehaviourPunCallbacks, IDropTarget, IDragSource
{
    public string district;
    public TextMeshPro binLabel;

    [Header("Crates")]
    [SerializeField] private string cratePrefabPath = "Prefabs/Crate"; // relative to Resources
    [SerializeField] private Transform crateSpawnPoint;
    [SerializeField] private int crateCapacity = 10;

    private int parcels;
    private int misrouted;

    private string StateKey => $"bin.{photonView.ViewID}";

    void Start()
    {
        if (Net.TryGetRoomProperty(StateKey, out int[] state))
            ApplyState(state);

        RefreshLabel();
    }

    public void Receive(IDraggable obj)
    {
        // Only the holder owns the package, and only the owner can remove it
        if (obj is not Package package || !package.IsMine) return;

        bool correct = package.district == district;
        package.NetworkDestroy();

        if (Net.Online) photonView.RPC(nameof(RPC_AddParcel), RpcTarget.MasterClient, correct);
        else RPC_AddParcel(correct);
    }

    public void TakeItem()
    {
        if (Net.Online) photonView.RPC(nameof(RPC_BagUp), RpcTarget.MasterClient);
        else RPC_BagUp();
    }

    [PunRPC]
    void RPC_AddParcel(bool correct)
    {
        if (!Net.IsAuthority) return;

        parcels++;
        if (!correct) misrouted++;

        if (parcels >= crateCapacity) BagUp();
        else Publish();
    }

    [PunRPC]
    void RPC_BagUp()
    {
        if (!Net.IsAuthority || parcels == 0) return;
        BagUp();
    }

    void BagUp()
    {
        Transform point = crateSpawnPoint != null ? crateSpawnPoint : transform;
        MailCrate.Spawn(cratePrefabPath, point.position, point.rotation, new CrateContents(district, parcels, misrouted));

        parcels = 0;
        misrouted = 0;
        Publish();
    }

    void Publish()
    {
        Net.SetRoomProperty(StateKey, new[] { parcels, misrouted });
        RefreshLabel();
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        // The master already has the newest values; applying its own echoes could undo a later change
        if (Net.IsAuthority) return;

        if (changed.TryGetValue(StateKey, out object value) && value is int[] state)
        {
            ApplyState(state);
            RefreshLabel();
        }
    }

    void ApplyState(int[] state)
    {
        parcels = state[0];
        misrouted = state[1];
    }

    void RefreshLabel()
    {
        if (binLabel != null)
            binLabel.text = $"{district}\n{parcels}/{crateCapacity}";
    }
}
