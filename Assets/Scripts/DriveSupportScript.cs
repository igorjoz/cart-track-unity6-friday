using UnityEngine;

public class DriveSupportScript : MonoBehaviour
{
    Rigidbody rb;
    float lastTimeChecked;

    public float antiRoll = 5000.0f;
    [Header("0 - lewe koło, 1 - prawe koło")]
    public WheelCollider[] frontWheels = new WheelCollider[2];
    public WheelCollider[] backWheels = new WheelCollider[2];

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.up.y > 0.5f || rb.linearVelocity.magnitude > 1)
        {
            lastTimeChecked = Time.time;
        }

        if (lastTimeChecked + 3f < Time.time)
        {
            TurnBackCar();
        }
    }

    void TurnBackCar()
    {
        transform.position += Vector3.up;
        transform.rotation = Quaternion.LookRotation(transform.forward);
    }

    void FixedUpdate()
    {
        HoldWheelOnGround(frontWheels);
        HoldWheelOnGround(backWheels);
    }

    void HoldWheelOnGround(WheelCollider[] wheels)
    {
        if (wheels == null || wheels.Length < 2 || wheels[0] == null || wheels[1] == null) return;

        WheelHit hit;
        float leftRiding = 1.0f;
        float rightRiding = 1.0f;

        bool groundedL = wheels[0].GetGroundHit(out hit);
        if (groundedL) leftRiding = (-wheels[0].transform.InverseTransformPoint(hit.point).y - wheels[0].radius) / wheels[0].suspensionDistance;
        else leftRiding = 1;

        bool groundedR = wheels[1].GetGroundHit(out hit);
        if (groundedR) rightRiding = (-wheels[1].transform.InverseTransformPoint(hit.point).y - wheels[1].radius) / wheels[1].suspensionDistance;
        else rightRiding = 1;

        float antiRollForce = (leftRiding - rightRiding) * antiRoll;

        if (groundedL) rb.AddForceAtPosition(wheels[0].transform.up * -antiRollForce, wheels[0].transform.position);
        if (groundedR) rb.AddForceAtPosition(wheels[1].transform.up * antiRollForce, wheels[1].transform.position);
    }
}
