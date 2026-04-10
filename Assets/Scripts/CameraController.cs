using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public Vector3[] positions;
    public CinemachineVirtualCamera cam;
    int activePosition = 0;

    void Start()
    {
        if (positions.Length == 0 || cam == null) return;
        
        var transposer = cam.GetCinemachineComponent(CinemachineCore.Stage.Body);
        // Uwaga:zależy od wersji Cinemachine (np. m_FollowOffset)
    }

    void Update()
    {
        if (positions.Length == 0 || cam == null) return;
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            activePosition++;
            activePosition = activePosition % positions.Length;
            
            var transposer = cam.GetCinemachineComponent(CinemachineCore.Stage.Body);
            // Aktualizacja pozycji (m_FollowOffset)
        }
    }
}
