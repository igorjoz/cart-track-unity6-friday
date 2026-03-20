using UnityEngine;

[ExecuteInEditMode]
public class CheckPointsNumbering : MonoBehaviour
{
    public Transform[] checkPoints;

    void Start()
    {
        checkPoints = GetComponentsInChildren<Transform>();

        for (int i = 1; i < checkPoints.Length; i++)
        {
            checkPoints[i].gameObject.name = (i).ToString();
        }
    }
}
