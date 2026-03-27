using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    public int lap = 0;
    public int checkPoint = -1;
    int pointCount;
    public int nextPoint;
    void Start()
    {
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint");
        pointCount = checkpoints.Length;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "CheckPoint")
        {
            int thisPoint = int.Parse(other.gameObject.name);
            if (thisPoint == nextPoint)
            {
                //lastPoint = other.gameObject;
                checkPoint = thisPoint;
                if (checkPoint == 0)
                {
                    lap++;
                    Debug.Log("Lap: " + lap);
                }
                nextPoint++;
                nextPoint = nextPoint % pointCount;
            }
        }
    }
}
