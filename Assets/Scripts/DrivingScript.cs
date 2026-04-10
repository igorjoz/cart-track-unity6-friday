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

    public GameObject backLights;
    public AudioSource engineSound;
    float rpm;
    int currentGear = 1;
    float currentGearPerc;
    public int numGears = 5;

    float lastTimeMove = 0;
    CheckPointController checkPointController;
    
    void Start()
    {
        checkPointController = GetComponent<CheckPointController>();
    }

    void Update()
    {
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
    }

    public void Drive(float acceleration, float brake, float steering)
    {
        acceleration = Mathf.Clamp(acceleration, -1, 1);
        brake = Mathf.Clamp(brake, 0, 1) * maxBreakTorque;
        steering = Mathf.Clamp(steering, -1, 1) * maxSteerAngle;

        if (backLights != null)
        {
            if (brake != 0) backLights.SetActive(true);
            else backLights.SetActive(false);
        }

        float thrustTorque = 0;
        currentSpeed = rb.linearVelocity.magnitude * 5;

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

    public void EngineSound()
    {
        if (engineSound == null) return;

        float gearPercentage = (1 / (float)numGears);
        float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear + 1), Mathf.Abs(currentSpeed / maxSpeed));
        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime * 5f);
        
        var gearNumFactor = currentGear / (float)numGears;
        rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
        float downGearMax = (1 / (float)numGears) * currentGear;

        if (currentGear > 0 && speedPercentage < downGearMax) currentGear--;
        if (speedPercentage > upperGearMax && (currentGear < (numGears - 1))) currentGear++;

        float pitch = Mathf.Lerp(1, 6, rpm);
        engineSound.pitch = Mathf.Min(6, pitch) * 0.15f;
    }
}
