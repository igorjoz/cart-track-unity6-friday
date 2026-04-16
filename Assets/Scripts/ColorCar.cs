using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorCar : MonoBehaviour
{
    public Renderer rend;

    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    public TextMeshProUGUI redSliderText;
    public TextMeshProUGUI greenSliderText;
    public TextMeshProUGUI blueSliderText;

    public Color col;

    void Start()
    {
        col = IntToColor(PlayerPrefs.GetInt("Red"), PlayerPrefs.GetInt("Green"), PlayerPrefs.GetInt("Blue"));
        rend.material.color = col;

        if (redSlider != null) redSlider.value = (int)(col.r * 255f);
        if (greenSlider != null) greenSlider.value = (int)(col.g * 255f);
        if (blueSlider != null) blueSlider.value = (int)(col.b * 255f);
    }

    void Update()
    {
        if (redSlider != null && greenSlider != null && blueSlider != null)
        {
            SetCarColor((int)redSlider.value, (int)greenSlider.value, (int)blueSlider.value);
            
            if (redSliderText != null) redSliderText.text = redSlider.value.ToString();
            if (greenSliderText != null) greenSliderText.text = greenSlider.value.ToString();
            if (blueSliderText != null) blueSliderText.text = blueSlider.value.ToString();
        }
    }

    public static Color IntToColor(int red, int green, int blue)
    {
        float r = (float)red / 255;
        float g = (float)green / 255;
        float b = (float)blue / 255;
        Color col = new Color(r, g, b);

        return col;
    }

    void SetCarColor(int red, int green, int blue)
    {
        Color col = IntToColor(red, green, blue);
        if (rend != null) rend.material.color = col;
        
        PlayerPrefs.SetInt("Red", red);
        PlayerPrefs.SetInt("Green", green);
        PlayerPrefs.SetInt("Blue", blue);
    }
}