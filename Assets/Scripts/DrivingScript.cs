using UnityEngine;

public class DrivingScript : MonoBehaviour
{
    [SerializeField]
    public WheelScript[] wheels;
    [SerializeField]
    public float currentSpeed;
    [SerializeField]
    private float maxSpeed = 150;

    [SerializeField]
    public float torque = 200; // moment obrotowy
    [SerializeField]
    public float maxSteerAngle = 30;
    [SerializeField]
    public float maxBreakTorque = 500;

    [SerializeField]
    Rigidbody rb;

    float lastTimeMove = 0;
    CheckPointController checkPointController;
    
    void Start()
    {
        checkPointController = GetComponent<CheckPointController>();
    }

    void Update()
    {
        float acceleration = Input.GetAxis("Vertical");
        float brake = Input.GetAxis("Jump");
        float steering = Input.GetAxis("Horizontal");

        if (rb.linearVelocity.magnitude > 1 || !RaceController.isRacing)
        {
            lastTimeMove = Time.time;
        }

        if (Time.time > lastTimeMove + 5 || rb.gameObject.transform.position.y < -5)
        {
            rb.transform.position = checkPointController.lastPoint.transform.position + Vector3.up * 2;
            rb.transform.rotation = checkPointController.lastPoint.transform.rotation;

            rb.gameObject.layer = 6;

            Invoke("ResetLayer", 5);
        }

        void ResetLayer()
        {
            rb.gameObject.layer = 0;
        }

        if (!RaceController.isRacing)
        {
            acceleration = 0;
            brake = 1;
        }

        Drive(acceleration, brake, steering);
    }

    public void Drive(float acceleration, float brake, float steering)
    {
        acceleration = Mathf.Clamp(acceleration, -1, 1);
        brake = Mathf.Clamp(brake, 0, 1) * maxBreakTorque;
        steering = Mathf.Clamp(steering, -1, 1) * maxSteerAngle;

        float thrustTorque = 0;

        if (currentSpeed < maxSpeed)
        {
            thrustTorque = acceleration * torque;
        }

        foreach (WheelScript wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = thrustTorque;

            if (wheel.isFrontWheel)
            {
                wheel.wheelCollider.steerAngle = steering;
            }
            else
            {
                wheel.wheelCollider.brakeTorque = brake;
            }

            Quaternion quat;
            Vector3 position;

            wheel.wheelCollider.GetWorldPose(out position, out quat);
            wheel.wheelModel.transform.position = position;
            wheel.wheelModel.transform.rotation = quat;
        }
    }
}
