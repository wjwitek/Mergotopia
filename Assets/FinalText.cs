using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalText : MonoBehaviour
{
    public LevelManager gameManager;
    public TextMeshProUGUI text;

    void OnEnable()
    {
        if (gameManager.CalculateStars() < 1)
        {
            text.text = "TRY AGAIN!";
        }
    }
}
