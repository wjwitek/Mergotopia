using UnityEngine;

public class SwapSlider : MonoBehaviour
{
    public RectTransform slider;

    public void Start()
    {
        int isSwaped = PlayerPrefs.GetInt("JoystickOnLeft");
        if (isSwaped == 1)
        {
            RectTransform sliderTransform = slider.GetComponent<RectTransform>();
            sliderTransform.anchorMin = new Vector2(1, 0);
            sliderTransform.anchorMax = new Vector2(1, 0);
            sliderTransform.pivot = new Vector2(2, 0);
            sliderTransform.rect.Set(0f, 0f, sliderTransform.rect.width, sliderTransform.rect.height);
        }
    }
}
