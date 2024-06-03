using UnityEngine;

public class MusicChange : MonoBehaviour
{
    public UISwitcher.UISwitcher switcher;
    public AudioSource music;

    void Start()
    {
        UISwitcher.UISwitcher togg = switcher.GetComponent<UISwitcher.UISwitcher>();
        togg.onValueChanged.AddListener(delegate
        {
            ChangeSetting();
        });
        ChangeSetting();
    }

    void ChangeSetting()
    {
        music.mute = !switcher.isOn;
    }
}
