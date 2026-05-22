using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    DrivingScript driveScript;
    float lastTimeMoving = 0;
    CheckPointController checkPointController;

    void Start()
    {
        driveScript = GetComponent<DrivingScript>();
        checkPointController = driveScript.rb.GetComponent<CheckPointController>();
    }

    void Update()
    {
        if (checkPointController.lap == RaceController.totalLaps + 1)
        {
            return;
        }

        float acceleration = ReadAcceleration();
        float steering = ReadSteering();
        float brake = ReadBrake();
        bool nitro = WasNitroPressed();

        driveScript.CheckNitro(nitro);

        if (driveScript.rb.linearVelocity.magnitude > 1 || !RaceController.isRacing)
        {
            lastTimeMoving = Time.time;
        }

        if (Time.time > lastTimeMoving + 5 || driveScript.rb.gameObject.transform.position.y < -5)
        {
            driveScript.rb.transform.position = checkPointController.lastPoint.transform.position + Vector3.up * 2;
            driveScript.rb.transform.rotation = checkPointController.lastPoint.transform.rotation;

            driveScript.rb.gameObject.layer = 6;
            Invoke("ResetLayer", 3);
        }

        if (!RaceController.isRacing)
        {
            acceleration = 0;
            brake = 1;
        }

        driveScript.Drive(acceleration, brake, steering);
        driveScript.EngineSound();
    }

    float ReadAcceleration()
    {
        float value = 0;

#if ENABLE_LEGACY_INPUT_MANAGER
        value += Input.GetAxis("Vertical");
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value += 1;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value -= 1;
        }
#endif

        return Mathf.Clamp(value, -1, 1);
    }

    float ReadSteering()
    {
        float value = 0;

#if ENABLE_LEGACY_INPUT_MANAGER
        value += Input.GetAxis("Horizontal");
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value += 1;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value -= 1;
        }
#endif

        return Mathf.Clamp(value, -1, 1);
    }

    float ReadBrake()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetAxis("Jump") > 0.1f || Input.GetKey(KeyCode.Space))
        {
            return 1;
        }
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.isPressed)
        {
            return 1;
        }
#endif

        return 0;
    }

    bool WasNitroPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            return true;
        }
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
        }
#endif

        return false;
    }

    void ResetLayer()
    {
        driveScript.rb.gameObject.layer = 0;
    }
}
