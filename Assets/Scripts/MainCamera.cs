using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform target;
    public bool followPlayer = true;
    public Rigidbody2D rb;

    public float time = 0.1f;
    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;
    private float pixelsToWorld = 0;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        pixelsToWorld = 1f / (cam.WorldToScreenPoint(Vector2.up) - cam.WorldToScreenPoint(Vector2.zero)).y;
        Debug.Log("pixelsToWorld: " + pixelsToWorld);
    }

    void FixedUpdate()
    {
        if (followPlayer)
        {
            Vector3 endPosition = new Vector3(rb.centerOfMass.x + target.position.x, rb.centerOfMass.y + target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref velocity, smoothTime);
        }
    }

    public float PixelsToWorld(float pixels)
    {
        return pixels * pixelsToWorld;
    }

    public float GetPixelsToWorld()
    {
        return pixelsToWorld;
    }
}
