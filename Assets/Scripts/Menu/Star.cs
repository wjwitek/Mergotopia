using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Star : MonoBehaviour
{
    public Sprite fullStarImage, emptyStarImage;
    public List<Image> stars;
    public string levelName;
    public bool loadFromPlayerPrefs;

    void Awake()
    {
        if (loadFromPlayerPrefs)
        {
            int savedStars = PlayerPrefs.GetInt(levelName.Replace(' ', '_') + "_stars", defaultValue: 0);
            SetStars(savedStars);
        }
    }

    public void SetStars(int numberOfFullStars)
    {
        Assert.IsTrue(numberOfFullStars >= 0 && numberOfFullStars <= stars.Count, "Invalid number of stars");
        for (int i = 0; i < numberOfFullStars; i++)
        {
            stars[i].sprite = fullStarImage;
        }
        for (int i = numberOfFullStars; i < stars.Count; i++)
        {
            stars[i].sprite = emptyStarImage;
        }
    }
}
