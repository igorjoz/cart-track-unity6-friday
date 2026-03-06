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
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Drive(float acceleration, float brake, float steering)
    {
        acceleration = Mathf.Clamp(acceleration, -1, 1);
        brake = Mathf.Clamp(brake, 0, 1) * maxBreakTorque;
        steering = Mathf.Clamp(steering, -1, 1) * maxSteerAngle;

    }
}
