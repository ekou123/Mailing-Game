using Photon.Pun;
using TMPro;
using UnityEngine;

// A crate of sorted mail for one district. Made by bagging up a SortingBin, loaded into the van,
// then carried to that district's DeliveryPoint.
public class MailCrate : NetworkDraggable, IPunInstantiateMagicCallback
{
    public TextMeshPro label;

    public CrateContents Contents { get; private set; }

    // Master client only (or offline). Room objects survive the player who made them leaving.
    public static void Spawn(string prefabPath, Vector3 position, Quaternion rotation, CrateContents contents)
    {
        if (Net.Online)
        {
            PhotonNetwork.InstantiateRoomObject(prefabPath, position, rotation, 0, new object[] { contents.Encode() });
        }
        else
        {
            MailCrate crate = Instantiate(Resources.Load<MailCrate>(prefabPath), position, rotation);
            crate.SetContents(contents);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        if (data != null && data.Length > 0)
            SetContents(CrateContents.Decode((string)data[0]));
    }

    void SetContents(CrateContents contents)
    {
        Contents = contents;
        if (label != null)
            label.text = $"{contents.district}\n{contents.parcels} parcels";
    }
}
