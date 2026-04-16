using TMPro;
using UnityEngine;

public class CarAppearance : MonoBehaviour
{
    public string playerName;
    public Color carColor;
    public TextMeshProUGUI nameText;
    public Renderer carRenderer;

    public int playerNumber;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerNumber == 0)
        {
            playerName = PlayerPrefs.GetString("PlayerName");
            carColor = ColorCar.IntToColor(PlayerPrefs.GetInt("Red"), PlayerPrefs.GetInt("Green"), PlayerPrefs.GetInt("Blue"));
        }
        else
        {
            string[] aiNames = { "Anna", "Marek", "Zofia", "Jan", "Kasia", "Tomasz", "Ewa", "Kamil" };
            playerName = aiNames[playerNumber % aiNames.Length];
            carColor = new Color(Random.Range(0f, 255f) / 255, Random.Range(0f, 255f) / 255, Random.Range(0f, 255f) / 255);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
