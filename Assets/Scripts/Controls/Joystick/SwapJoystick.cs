using UnityEngine;

public class SwapJoystick : MonoBehaviour
{
    public RectTransform joystick;

    public void Start()
    {
        int isSwaped = PlayerPrefs.GetInt("JoystickOnLeft");
        print(isSwaped);
        if (isSwaped == 1)
        {
            RectTransform joystickTransform = joystick.GetComponent<RectTransform>();
            joystickTransform.anchorMin = new Vector2(0, 0);
            joystickTransform.anchorMax = new Vector2(0, 0);
            joystickTransform.pivot = new Vector2(-1, 0);
            joystickTransform.rect.Set(0f, 0f, joystickTransform.rect.width, joystickTransform.rect.height);
        }
    }
}
