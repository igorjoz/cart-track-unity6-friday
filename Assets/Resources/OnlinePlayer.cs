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
        }
        else if (photonView.InstantiationData != null && photonView.InstantiationData.Length >= 4)
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
