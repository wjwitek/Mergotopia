using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayer : MonoBehaviour
{
    public float speed;
    public VariableJoystick joystick;
    public VariableJoystick slider;
    public Rigidbody2D rb;

    public void FixedUpdate()
    {
        Vector2 direction = Vector2.up * joystick.Vertical + Vector2.right * joystick.Horizontal;
        rb.AddForce(direction * speed * Time.fixedDeltaTime, ForceMode2D.Impulse);
        var impulse = slider.Vertical * Mathf.Deg2Rad * rb.inertia;
        rb.AddTorque(impulse, ForceMode2D.Impulse);
        if (rb.velocity.sqrMagnitude < 0.005)
        {
            rb.velocity = Vector2.zero;
        }
    }
}