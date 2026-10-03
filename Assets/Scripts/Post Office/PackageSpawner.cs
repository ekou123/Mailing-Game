using System.Collections;
using Photon.Pun;
using UnityEngine;

// Spawns packages on the master client only. They're room objects, so they survive
// the player who spawned them leaving, and ownership passes to the next master.
public class PackageSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawning")]
    [SerializeField] private string packagePrefabPath = "Prefabs/Package"; // relative to Resources
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private int maxPackages = 10;

    [Header("Label Pools")]
    [SerializeField] private string[] recipientNames = { "J. Smith", "M. Garcia", "A. Chen", "R. Okafor" };
    [SerializeField] private string[] streetAddresses = { "12 Elm St", "4 Harbour Rd", "88 Mill Ln", "301 Kings Ave" };
    [SerializeField] private string[] districts = { "North", "South", "East", "West" }; // must match SortingBin districts

    private Coroutine spawnRoutine;

    void Start() => TryStartSpawning();

    public override void OnJoinedRoom() => TryStartSpawning();

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) => TryStartSpawning();

    void TryStartSpawning()
    {
        if (spawnRoutine != null) return;
        if (PhotonNetwork.IsConnected && !(PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)) return;

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            if (ShiftManager.IsShiftActive && FindObjectsOfType<Package>().Length < maxPackages)
                SpawnPackage();
            yield return wait;
        }
    }

    void SpawnPackage()
    {
        // No spawn points set up yet: spawn at the spawner itself
        Transform point = spawnPoints == null || spawnPoints.Length == 0
            ? transform
            : spawnPoints[Random.Range(0, spawnPoints.Length)];
        object[] data =
        {
            recipientNames[Random.Range(0, recipientNames.Length)],
            streetAddresses[Random.Range(0, streetAddresses.Length)],
            districts[Random.Range(0, districts.Length)]
        };

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.InstantiateRoomObject(packagePrefabPath, point.position, point.rotation, 0, data);
        }
        else
        {
            // Offline testing without Photon
            Package package = Instantiate(Resources.Load<Package>(packagePrefabPath), point.position, point.rotation);
            package.recipientName = (string)data[0];
            package.streetAddress = (string)data[1];
            package.district = (string)data[2];
        }
    }
}
