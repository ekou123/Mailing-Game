using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Points (optional)")]
    [SerializeField] private Transform[] spawnPoints;

    private GameObject localPlayerInstance;

    public override void OnJoinedRoom()
    {
        // Prevent double-spawns if something calls this twice or scene reloads weirdly
        if (localPlayerInstance != null) return;

        Vector3 pos = GetSpawnPosition();
        Quaternion rot = Quaternion.identity;

        // "Player" must match the prefab name in Resources
        localPlayerInstance = PhotonNetwork.Instantiate("Player", pos, rot);
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Vector3.zero;

        // Simple: spread players across spawn points by ActorNumber
        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        return spawnPoints[index].position;
    }
}