using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using SharedSpaceExperience;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private bool joinRandomRoom = true;
    [SerializeField]
    private string debugRoomName = "ShareSpaceExperience";

    private string[] region = { "usw", "us", "cae", "sa", "jp", "asia" };
    public Dictionary<string, int> regionIndices = new Dictionary<string, int>(){
        {"usw",     0},
        {"us",      1},
        {"cae",     2},
        {"sa",      3},
        {"jp",      4},
        {"asia",    5},
    };
    private static string curRegion = "usw";

    private bool tryReconnect = false;

    [SerializeField]
    private LobbyUIController ui;

    [SerializeField]
    private SceneTransition sceneTransition;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        ConnectToServer();
    }


    void OnApplicationFocus(bool focus)
    {
#if !UNITY_EDITOR
        if (focus) ConnectToServer();
        else DisconnectFromServer();
#endif
    }

    private void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    public void ConnectToSpecificRegion(int regionIdx)
    {
        if (region[regionIdx] == curRegion) return;
        curRegion = region[regionIdx];

        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = curRegion;

        if (PhotonNetwork.IsConnected)
        {
            tryReconnect = true;
            DisconnectFromServer();
        }
        else
        {
            ConnectToServer();
        }
    }

    private void ConnectToServer()
    {
        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
            return;
        }
        ui.TryConnectToServer();

        PhotonNetwork.ConnectUsingSettings();
        Logger.Log("[LobbyManager] Try to connect to server");
    }

    public override void OnConnectedToMaster()
    {
        Logger.Log("[LobbyManager] Connected to server at " + PhotonNetwork.CloudRegion);
        ui.OnConnectedToServer(regionIndices[PhotonNetwork.CloudRegion.Split('/')[0]]);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Logger.Log("[LobbyManager] Disconnected from server: " + PhotonNetwork.CloudRegion + " " + cause);

        // UI notify
        ui.OnDisconnectedFromServer(cause);

        if (tryReconnect && cause == DisconnectCause.DisconnectByClientLogic) ConnectToServer();
    }

    private void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
        Logger.Log("[LobbyManager] Try to disconnect from server");
    }

    public void JoinMatch()
    {
        Logger.Log("[LobbyManager] Join or create room");

        ui.OnMatching();

        // Join or create room
        if (joinRandomRoom)
        {
            // join random room
            RoomOptions roomOptions = MatchManager.GetInitialRoomOptions(true);
            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
        }
        else
        {
            // debug mode
            RoomOptions roomOptions = MatchManager.GetInitialRoomOptions(false);
            PhotonNetwork.JoinOrCreateRoom(debugRoomName, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Logger.Log("[LobbyManager] Joined room: " + PhotonNetwork.CurrentRoom.Name);

        // load game scene
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Logger.Log("[LobbyManager] Load game scene");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Logger.Log("[LobbyManager] Join room failed " + returnCode + " " + message);

        // return to menu page
        ConnectToServer();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Logger.Log("[LobbyManager] Join random failed " + returnCode + " " + message);

        // return to menu page
        ConnectToServer();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Logger.Log("[LobbyManager] Create room failed " + returnCode + " " + message);

        // return to menu page
        ConnectToServer();
    }

    public void Quit()
    {
        Logger.Log("[LobbyManager] Quit game");

        Application.Quit();
    }

}
