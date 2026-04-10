using UnityEngine;

public class PlayerController : MonoBehaviour
{
    DrivingScript driveScript;

    void Start()
    {
        driveScript = GetComponent<DrivingScript>();
    }

    void Update()
    {
        float acceleration = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        float brake = Input.GetAxis("Jump");

        if (!RaceController.isRacing)
        {
            acceleration = 0;
            brake = 1;
        }

        driveScript.Drive(acceleration, brake, steering);
        driveScript.EngineSound();
    }
}
