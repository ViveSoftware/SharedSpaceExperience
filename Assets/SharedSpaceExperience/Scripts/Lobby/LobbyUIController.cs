using UnityEngine;
using TMPro;

using Photon.Realtime;

public class LobbyUIController : MonoBehaviour
{
    // Lobby Menu
    [SerializeField]
    private GameObject lobbyMenu;


    // Loading Panel
    [SerializeField]
    private GameObject loadingPanel;
    [SerializeField]
    private TMP_Text loadingMessage;

    [SerializeField]
    private TMP_Dropdown regionDropdown;

    private void Start() {
        TryConnectToServer();
    }

    public void TryConnectToServer(){
        loadingPanel.SetActive(true);
        lobbyMenu.SetActive(false);

        loadingMessage.text = "Connecting to Server...";
    }

    public void OnConnectedToServer(int regionIdx){
        loadingPanel.SetActive(false);
        lobbyMenu.SetActive(true);

        regionDropdown.value = regionIdx;
    }

    public void OnDisconnectedFromServer(DisconnectCause cause){
        if(loadingPanel) loadingPanel.SetActive(true);
        if(lobbyMenu) lobbyMenu.SetActive(false);

        switch(cause){
            case DisconnectCause.DisconnectByClientLogic:
                loadingMessage.text = "Disconnected from the server.";
                break;
            case DisconnectCause.DnsExceptionOnConnect:
                loadingMessage.text = "Failed to connect to the server.\nMake sure you have connected to the Internet.";
                break;
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.InvalidRegion:
                loadingMessage.text = "Failed to connect to the server.\nServer is not available.";
                break;
            default:
                loadingMessage.text = "Failed to connect to the server.";
                break;
        }

    }

    public void OnMatching(){
        loadingPanel.SetActive(true);
        lobbyMenu.SetActive(false);
        
        loadingMessage.text = "Matching...";
    }
}
