using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float angle;

    public void Rotate()
    {
        transform.Rotate(Vector3.up, angle);
    }
}
