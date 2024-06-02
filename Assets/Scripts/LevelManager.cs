using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private enum GameState
    {
        None        = 0,
        Start       = 1,
        Gameplay    = 2,
        End         = 3
    }

    private GameState gameState = GameState.None;

    public MainCamera mainCamera;

    public GameObject startGameUI;
    public GameObject startGameObjects;

    public GameObject gameplayUI;
    public GameObject gameplayObjects;

    public GameObject endGameUI;
    public GameObject endGameObjects;

    public GameObject resultViewUI;
    public TextMeshProUGUI resultViewScoreText;
    public Star resultStars;

    public GameObject goal;
    public GameObject player;

    public float gameplayTime = 120;
    public TextMeshProUGUI timerText;
    private float currentTime;

    public TextMeshProUGUI scoreText;
    private int score = 0;

    private AreaChecker areaChecker = new AreaChecker(0.01f);

    public void Start()
    {
        SwitchGameState(GameState.Start);
        Invoke(nameof(StartGameplay), 5);
    }

    public void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            }
        }
        else 
        {
           if (Input.GetKey(KeyCode.Backspace))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            }
        }

        if (gameState == GameState.Gameplay)
        {
            if (currentTime < 0)
            {
                EndGameplay();
                return;
            }

            currentTime -= Time.deltaTime;
            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartGameplay()
    {
        if (gameState < GameState.Gameplay)
        {
            SwitchGameState(GameState.Gameplay);
            mainCamera.followPlayer = true;
            currentTime = gameplayTime;
        }
    }

    public void EndGameplay()
    {
        SwitchGameState(GameState.End);

        DisablePlayer();
        player.transform.position = Vector3.zero;
        mainCamera.followPlayer = false;
        mainCamera.transform.position = new Vector3(0, 0, mainCamera.transform.position.z);
    }

    public void AcceptScore()
    {
        CalculateScore();
        Debug.Log("Your final score is: " + score + "%");
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string starsKey = currentSceneName.Replace(' ', '_') + "_stars";
        int acquiredStars = CalculateStars();
        int currentStars = PlayerPrefs.GetInt(starsKey, defaultValue: 0);
        PlayerPrefs.SetInt(starsKey, Math.Max(currentStars, acquiredStars));
        resultStars.SetStars(acquiredStars);

        resultViewScoreText.text = scoreText.text;
        endGameObjects.SetActive(false);
        endGameUI.SetActive(false);
        player.SetActive(false);
        resultViewUI.SetActive(true);
    }

    public void ShowGoal(GameObject goal)
    {
        goal.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z + 1);
        goal.SetActive(true);
    }

    public void StopShowingGoal(GameObject goal)
    {
        goal.SetActive(false);
    }

    public int CalculateStars()
    {
        if (score == 100)
        {
            return 3;
        }
        else if (score >= 75)
        {
            return 2;
        }
        else if (score >= 50)
        {
            return 1;
        }
        return 0;
    }

    private void SwitchGameState(GameState state)
    {
        if (state == gameState)
        {
            return;
        }

        switch (state)
        {
            case GameState.None:
                Debug.LogError("Cannot switch back to [" + state + "] game state");
                return;

            case GameState.Start:
                player.SetActive(false);
                setGameplayElementsVisibility(false);
                setEndGameElementsVisibility(false);
                setStartGameElementsVisibility(true);
                break;

            case GameState.Gameplay:
                player.SetActive(true);
                setStartGameElementsVisibility(false);
                setEndGameElementsVisibility(false);
                setGameplayElementsVisibility(true);
                break;

            case GameState.End:
                player.SetActive(true);
                setEndGameElementsVisibility(true);
                setStartGameElementsVisibility(false);
                setGameplayElementsVisibility(false);
                break;

            default:
                Debug.LogError("Unknown game state: [" + state + "]");
                return;
        }

        gameState = state;
    }

    private void setStartGameElementsVisibility(bool visible)
    {
        startGameUI.SetActive(visible);
        startGameObjects.SetActive(visible);
    }

    private void setGameplayElementsVisibility(bool visible)
    {
        gameplayUI.SetActive(visible);
        gameplayObjects.SetActive(visible);
    }

    private void setEndGameElementsVisibility(bool visible)
    {
        endGameUI.SetActive(visible);
        endGameObjects.SetActive(visible);
    }

    public void CalculateScore()
    {
        score = Mathf.CeilToInt(areaChecker.GetAreasSimilarity(player, goal, 0.9f) * 100);
        if (scoreText == null)
        {
            Debug.LogError("Score text is not set");
            return;
        }
        scoreText.text = score.ToString() + "%";
    }

    private void DisablePlayer()
    {
        var playerScript = player.GetComponent<Player>();
        if (playerScript == null)
        {
            Debug.LogError("Player does not have Player script");
            return;
        }
        // Disable rigibody (physics of player)
        playerScript.rbPhysics.simulated = false;
        playerScript.enabled = false;
    }

    public void RestartGame()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
