using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

using Photon.Realtime;
using Photon.Pun;

public class RaceLauncher : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerNameInput;

    byte maxPlayersPerRoom = 4;
    bool isConnecting;
    public TextMeshProUGUI networkText;
    string gameVersion = "2";

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            networkText.text += "Connected to Master Server\n";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        networkText.text += "Failed to join a random room. Creating a new room.\n";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        networkText.text = "Joined room with " + PhotonNetwork.CurrentRoom.PlayerCount + " players\n";

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        networkText.text += "Disconnected because " + cause.ToString() + "\n";
        isConnecting = false;
    }

    public void ConnectToNetwork()
    {
        networkText.text = "";
        isConnecting = true;
        PhotonNetwork.NickName = playerNameInput.text;

        if (PhotonNetwork.IsConnected)
        {
            networkText.text += "Joining room...\n";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            networkText.text += "Connecting...\n";
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void Awake()
    {
        Application.runInBackground = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        
        if (PlayerPrefs.HasKey("PlayerName") && playerNameInput != null)
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SetName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
    }

    // WAŻNE: numer w LoadScene powinien być taki sam, jak indeks sceny gry w ustawieniach Build Settings
    public void StartTrial()
    {
        SceneManager.LoadScene(1); // Upewnij się, że scena "Game" ma indeks 1. Jeśli ma 0, zmień na 0.
    }
}
