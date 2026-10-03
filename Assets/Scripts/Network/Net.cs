using ExitGames.Client.Photon;
using Photon.Pun;

// Small helpers so gameplay scripts work both online and when a scene is played offline for testing.
public static class Net
{
    // True when playing in a Photon room; false when testing a scene without Photon
    public static bool Online => PhotonNetwork.InRoom;

    // The client that makes decisions for shared objects: the master client, or us when offline
    public static bool IsAuthority => !Online || PhotonNetwork.IsMasterClient;

    // Shared state lives in room properties, so players who join late get the current values
    public static void SetRoomProperty(string key, object value)
    {
        if (!Online) return;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { key, value } });
    }

    public static bool TryGetRoomProperty<T>(string key, out T value)
    {
        value = default;
        if (!Online || !PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object raw) || raw is not T typed)
            return false;

        value = typed;
        return true;
    }
}
