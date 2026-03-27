using UnityEngine;
using UnityEngine.UIElements.Experimental;

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
    }

        //void Start()
        //{
        //    Debug.Log("-----------------------------");
        //    InvokeRepeating("CountDown", 3, 1);
        //    GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        //    carControllers = new CheckPointController[cars.Length];
        //    for (int i = 0; i < cars.Length; i++)
        //    {
        //        carControllers[i] = cars[i].GetComponent<CheckPointController>();
        //    }
        //}
        //void LateUpdate()
        //{
        //    int finishedLap = 0;
        //    foreach (CheckPointController controller in carControllers)
        //    {
        //        if (controller.lap == totalLaps + 1) finishedLap++;
        //        if (finishedLap == carControllers.Length && isRacing)
        //        {
        //            Debug.Log("FinishRace");
        //            isRacing = false;
        //        }
        //    }
        //}
}
