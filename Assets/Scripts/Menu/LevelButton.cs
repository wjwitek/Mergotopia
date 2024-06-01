using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public string levelName;
    public TextMeshProUGUI levelNameLabel;
    public Star stars;
    public Button button;
    public GameObject lockedPanel;

    void Awake()
    {
        Reassign();
    }

    public void Reassign()
    {
        levelNameLabel.text = levelName;
        stars.levelName = levelName;
        stars.loadFromPlayerPrefs = true;
    }

    public void LoadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }

    public void SetLockedState(bool locked)
    {
        button.enabled = !locked;
        lockedPanel.SetActive(locked);
    }
}
