using UnityEngine;

public class PlayerController : MonoBehaviour
{
    DrivingScript driveScript;
    float lastTimeMoving = 0;
    CheckPointController checkPointController;

    void Start()
    {
        driveScript = GetComponent<DrivingScript>();
        if (driveScript != null)
        {
            checkPointController = driveScript.GetComponentInChildren<CheckPointController>();
            if (checkPointController == null && driveScript.rb != null)
            {
                checkPointController = driveScript.rb.GetComponent<CheckPointController>();
            }
        }
    }

    void Update()
    {
        float acceleration = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        float brake = Input.GetAxis("Jump");

        if (driveScript != null && driveScript.rb != null)
        {
            if (driveScript.rb.linearVelocity.magnitude > 1 || !RaceController.isRacing)
            {
                lastTimeMoving = Time.time;
            }

            if (Time.time > lastTimeMoving + 5 || driveScript.rb.gameObject.transform.position.y < -5)
            {
                if (checkPointController != null && checkPointController.lastPoint != null)
                {
                    driveScript.rb.transform.position = checkPointController.lastPoint.transform.position + Vector3.up * 2;
                    driveScript.rb.transform.rotation = checkPointController.lastPoint.transform.rotation;
                }

                driveScript.rb.gameObject.layer = 6;
                Invoke("ResetLayer", 3);
            }
        }

        if (!RaceController.isRacing)
        {
            acceleration = 0;
            brake = 1;
        }

        if (driveScript != null)
        {
            driveScript.Drive(acceleration, brake, steering);
            driveScript.EngineSound();
        }
    }

    void ResetLayer()
    {
        if (driveScript != null && driveScript.rb != null)
        {
            driveScript.rb.gameObject.layer = 0;
        }
    }
}
