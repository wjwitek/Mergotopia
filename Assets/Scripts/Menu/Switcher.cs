using UnityEngine;

public class Switcher : MonoBehaviour
{
    public UISwitcher.UISwitcher switcher;
    public string valueName;
    public int defaultValue;
    public AudioSource switchChangeSound;

    void Awake()
    {
        UISwitcher.UISwitcher togg = switcher.GetComponent<UISwitcher.UISwitcher>();
        togg.onValueChanged.AddListener(delegate
        {
            ChangeSetting();
        });
        if (!PlayerPrefs.HasKey(valueName))
        {
            PlayerPrefs.SetInt(valueName, defaultValue);
        }
        togg.isOn = PlayerPrefs.GetInt(valueName) == 1 ? true : false;
    }

    private void ChangeSetting()
    {
        switchChangeSound.Play();
        PlayerPrefs.SetInt(valueName, switcher.isOn ? 1 : 0);
    }
}
