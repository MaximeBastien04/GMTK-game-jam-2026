using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLoopManager : MonoBehaviour
{
    private enum GameState
    {
        Login,
        Tutorial,
        Shop,
        Minigame,
        GameOver
    }

    [Header("Shift Settings")]
    [Min(1f)]
    [SerializeField] private float shiftDuration = 60f;

    [Min(1)]
    [SerializeField] private int totalShifts = 30;

    [Header("Game Objects")]
    [SerializeField] private GameObject loginObject;
    [SerializeField] private GameObject tutorialObject;
    [SerializeField] private GameObject minigameObject;
    [SerializeField] private GameObject shopObject;
    [SerializeField] private GameObject gameOverObject;

    [Header("References")]
    [SerializeField] private AdSpawner adSpawner;
    [SerializeField] private ShopManager shopManager;

    [Header("Buttons")]
    [SerializeField] private Button loginStartButton;
    [SerializeField] private Button playButton;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayText;

    public int CompletedShifts { get; private set; }

    public int CurrentJulyDay =>
        CompletedShifts + 1;

    private GameState currentState;
    private float timeRemaining;

    private void Awake()
    {
        if (loginStartButton != null)
        {
            loginStartButton.onClick.AddListener(
                StartTutorial
            );
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(
                StartNextShift
            );
        }
    }

    private void Start()
    {
        OpenLoginScreen();
    }

    private void Update()
    {
        if (currentState != GameState.Minigame)
        {
            return;
        }

        UpdateShiftTimer();
    }

    private void OpenLoginScreen()
    {
        currentState = GameState.Login;
        CompletedShifts = 0;

        StopAdGameplay();

        SetObjectActive(loginObject, true);
        SetObjectActive(tutorialObject, false);
        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, false);
        SetObjectActive(gameOverObject, false);

        SetLoginButtonActive(true);
        SetPlayButtonActive(false);

        UpdateDayText();
        UpdateTimerText(shiftDuration);
    }

    public void StartTutorial()
    {
        if (currentState != GameState.Login)
        {
            return;
        }

        currentState = GameState.Tutorial;

        SetObjectActive(loginObject, false);
        SetObjectActive(tutorialObject, true);
        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, false);
        SetObjectActive(gameOverObject, false);

        SetLoginButtonActive(false);
        SetPlayButtonActive(false);

        StopAdGameplay();
    }

    /// <summary>
    /// Call this when the tutorial has finished.
    /// For now, you can connect a temporary tutorial button to this method.
    /// </summary>
    public void CompleteTutorial()
    {
        if (currentState != GameState.Tutorial)
        {
            return;
        }

        SetObjectActive(tutorialObject, false);

        OpenInitialShop();
    }

    private void OpenInitialShop()
    {
        CompletedShifts = 0;
        currentState = GameState.Shop;

        SetObjectActive(loginObject, false);
        SetObjectActive(tutorialObject, false);
        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, true);
        SetObjectActive(gameOverObject, false);

        if (shopManager != null)
        {
            shopManager.RefreshShop();
        }
        else
        {
            Debug.LogWarning(
                "GameLoopManager has no ShopManager assigned.",
                this
            );
        }

        SetLoginButtonActive(false);
        SetPlayButtonActive(true);

        UpdateDayText();
        UpdateTimerText(shiftDuration);
    }

    public void StartNextShift()
    {
        if (currentState != GameState.Shop)
        {
            return;
        }

        if (CompletedShifts >= totalShifts)
        {
            EndGame();
            return;
        }

        currentState = GameState.Minigame;
        timeRemaining = shiftDuration;

        SetObjectActive(loginObject, false);
        SetObjectActive(tutorialObject, false);
        SetObjectActive(shopObject, false);
        SetObjectActive(minigameObject, true);
        SetObjectActive(gameOverObject, false);

        SetLoginButtonActive(false);
        SetPlayButtonActive(false);

        UpdateDayText();
        UpdateTimerText(timeRemaining);

        if (adSpawner != null)
        {
            adSpawner.CloseAllAds();
            adSpawner.StartSpawning();
        }
    }

    private void UpdateShiftTimer()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            UpdateTimerText(timeRemaining);
            FinishShift();

            return;
        }

        UpdateTimerText(timeRemaining);
    }

    private void FinishShift()
    {
        if (currentState != GameState.Minigame)
        {
            return;
        }

        StopAdGameplay();

        CompletedShifts++;

        SetObjectActive(minigameObject, false);

        if (CompletedShifts >= totalShifts)
        {
            EndGame();
            return;
        }

        OpenShop();
    }

    private void OpenShop()
    {
        currentState = GameState.Shop;

        SetObjectActive(loginObject, false);
        SetObjectActive(tutorialObject, false);
        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, true);
        SetObjectActive(gameOverObject, false);

        if (shopManager != null)
        {
            shopManager.RefreshShop();
        }
        else
        {
            Debug.LogWarning(
                "GameLoopManager has no ShopManager assigned.",
                this
            );
        }

        SetLoginButtonActive(false);
        SetPlayButtonActive(true);

        UpdateDayText();
        UpdateTimerText(shiftDuration);
    }

    private void EndGame()
    {
        currentState = GameState.GameOver;

        StopAdGameplay();

        SetObjectActive(loginObject, false);
        SetObjectActive(tutorialObject, false);
        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, false);
        SetObjectActive(gameOverObject, true);

        SetLoginButtonActive(false);
        SetPlayButtonActive(false);

        if (dayText != null)
        {
            dayText.text = "AUGUST 1";
        }

        UpdateTimerText(0f);
    }

    private void StopAdGameplay()
    {
        if (adSpawner == null)
        {
            return;
        }

        adSpawner.StopSpawning();
        adSpawner.CloseAllAds();
    }

    private void UpdateTimerText(float seconds)
    {
        if (timerText == null)
        {
            return;
        }

        int displayedSeconds =
            Mathf.CeilToInt(
                Mathf.Max(0f, seconds)
            );

        int minutes =
            displayedSeconds / 60;

        int remainingSeconds =
            displayedSeconds % 60;

        timerText.text =
            $"{minutes:00}:{remainingSeconds:00}";
    }

    private void UpdateDayText()
    {
        if (dayText == null)
        {
            return;
        }

        dayText.text =
            $"JULY {CurrentJulyDay}";
    }

    private void SetLoginButtonActive(bool isActive)
    {
        if (loginStartButton == null)
        {
            return;
        }

        loginStartButton.interactable = isActive;
    }

    private void SetPlayButtonActive(bool isActive)
    {
        if (playButton == null)
        {
            return;
        }

        playButton.gameObject.SetActive(isActive);
        playButton.interactable = isActive;
    }

    private static void SetObjectActive(
        GameObject target,
        bool isActive
    )
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }

    private void OnDestroy()
    {
        if (loginStartButton != null)
        {
            loginStartButton.onClick.RemoveListener(
                StartTutorial
            );
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                StartNextShift
            );
        }
    }
}