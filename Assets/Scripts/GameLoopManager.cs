using System.Collections;
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
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private CalendarManager calendarManager;

    [Header("Buttons")]
    [SerializeField] private Button loginStartButton;
    [SerializeField] private Button playButton;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayText;

    [Header("Timer Warning")]
    [Min(0f)]
    [SerializeField] private float timerFlickerThreshold = 3f;

    [Min(0.1f)]
    [SerializeField] private float timerFlickerSpeed = 4f;

    public int CompletedShifts { get; private set; }

    public int CurrentJulyDay =>
        calendarManager != null
            ? calendarManager.CurrentDate.Day
            : CompletedShifts + 1;

    private GameState currentState;
    private float timeRemaining;
    private bool isStartingShift;

    public bool IsMinigameActive => currentState == GameState.Minigame;

    [Header("Debug")]
    [SerializeField] private bool skipTutorial = true;

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
        SetTimerOpacity(1f);
    }

    public void StartTutorial()
    {
        if (currentState != GameState.Login)
        {
            return;
        }

        /*
         * Debug option:
         * When Skip Tutorial is enabled in the Inspector,
         * bypass Dave's tutorial and open the initial shop.
         */
        if (skipTutorial)
        {
            StopAdGameplay();
            OpenInitialShop();
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

        if (tutorialManager != null)
        {
            tutorialManager.BeginTutorial();
        }
        else
        {
            Debug.LogError(
                "No TutorialManager assigned.",
                this
            );
        }
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
        SetTimerOpacity(1f);
    }

    public void StartNextShift()
    {
        if (currentState != GameState.Shop ||
            isStartingShift)
        {
            return;
        }

        StartCoroutine(
            StartNextShiftRoutine()
        );
    }

    private IEnumerator StartNextShiftRoutine()
    {
        isStartingShift = true;
        SetPlayButtonActive(false);

        /*
         * The initial Play press starts July 1.
         * Every later Play press first changes the calendar,
         * then begins the next shift.
         */
        if (CompletedShifts > 0 &&
            calendarManager != null)
        {
            /*
             * Advance the gameplay date immediately, then run the
             * calendar page animations independently. The minigame
             * does not wait for Friday/weekend page animations.
             */
            StartCoroutine(
                calendarManager.AdvanceToNextWorkday()
            );
        }

        if (calendarManager != null &&
            calendarManager.IsFinalDay &&
            CompletedShifts > 0)
        {
            /*
             * July 31 is still playable. This guard only prevents
             * attempting to start a shift after the final date.
             */
        }
        else if (calendarManager == null &&
                 CompletedShifts >= totalShifts)
        {
            isStartingShift = false;
            EndGame();
            yield break;
        }

        BeginShiftGameplay();
        isStartingShift = false;

        yield break;
    }

    private void BeginShiftGameplay()
    {
        if (ItemEffectManager.Instance != null)
        {
            ItemEffectManager.Instance.ResetShiftEffects();
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
        SetTimerOpacity(1f);

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
            SetTimerOpacity(1f);
            FinishShift();

            return;
        }

        UpdateTimerText(timeRemaining);
        UpdateTimerFlicker();
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

        bool reachedFinalDay =
            calendarManager != null &&
            calendarManager.IsFinalDay;

        bool reachedFallbackShiftLimit =
            calendarManager == null &&
            CompletedShifts >= totalShifts;

        if (reachedFinalDay ||
            reachedFallbackShiftLimit)
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
        SetTimerOpacity(1f);
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
        SetTimerOpacity(1f);
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

        /*
         * Convert the full remaining duration into whole seconds first.
         * Dividing by 60 and using modulo guarantees values such as
         * 60 seconds display as 01:00 rather than 00:60.
         */
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

    private void UpdateTimerFlicker()
    {
        if (timerText == null)
        {
            return;
        }

        if (timeRemaining > timerFlickerThreshold)
        {
            SetTimerOpacity(1f);
            return;
        }

        /*
         * Alternate instantly between fully visible and invisible.
         * FloorToInt creates a hard on/off flicker instead of a fade.
         */
        int flickerStep =
            Mathf.FloorToInt(
                Time.unscaledTime * timerFlickerSpeed
            );

        bool isVisible =
            flickerStep % 2 == 0;

        SetTimerOpacity(
            isVisible ? 1f : 0f
        );
    }

    private void SetTimerOpacity(float opacity)
    {
        if (timerText == null)
        {
            return;
        }

        Color timerColor =
            timerText.color;

        timerColor.a =
            Mathf.Clamp01(opacity);

        timerText.color =
            timerColor;
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

    public bool AddTime(float seconds)
    {
        if (currentState != GameState.Minigame)
        {
            return false;
        }

        if (seconds <= 0f)
        {
            return false;
        }

        timeRemaining += seconds;

        UpdateTimerText(timeRemaining);

        return true;
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