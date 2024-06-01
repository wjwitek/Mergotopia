using UnityEngine;

public class ParticleChange : MonoBehaviour
{
    public UISwitcher.UISwitcher switcher;
    public ParticleSystem particles;

    void Start()
    {
        UISwitcher.UISwitcher togg = switcher.GetComponent<UISwitcher.UISwitcher>();
        togg.onValueChanged.AddListener(delegate
        {
            ChangeSetting();
        });
    }

    void ChangeSetting()
    {
        if (switcher.isOn) {
            particles.Play();
        }
        else
        {
            print("Stopped");
            particles.Pause();
            particles.Clear();
        }
    }
}
