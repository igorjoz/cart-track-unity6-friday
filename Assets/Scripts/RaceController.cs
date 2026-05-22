using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using Photon.Realtime;
using Photon.Pun;

public class RaceController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public const byte CarStateEventCode = 42;
    public const byte RaceStartEventCode = 43;

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
    bool spawnStarted;
    readonly Dictionary<int, GameObject> fallbackRemoteCars = new Dictionary<int, GameObject>();

    [PunRPC]
    public void StartGame()
    {
        if (isRacing || IsInvoking("CountDown"))
        {
            return;
        }

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
        if (!PhotonNetwork.InRoom)
        {
            StartGame();
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Usunięty zbędny "null" dla bezpieczniejszego RPC
            StartGame();
            PhotonNetwork.RaiseEvent(
                RaceStartEventCode,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                SendOptions.SendReliable);
            return;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (PhotonNetwork.IsMasterClient)
        {
            Transform spawnPoint = GetSpawnPointForActor(newPlayer.ActorNumber);
            EnsureFallbackRemoteCar(newPlayer.ActorNumber, spawnPoint.position, spawnPoint.rotation, newPlayer.NickName, Color.white);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerCount = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 1;

        if (fallbackRemoteCars.TryGetValue(otherPlayer.ActorNumber, out GameObject fallbackCar) && fallbackCar != null)
        {
            Destroy(fallbackCar);
        }

        fallbackRemoteCars.Remove(otherPlayer.ActorNumber);
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

    void Update()
    {
        UpdateStartControls();

        if (!CanLocalPlayerStartRace() || isRacing || IsInvoking("CountDown"))
        {
            return;
        }

        if (WasStartInputPressed())
        {
            BeginRace();
        }
    }

    void Start()
    {
        Application.runInBackground = true;
        PhotonNetwork.IsMessageQueueRunning = true;
        isRacing = false;
        timer = 3;

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

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            UpdateStartControls();

            StartCoroutine(SpawnNetworkCarWhenReady());
        }
        else
        {
            startRace.SetActive(true);
            waitingText.SetActive(false);

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            ConfigureLocalCar(Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation));
        }
    }

    bool CanLocalPlayerStartRace()
    {
        return !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
    }

    void UpdateStartControls()
    {
        if (startRace != null)
        {
            startRace.SetActive(CanLocalPlayerStartRace() && !isRacing && !IsInvoking("CountDown"));
        }

        if (waitingText != null)
        {
            waitingText.SetActive(PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient && !isRacing && !IsInvoking("CountDown"));
        }
    }

    bool WasStartInputPressed()
    {
        return WasConfirmKeyPressed() || WasStartButtonClicked();
    }

    bool WasConfirmKeyPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            return true;
        }
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.enterKey.wasPressedThisFrame ||
                   keyboard.numpadEnterKey.wasPressedThisFrame ||
                   keyboard.spaceKey.wasPressedThisFrame;
        }
#endif

        return false;
    }

    bool WasStartButtonClicked()
    {
        if (startRace == null || !startRace.activeInHierarchy || !WasPrimaryPointerPressed())
        {
            return false;
        }

        // W buildzie samo klikniecie czasem nie dochodzilo do UI Buttona.
        // Jesli przycisk startu jest widoczny, klik mysza mastera traktujemy jako start.
        return true;
    }

    bool WasPrimaryPointerPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }
#endif

#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            return true;
        }
