using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStar : MonoBehaviour
{
    public GameObject fullStar;
    public LevelManager gameManager;
    public string levelName;
    public int starNum;
    void OnEnable()
    {
        if (gameManager.CalculateStars() >= starNum)
        {
            fullStar.SetActive(true);
        }
    }
}
