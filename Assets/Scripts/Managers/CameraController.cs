using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Player transform to follow
    public Vector3 offset = new Vector3(0, 0, -10); // Camera offset from player
    public float smoothSpeed = 0.125f; // How smoothly the camera follows

    void LateUpdate()
    {
        if (target == null)
        {
            // Find player if target is not set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move camera 
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
