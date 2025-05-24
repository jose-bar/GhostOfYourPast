using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10); // Keep Z at -10 for 2D

    [Header("Bounds (Optional)")]
    public bool useBounds = false;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    void Start()
    {
        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Apply bounds if enabled
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // Call this when player enters new room to instantly move camera
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 snapPosition = target.position + offset;
            if (useBounds)
            {
                snapPosition.x = Mathf.Clamp(snapPosition.x, minX, maxX);
                snapPosition.y = Mathf.Clamp(snapPosition.y, minY, maxY);
            }
            transform.position = snapPosition;
        }
    }

    // Set new bounds when entering different rooms
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }
}