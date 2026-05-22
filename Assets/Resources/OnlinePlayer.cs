using Photon.Pun;
using UnityEngine;

public class OnlinePlayer : MonoBehaviourPunCallbacks
{
    public static GameObject LocalPlayerInstance;

    void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            ConfigureLocalPlayer();
        }

        ApplyInstantiationData();

        if (!photonView.IsMine)
        {
            ConfigureRemotePlayer();
        }
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            ConfigureLocalPlayer();
        }
    }

    void OnDestroy()
    {
        if (LocalPlayerInstance == gameObject)
        {
            LocalPlayerInstance = null;
        }
    }

    void ConfigureLocalPlayer()
    {
        LocalPlayerInstance = gameObject;

        DrivingScript drivingScript = GetComponent<DrivingScript>();
        if (drivingScript != null)
        {
            drivingScript.enabled = true;

            if (drivingScript.engineSound != null)
            {
                drivingScript.engineSound.mute = false;
            }
        }

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        CarAppearance appearance = GetComponentInChildren<CarAppearance>();
        if (appearance != null)
        {
            appearance.SetLocalPlayer();
        }

        CameraController cameraController = FindAnyObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.SetCameraProperties(gameObject);
        }
    }

    void ConfigureRemotePlayer()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        DrivingScript drivingScript = GetComponent<DrivingScript>();
        if (drivingScript != null)
        {
            if (drivingScript.engineSound != null)
            {
                drivingScript.engineSound.Stop();
                drivingScript.engineSound.mute = true;
            }

            drivingScript.enabled = false;
        }
    }

    void ApplyInstantiationData()
    {
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length >= 4)
        {
            string playerName = photonView.InstantiationData[0] as string;
            Color playerColor = ColorCar.IntToColor(
                (int)photonView.InstantiationData[1],
                (int)photonView.InstantiationData[2],
                (int)photonView.InstantiationData[3]);

            CarAppearance appearance = GetComponentInChildren<CarAppearance>();
            if (appearance != null && !string.IsNullOrWhiteSpace(playerName))
            {
                appearance.SetNameAndColor(playerName, playerColor);
            }
        }
    }
}
