using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputCreate;
    public TMP_InputField inputJoin;

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(inputCreate.text)) return;
        if (!PhotonNetwork.IsConnectedAndReady) { Debug.Log("Not connected yet"); return; }

        Debug.Log("Realtime AppId: " + PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
        Debug.Log("FixedRegion: " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion);

        var roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsOpen = true,
            IsVisible = true,
            Plugins = null // IMPORTANT: prevents "Unsupported Plugin:" (empty)
        };

        Debug.Log("Creating room " + inputCreate.text);
        PhotonNetwork.CreateRoom(inputCreate.text, roomOptions, TypedLobby.Default, null);
    }
    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(inputJoin.text))
        {
            return;
        }
        Debug.Log("Joining room " + inputJoin.text);
        PhotonNetwork.JoinRoom(inputJoin.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joining room Lobby");
        Debug.Log("Joined room with ActorNumber: " + PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.LoadLevel("Lobby");

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        
        Debug.LogError($"CreateRoom failed. Code={returnCode}, Message={message}");
    }
}
