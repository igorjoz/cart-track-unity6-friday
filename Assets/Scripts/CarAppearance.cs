using TMPro;
using UnityEngine;

public class CarAppearance : MonoBehaviour
{
    public string playerName;
    public Color carColor;
    public TextMeshProUGUI nameText;
    public Renderer carRenderer;

    public int playerNumber;
    public CheckPointController checkPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (checkPoint == null)
        {
            checkPoint = GetComponent<CheckPointController>();
        }

        if (string.IsNullOrWhiteSpace(playerName) && playerNumber == 0)
        {
            playerName = PlayerPrefs.GetString("PlayerName");
            carColor = ColorCar.IntToColor(PlayerPrefs.GetInt("Red"), PlayerPrefs.GetInt("Green"), PlayerPrefs.GetInt("Blue"));
        }
        else if (string.IsNullOrWhiteSpace(playerName))
        {
            string[] aiNames = { "Anna", "Marek", "Zofia", "Jan", "Kasia", "Tomasz", "Ewa", "Kamil" };
            playerName = aiNames[playerNumber % aiNames.Length];
            carColor = new Color(Random.Range(0f, 255f) / 255, Random.Range(0f, 255f) / 255, Random.Range(0f, 255f) / 255);
        }

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }

        if (nameText != null)
        {
            nameText.text = playerName;
            nameText.color = carColor; // opcjonalnie
        }

        if (carRenderer != null)
        {
            carRenderer.material.color = carColor;
        }
    }

    public void SetNameAndColor(string name, Color color)
    {
        playerName = name;
        carColor = color;

        if (nameText != null)
        {
            nameText.text = name;
            nameText.color = color;
        }

        if (carRenderer != null)
        {
            carRenderer.material.color = color;
        }
    }

    public void SetLocalPlayer()
    {
        DrivingScript ds = GetComponentInParent<DrivingScript>();
        CameraController cameraController = FindAnyObjectByType<CameraController>();

        if (ds != null && cameraController != null)
        {
            cameraController.SetCameraProperties(ds.gameObject);
        }
        else if (cameraController != null)
        {
            cameraController.SetCameraProperties(this.gameObject);
        }

        playerName = PlayerPrefs.GetString("PlayerName");
        carColor = ColorCar.IntToColor(PlayerPrefs.GetInt("Red"),
        PlayerPrefs.GetInt("Green"), PlayerPrefs.GetInt("Blue"));
        SetNameAndColor(playerName, carColor);
    }

}
