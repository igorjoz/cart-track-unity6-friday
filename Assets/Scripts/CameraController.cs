using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public Vector3[] positions;
    public CinemachineCamera cam;
    int activePosition = 0;

    void Start()
    {
        if (positions.Length == 0 || cam == null) return;
        
        var follow = cam.GetComponent<CinemachineThirdPersonFollow>();
        if (follow != null) {
            follow.ShoulderOffset = positions[activePosition];
        }
    }

    void Update()
    {
        if (positions.Length == 0 || cam == null) return;
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            activePosition++;
            activePosition = activePosition % positions.Length;
            
            var follow = cam.GetComponent<CinemachineThirdPersonFollow>();
            if (follow != null) {
                follow.ShoulderOffset = positions[activePosition];
            }
        }
    }

    public void SetCameraProperties(GameObject car)
    {
        if (cam != null && car != null)
        {
            DrivingScript ds = car.GetComponent<DrivingScript>();
            if (ds != null && ds.rb != null)
            {
                cam.Follow = ds.rb.transform;
            }
            if (ds != null && ds.cameraTarget != null)
            {
                cam.LookAt = ds.cameraTarget.transform;
            }
        }
    }
}