#endif

        return false;
    }

    IEnumerator SpawnNetworkCarWhenReady()
    {
        if (spawnStarted)
        {
            yield break;
        }

        spawnStarted = true;

        while (!PhotonNetwork.InRoom || !PhotonNetwork.IsMessageQueueRunning)
        {
            yield return null;
        }

        yield return null;

        GameObject playerCar = OnlinePlayer.LocalPlayerInstance;

        if (playerCar == null)
        {
            Transform spawnPoint = GetSpawnPointForActor(PhotonNetwork.LocalPlayer.ActorNumber);
            object[] instanceData = new object[4];
            instanceData[0] = PlayerPrefs.GetString("PlayerName");
            instanceData[1] = PlayerPrefs.GetInt("Red");
            instanceData[2] = PlayerPrefs.GetInt("Green");
            instanceData[3] = PlayerPrefs.GetInt("Blue");

            playerCar = PhotonNetwork.Instantiate(carPrefab.name, spawnPoint.position, spawnPoint.rotation, 0, instanceData);
            PhotonNetwork.SendAllOutgoingCommands();
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
                drivingScript.engineSound.loop = true;
                if (!drivingScript.engineSound.isPlaying)
                {
                    drivingScript.engineSound.Play();
                }
            }

            if (drivingScript.rb != null)
            {
                drivingScript.rb.isKinematic = false;
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

    [PunRPC]
    void SyncRemoteCarState(int actorNumber, Vector3 position, Quaternion rotation, string playerName, float red, float green, float blue)
    {
        ApplyRemoteCarState(actorNumber, position, rotation, playerName, new Color(red, green, blue));
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == RaceStartEventCode)
        {
            StartGame();
            return;
        }

        if (photonEvent.Code != CarStateEventCode)
        {
            return;
        }

        object[] data = photonEvent.CustomData as object[];
        if (data == null || data.Length < 12)
        {
            return;
        }

        int actorNumber = (int)data[0];
        Vector3 position = new Vector3((float)data[1], (float)data[2], (float)data[3]);
        Quaternion rotation = new Quaternion((float)data[4], (float)data[5], (float)data[6], (float)data[7]);
        string playerName = data[8] as string;
        Color playerColor = new Color((float)data[9], (float)data[10], (float)data[11]);

        ApplyRemoteCarState(actorNumber, position, rotation, playerName, playerColor);
    }

    void ApplyRemoteCarState(int actorNumber, Vector3 position, Quaternion rotation, string playerName, Color playerColor)
    {
        if (!PhotonNetwork.InRoom || actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            return;
        }

        OnlinePlayer networkCar = FindNetworkCarForActor(actorNumber);
        if (networkCar != null)
        {
            if (fallbackRemoteCars.TryGetValue(actorNumber, out GameObject oldFallback) && oldFallback != null)
            {
                Destroy(oldFallback);
            }

            fallbackRemoteCars.Remove(actorNumber);
            SetFallbackAppearance(networkCar.gameObject, playerName, playerColor);
            SetCarTransform(networkCar.gameObject, position, rotation);
            return;
        }

        GameObject fallbackCar = EnsureFallbackRemoteCar(actorNumber, position, rotation, playerName, playerColor);

        if (fallbackCar != null)
        {
            SetFallbackAppearance(fallbackCar, playerName, playerColor);
            SetCarTransform(fallbackCar, position, rotation);
        }
    }

    OnlinePlayer FindNetworkCarForActor(int actorNumber)
    {
        OnlinePlayer[] players = FindObjectsByType<OnlinePlayer>(FindObjectsInactive.Exclude);
        foreach (OnlinePlayer player in players)
        {
            if (player.ActorNumber == actorNumber)
            {
                return player;
            }
        }

        return null;
    }

    Transform GetSpawnPointForActor(int actorNumber)
    {
        int spawnIndex = Mathf.Abs(actorNumber - 1) % spawnPoints.Length;
        return spawnPoints[spawnIndex];
    }

    GameObject EnsureFallbackRemoteCar(int actorNumber, Vector3 position, Quaternion rotation, string playerName, Color playerColor)
    {
        if (carPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }

        OnlinePlayer networkCar = FindNetworkCarForActor(actorNumber);
        if (networkCar != null)
        {
            return null;
        }

        if (!fallbackRemoteCars.TryGetValue(actorNumber, out GameObject fallbackCar) || fallbackCar == null)
        {
            fallbackCar = Instantiate(carPrefab, position, rotation);
            fallbackRemoteCars[actorNumber] = fallbackCar;
            ConfigureFallbackRemoteCar(fallbackCar, playerName, playerColor);
        }

        return fallbackCar;
    }

    void ConfigureFallbackRemoteCar(GameObject car, string playerName, Color playerColor)
    {
        PlayerController playerController = car.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        DrivingScript drivingScript = car.GetComponent<DrivingScript>();
        if (drivingScript != null)
        {
            if (drivingScript.rb != null)
            {
                drivingScript.rb.isKinematic = true;
            }

            if (drivingScript.engineSound != null)
            {
                drivingScript.engineSound.Stop();
                drivingScript.engineSound.mute = true;
            }

            drivingScript.enabled = false;
        }

        OnlinePlayer onlinePlayer = car.GetComponent<OnlinePlayer>();
        if (onlinePlayer != null)
        {
            onlinePlayer.enabled = false;
            Destroy(onlinePlayer);
        }

        PhotonRigidbodyView[] rigidbodyViews = car.GetComponentsInChildren<PhotonRigidbodyView>();
        foreach (PhotonRigidbodyView view in rigidbodyViews)
        {
            view.enabled = false;
            Destroy(view);
        }

        PhotonView[] photonViews = car.GetComponentsInChildren<PhotonView>();
        foreach (PhotonView view in photonViews)
        {
            view.enabled = false;
            Destroy(view);
        }

        PhotonTransformView[] transformViews = car.GetComponentsInChildren<PhotonTransformView>();
        foreach (PhotonTransformView view in transformViews)
        {
            view.enabled = false;
            Destroy(view);
        }

        SetFallbackAppearance(car, playerName, playerColor);
    }

    void SetFallbackAppearance(GameObject car, string playerName, Color playerColor)
    {
        CarAppearance appearance = car.GetComponentInChildren<CarAppearance>();
        if (appearance != null)
        {
            appearance.SetNameAndColor(playerName, playerColor);
        }
    }

    void SetCarTransform(GameObject car, Vector3 position, Quaternion rotation)
    {
        DrivingScript drivingScript = car.GetComponent<DrivingScript>();
        if (drivingScript != null && drivingScript.rb != null)
        {
            drivingScript.rb.position = position;
            drivingScript.rb.rotation = rotation;
            if (!drivingScript.rb.isKinematic)
            {
                drivingScript.rb.linearVelocity = Vector3.zero;
                drivingScript.rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        car.transform.SetPositionAndRotation(position, rotation);
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
