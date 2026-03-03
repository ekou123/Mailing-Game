using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

     public static LobbyManager Instance;

    private void Awake() 
    {
        PV = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    
    void StartGame()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            PhotonNetwork.LoadLevel("Game");
        }
        
    }

    public override void OnJoinedRoom()
    {
        
        Debug.Log("Successfully joined room with Actor: " + PhotonNetwork.LocalPlayer.ActorNumber);
        
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
    }

    public void StartGameForAllPlayers()
    {
        // Only master triggers the scene load; others will follow because AutomaticallySyncScene = true
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Only master client can start game");
            return;
        }

        PhotonNetwork.LoadLevel("Game");
    }



    

}
