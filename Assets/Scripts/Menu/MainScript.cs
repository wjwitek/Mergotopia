using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    public Button quitButton;
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public GameObject mainMenu;
    public GameObject levels;
    public GameObject settings;
    public ParticleSystem particles;
    public AudioSource buttonClickSound;

    void Start()
    {
        Button quitBtn = quitButton.GetComponent<Button>();
        quitBtn.onClick.AddListener(QuitClick);

        Button startBtn = startButton.GetComponent<Button>();
        startBtn.onClick.AddListener(() => OnClick(levels));

        Button settingsBtn = settingsButton.GetComponent<Button>();
        settingsBtn.onClick.AddListener(() => OnClick(settings));

        Button exitBtn = exitButton.GetComponent<Button>();
        exitBtn.onClick.AddListener(() => OnClick(mainMenu));

        levels.SetActive(false);
        settings.SetActive(false);

        if (PlayerPrefs.GetInt("Particles") == 1)
        {
            ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
            particleSystem.Play();
        }
    }

    public void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                if (settings.activeSelf || levels.activeSelf)
                {
                    MenuClick();
                }
            }
        }
        else 
        {
           if (Input.GetKey(KeyCode.Backspace))
            {
                if (settings.activeSelf || levels.activeSelf)
                {
                    MenuClick();
                }
            }
        }
    }

    void OnClick(GameObject active)
    {
        settings.SetActive(false);
        mainMenu.SetActive(false);
        levels.SetActive(false);
        active.SetActive(true);
        buttonClickSound.Play();
    }

    void QuitClick()
    {
        Debug.Log("exitgame");
        Application.Quit();
    }

    void MenuClick()
    {
        levels.SetActive(false);
        mainMenu.SetActive(true);
        settings.SetActive(false);
    }
}
