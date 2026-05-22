using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class OnlinePlayer : MonoBehaviourPunCallbacks
{
    public static GameObject LocalPlayerInstance;
    PhotonView view;
    float nextStateSyncTime;
    bool configured;

    public int ActorNumber => view != null ? view.OwnerActorNr : -1;

    void Awake()
    {
        view = GetComponent<PhotonView>();

        if (view == null)
        {
            enabled = false;
            return;
        }

        ApplyInstantiationData();
    }

    IEnumerator Start()
    {
        while (enabled && PhotonNetwork.InRoom && view != null && view.ViewID == 0)
        {
            yield return null;
        }

        ConfigureForOwnership();
    }

    void Update()
    {
        if (!configured)
        {
            ConfigureForOwnership();
        }

        if (!configured || view == null || !view.IsMine || !PhotonNetwork.InRoom || Time.time < nextStateSyncTime)
        {
            return;
        }

        nextStateSyncTime = Time.time + 0.05f;
        SendCarStateEvent();
    }

    void ConfigureForOwnership()
    {
        if (view == null || (PhotonNetwork.InRoom && view.ViewID == 0))
        {
            return;
        }

        if (view.IsMine)
        {
            LocalPlayerInstance = gameObject;
            ConfigureLocalPlayer();
        }

        if (!view.IsMine)
        {
            ConfigureRemotePlayer();
        }

        configured = true;
    }

    void SendCarStateEvent()
    {
        Transform stateTransform = transform;
        DrivingScript drivingScript = GetComponent<DrivingScript>();
        if (drivingScript != null && drivingScript.rb != null)
        {
            stateTransform = drivingScript.rb.transform;
        }

        string playerName = PhotonNetwork.LocalPlayer.NickName;
        Color playerColor = Color.white;
        CarAppearance appearance = GetComponentInChildren<CarAppearance>();
        if (appearance != null)
        {
            playerName = appearance.playerName;
            playerColor = appearance.carColor;
        }

        object[] state = new object[]
        {
            PhotonNetwork.LocalPlayer.ActorNumber,
            stateTransform.position.x,
            stateTransform.position.y,
            stateTransform.position.z,
            stateTransform.rotation.x,
            stateTransform.rotation.y,
            stateTransform.rotation.z,
            stateTransform.rotation.w,
            playerName,
            playerColor.r,
            playerColor.g,
            playerColor.b
        };

        PhotonNetwork.RaiseEvent(
            RaceController.CarStateEventCode,
            state,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendUnreliable);
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

        SetPhotonSyncComponentsEnabled(true);

        DrivingScript drivingScript = GetComponent<DrivingScript>();
        if (drivingScript != null)
        {
            drivingScript.enabled = true;
            if (drivingScript.rb != null)
            {
                drivingScript.rb.isKinematic = false;
            }

            if (drivingScript.engineSound != null)
            {
                drivingScript.engineSound.mute = false;
                drivingScript.engineSound.loop = true;
                if (!drivingScript.engineSound.isPlaying)
                {
                    drivingScript.engineSound.Play();
                }
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
        SetPhotonSyncComponentsEnabled(false);

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        DrivingScript drivingScript = GetComponent<DrivingScript>();
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
    }

    void SetPhotonSyncComponentsEnabled(bool isEnabled)
    {
        PhotonTransformView[] transformViews = GetComponentsInChildren<PhotonTransformView>();
        foreach (PhotonTransformView transformView in transformViews)
        {
            transformView.enabled = isEnabled;
        }

        PhotonRigidbodyView[] rigidbodyViews = GetComponentsInChildren<PhotonRigidbodyView>();
        foreach (PhotonRigidbodyView rigidbodyView in rigidbodyViews)
        {
            rigidbodyView.enabled = isEnabled;
        }
    }

    void ApplyInstantiationData()
    {
        if (view != null && view.InstantiationData != null && view.InstantiationData.Length >= 4)
        {
            string playerName = view.InstantiationData[0] as string;
            Color playerColor = ColorCar.IntToColor(
                (int)view.InstantiationData[1],
                (int)view.InstantiationData[2],
                (int)view.InstantiationData[3]);

            CarAppearance appearance = GetComponentInChildren<CarAppearance>();
            if (appearance != null && !string.IsNullOrWhiteSpace(playerName))
            {
                appearance.SetNameAndColor(playerName, playerColor);
            }
        }
    }
}
