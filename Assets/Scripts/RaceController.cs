using UnityEngine;

public class RaceController : MonoBehaviour
{
    public static bool isRacing = false;
    public static int totalLaps = 1;
    public int timer = 3;

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
        InvokeRepeating("CountDown", 2f, 1f);
    }

    void Update()
    {
        
    }
}
