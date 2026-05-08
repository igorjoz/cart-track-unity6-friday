using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

using Photon.Realtime;
using Photon.Pun;

public class RaceController : MonoBehaviourPunCallbacks
{
    public static bool isRacing = false;
    public static int totalLaps = 1;
    public int timer = 3;

    public CheckPointController[] carControllers;

    public TextMeshProUGUI startText;
    AudioSource audioSource;
    public AudioClip count;
    public AudioClip start;
    public GameObject endPanel;

    public int playerCount;
    public Transform[] spawnPoints;
    public GameObject carPrefab;

    public GameObject startRace;
    public GameObject waitingText;

    [PunRPC]
    public void StartGame()
    {
        InvokeRepeating("CountDown", 3, 1);
        startRace.SetActive(false);
        waitingText.SetActive(false);

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        carControllers = new CheckPointController[cars.Length];

        for (int i = 0; i < cars.Length; i++)
        {
            carControllers[i] = cars[i].GetComponent<CheckPointController>();
        }
    }

    public void BeginRace()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame", RpcTarget.All, null);
        }
    }

    void CountDown()
    {
        if (startText != null) startText.gameObject.SetActive(true);

        if (timer > 0)
        {
            if (startText != null) startText.text = timer.ToString();
            if (audioSource != null && count != null) audioSource.PlayOneShot(count);
            timer--;
        }
        else
        {
            if (startText != null) startText.text = "START!!!";
            if (audioSource != null && start != null) audioSource.PlayOneShot(start);
            isRacing = true;
            CancelInvoke("CountDown");
            Invoke("HideStartText", 1f);
        }
    }

    void HideStartText()
    {
        if (startText != null) startText.gameObject.SetActive(false);
    }

    void Start()
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        endPanel.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        startText.gameObject.SetActive(false);
        startRace.SetActive(false);
        waitingText.SetActive(false);
        int randomStartPosition = Random.Range(0, spawnPoints.Length);
        Vector3 startPos = spawnPoints[randomStartPosition].position;
        Quaternion startRot = spawnPoints[randomStartPosition].rotation;
        GameObject playerCar = null;
        if (PhotonNetwork.IsConnected)
        {
            startPos = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].position;
            startRot = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].rotation;
            object[] instanceData = new object[4];
            instanceData[0] = (string)PlayerPrefs.GetString("PlayerName");
            instanceData[1] = PlayerPrefs.GetInt("Red");
            instanceData[2] = PlayerPrefs.GetInt("Green");
            instanceData[3] = PlayerPrefs.GetInt("Blue");
            if (OnlinePlayer.LocalPlayerInstance == null)
            {
                playerCar = PhotonNetwork.Instantiate(carPrefab.name, startPos, startRot, 0, instanceData);

                playerCar.GetComponent<CarAppearance>().SetLocalPlayer();
            }
            if (PhotonNetwork.IsMasterClient)
            {
                startRace.SetActive(true);
            }
            else
            {
                waitingText.SetActive(true);
            }
        }
        playerCar.GetComponent<DrivingScript>().enabled = true;
        playerCar.GetComponent<PlayerController>().enabled = true;
    }

    //audioSource = GetComponent<AudioSource>();
    //if (startText != null) startText.gameObject.SetActive(false);
    //if (endPanel != null) endPanel.SetActive(false);

    ////InvokeRepeating("CountDown", 3, 1);

    //for (int i = 0; i < playerCount; i++)
    //{
    //    GameObject car = Instantiate(carPrefab);
    //    car.transform.position = spawnPoints[i].position;
    //    car.transform.rotation = spawnPoints[i].rotation;

    //    CarAppearance appearance = car.GetComponentInChildren<CarAppearance>();
    //    if (appearance != null)
    //    {
    //        appearance.playerNumber = i;
    //    }

    //    if (i == 0)
    //    {
    //        PlayerController playerController = car.GetComponent<PlayerController>();
    //        if (playerController != null)
    //        {
    //            playerController.enabled = true;
    //        }

    //        CameraController camController = GameObject.FindFirstObjectByType<CameraController>();
    //        if (camController != null)
    //        {
    //            camController.SetCameraProperties(car);
    //        }
    //    }
    //}

    //GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

    //carControllers = new CheckPointController[cars.Length];

    //for (int i = 0; i < cars.Length; i++)
    //{
    //    carControllers[i] = cars[i].GetComponent<CheckPointController>();
    //}
//}

    private void LateUpdate()
    {
        int carsThatCompletedRace = 0;

        foreach (CheckPointController controller in carControllers)
        {
            if (controller.lap == totalLaps + 1)
            {
                carsThatCompletedRace++;
            }

            if (carsThatCompletedRace == carControllers.Length && isRacing)
            {
                if (endPanel != null) endPanel.SetActive(true);
                isRacing = false;
            }
        }
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
