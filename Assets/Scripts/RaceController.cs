using UnityEngine;

public class RaceController : MonoBehaviour
{
    public static bool isRacing = false;
    public static int totalLaps = 1;
    public int timer = 3;

    public CheckPointController[] carControllers;

    void CountDown()
    {
        if (timer > 0)
        {
            Debug.Log("Race is starting: " + timer);
            timer--;
        }
        else
        {
            Debug.Log("Start!");
            isRacing = true;
            CancelInvoke("CountDown");
        }
    }

    void Start()
    {
        InvokeRepeating("CountDown", 3, 1);

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
                Debug.Log("Race finished!");
                isRacing = false;
            }
        }
    }
}
