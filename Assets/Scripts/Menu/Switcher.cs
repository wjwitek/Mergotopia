using UnityEngine;

public class Switcher : MonoBehaviour
{
    public UISwitcher.UISwitcher switcher;
    public string valueName;
    public int defaultValue;
    public AudioSource switchChangeSound;

    void Start()
    {
        UISwitcher.UISwitcher togg = switcher.GetComponent<UISwitcher.UISwitcher>();
        togg.onValueChanged.AddListener(delegate
        {
            ChangeSetting();
        });
        if (!PlayerPrefs.HasKey(valueName)) {
            PlayerPrefs.SetInt(valueName, defaultValue);
        }
        if (defaultValue == 1)
        {
            togg.SetOn();
        }
    }

    void ChangeSetting()
    {
        switchChangeSound.Play();
        var value = 0;
        if (switcher.isOn)
        {
            value = 1;
        }

        PlayerPrefs.SetInt(valueName, value);
    }
}
