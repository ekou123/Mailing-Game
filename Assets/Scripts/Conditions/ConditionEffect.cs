using Photon.Pun;
using UnityEngine;

public class ConditionEffect : MonoBehaviourPun
{
    // The condition of the player on this machine. Remote players' conditions stay disabled,
    // so each client renders labels through its own player's condition.
    public static ConditionEffect Local { get; private set; }

    protected virtual void Start()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine) Local = this;
        else enabled = false;
    }

    protected virtual void OnDestroy()
    {
        if (Local == this) Local = null;
    }

    // Override this in each condition to process a label string
    public virtual string ProcessLabel(string originalText)
    {
        return originalText;
    }
}
