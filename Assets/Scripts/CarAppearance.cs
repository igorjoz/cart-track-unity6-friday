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
        nameText.text = playerName;
        carRenderer.material.color = carColor;
        nameText.color = carColor; // opcjonalne
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
