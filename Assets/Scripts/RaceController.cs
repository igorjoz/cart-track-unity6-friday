using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RaceController : MonoBehaviour
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
        audioSource = GetComponent<AudioSource>();
        if (startText != null) startText.gameObject.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);

        InvokeRepeating("CountDown", 3, 1);

        for (int i = 0; i < playerCount; i++)
        {
            GameObject car = Instantiate(carPrefab);
            car.transform.position = spawnPoints[i].position;
            car.transform.rotation = spawnPoints[i].rotation;

            car.GetComponentInChildren<CarAppearance>().playerNumber = i;

            if (i == 0)
            {
                car.GetComponent<PlayerController>().enabled = true;
                //GameObject.FindObjectOfType<CameraController>().SetCameraProperties(car);
                //GameObject.FindFirstObjectByType<CameraController>().SetCameraProperties(car);
            }
        }

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

        carControllers = new CheckPointController[cars.Length];

        for (int i = 0; i < cars.Length; i++)
        {
            carControllers[i] = cars[i].GetComponent<CheckPointController>();
        }
    }

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
