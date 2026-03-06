using UnityEngine;

public class DriveSupportScript : MonoBehaviour
{
    Rigidbody rb;
    float lastTimeChecked;

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
}
