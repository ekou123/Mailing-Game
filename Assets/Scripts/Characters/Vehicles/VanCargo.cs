using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

// The van's load. Drop a crate on the back doors to load it; click the doors to unload.
// Crates come out last-in-first-out, so load your last stop first.
// Crates are stored as data rather than physical objects so they can't fall out while driving.
// Put this on the same object as the Vehicle and its PhotonView. The back doors need a
// collider (trigger is fine) on the Bin layer, which can be on a child object.
[RequireComponent(typeof(Vehicle), typeof(PhotonView))]
public class VanCargo : MonoBehaviourPunCallbacks, IDropTarget, IDragSource
{
    [SerializeField] private int capacity = 6;
    [SerializeField] private string cratePrefabPath = "Prefabs/Crate"; // relative to Resources
    [SerializeField] private Transform unloadPoint;                    // just behind the back doors
    [SerializeField] private TextMeshPro cargoLabel;

    private readonly List<CrateContents> crates = new(); // last entry is at the doors

    private string StateKey => $"van.{photonView.ViewID}";

    void Start()
    {
        if (Net.TryGetRoomProperty(StateKey, out string[] state))
            ApplyState(state);

        RefreshLabel();
    }

    public void Receive(IDraggable obj)
    {
        if (obj is not MailCrate crate || !crate.IsMine) return;

        if (crates.Count >= capacity)
        {
            Debug.Log("The van is full.");
            return;
        }

        string contents = crate.Contents.Encode();
        crate.NetworkDestroy();

        if (Net.Online) photonView.RPC(nameof(RPC_Load), RpcTarget.MasterClient, contents);
        else RPC_Load(contents);
    }

    public void TakeItem()
    {
        if (Net.Online) photonView.RPC(nameof(RPC_Unload), RpcTarget.MasterClient);
        else RPC_Unload();
    }

    [PunRPC]
    void RPC_Load(string encoded)
    {
        if (!Net.IsAuthority) return;

        CrateContents contents = CrateContents.Decode(encoded);

        // Two players loaded the last slot at once; give the extra crate back rather than losing it
        if (crates.Count >= capacity)
        {
            SpawnAtDoors(contents);
            return;
        }

        crates.Add(contents);
        Publish();
    }

    [PunRPC]
    void RPC_Unload()
    {
        if (!Net.IsAuthority || crates.Count == 0) return;

        CrateContents top = crates[crates.Count - 1];
        crates.RemoveAt(crates.Count - 1);
        SpawnAtDoors(top);
        Publish();
    }

    void SpawnAtDoors(CrateContents contents)
    {
        Transform point = unloadPoint != null ? unloadPoint : transform;
        MailCrate.Spawn(cratePrefabPath, point.position, point.rotation, contents);
    }

    void Publish()
    {
        Net.SetRoomProperty(StateKey, crates.Select(c => c.Encode()).ToArray());
        RefreshLabel();
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        // The master already has the newest values; applying its own echoes could undo a later change
        if (Net.IsAuthority) return;

        if (changed.TryGetValue(StateKey, out object value) && value is string[] state)
        {
            ApplyState(state);
            RefreshLabel();
        }
    }

    void ApplyState(string[] state)
    {
        crates.Clear();
        crates.AddRange(state.Select(CrateContents.Decode));
    }

    void RefreshLabel()
    {
        if (cargoLabel == null) return;

        cargoLabel.text = crates.Count == 0
            ? $"Empty (0/{capacity})"
            : $"{crates.Count}/{capacity} crates\nNext out: {crates[crates.Count - 1].district}";
    }
}
