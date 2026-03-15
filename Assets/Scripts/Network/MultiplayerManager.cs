using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerManager Instance;


    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
        
    }
    
    private void Start() 
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer != null)
        {
            Debug.Log("Setting up board for both players");
            InstantiateBoard();
        }
    }

    public void InstantiateBoard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //player2Button.gameObject.SetActive(false);
            CreatePlayerObjects();
        }
        else
        {
            //player1Button.gameObject.SetActive(false);
            CreatePlayerObjects();
        }
    }



    public override void OnEnable() {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("OnEnable: Scene loaded callback registered.");
    }

    public override void OnDisable() {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("OnDisable: Scene loaded callback deregistered."); 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
    }

    private void CreatePlayerObjects()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer != null)
        {
            Debug.Log($"Instantiating PlayerObject for Player {PhotonNetwork.LocalPlayer.ActorNumber}");
            //GameObject playerManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero,Quaternion.identity);
            //GameObject playerController = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), Vector3.zero, Quaternion.identity);
            GameObject playerObject = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player"), new Vector3(17.18f,5,-42.122f),Quaternion.identity);
        }
    }
}
