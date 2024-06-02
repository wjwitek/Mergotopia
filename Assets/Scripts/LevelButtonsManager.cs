using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelButtonsManager : MonoBehaviour
{
    public LevelButton[] levelButtons;

    void Awake()
    {
        Assert.IsTrue(levelButtons.Length > 0, "There are no levels set");

        for (int i = 1; i < levelButtons.Length; i++)
        {
            string prevStarsKey = levelButtons[i - 1].levelName.Replace(' ', '_') + "_stars";
            int prevSavedStars = PlayerPrefs.GetInt(prevStarsKey, defaultValue: 0);
            if (prevSavedStars == 0)
            {
                break;
            }
            levelButtons[i].SetLockedState(false);
        }
    }
}
