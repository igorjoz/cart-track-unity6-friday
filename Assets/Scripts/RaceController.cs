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
            // Usunięty zbędny "null" dla bezpieczniejszego RPC
            photonView.RPC("StartGame", RpcTarget.All);
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
        playerCount = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 1;

        endPanel.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        startText.gameObject.SetActive(false);
        startRace.SetActive(false);
        waitingText.SetActive(false);

        if (carPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("RaceController nie ma ustawionego carPrefab albo spawnPoints.");
            return;
        }

        int randomStartPosition = Random.Range(0, spawnPoints.Length);
        Vector3 startPos = spawnPoints[randomStartPosition].position;
        Quaternion startRot = spawnPoints[randomStartPosition].rotation;
        GameObject playerCar = null;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            startPos = spawnPoints[spawnIndex].position;
            startRot = spawnPoints[spawnIndex].rotation;

            object[] instanceData = new object[4];
            instanceData[0] = PlayerPrefs.GetString("PlayerName");
            instanceData[1] = PlayerPrefs.GetInt("Red");
            instanceData[2] = PlayerPrefs.GetInt("Green");
            instanceData[3] = PlayerPrefs.GetInt("Blue");

            playerCar = OnlinePlayer.LocalPlayerInstance;

            if (playerCar == null)
            {
                playerCar = PhotonNetwork.Instantiate(carPrefab.name, startPos, startRot, 0, instanceData);
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
        else
        {
            playerCar = Instantiate(carPrefab, startPos, startRot);
        }

        ConfigureLocalCar(playerCar);
    }

    void ConfigureLocalCar(GameObject playerCar)
    {
        if (playerCar == null)
        {
            Debug.LogWarning("Nie znaleziono lokalnego samochodu gracza.");
            return;
        }

        DrivingScript drivingScript = playerCar.GetComponent<DrivingScript>();
        if (drivingScript != null)
        {
            drivingScript.enabled = true;

            if (drivingScript.engineSound != null)
            {
                drivingScript.engineSound.mute = false;
            }
        }

        PlayerController playerController = playerCar.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        CarAppearance appearance = playerCar.GetComponentInChildren<CarAppearance>();
        if (appearance != null)
        {
            appearance.SetLocalPlayer();
        }
        else
        {
            Debug.LogWarning("Nie znaleziono komponentu CarAppearance na samochodzie gracza.");
        }
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
        // Zabezpieczenie przed momentem zanim wywoła się StartGame:
        if (carControllers == null || carControllers.Length == 0) return;

        int carsThatCompletedRace = 0;

        foreach (CheckPointController controller in carControllers)
        {
            if (controller.lap == totalLaps + 1)
            {
                carsThatCompletedRace++;
            }
        } // Wyciągnięty warunek końca gry poza pętlę dla poprawnej struktury

        if (carsThatCompletedRace == carControllers.Length && isRacing)
        {
            if (endPanel != null) endPanel.SetActive(true);
            isRacing = false;
        }
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
