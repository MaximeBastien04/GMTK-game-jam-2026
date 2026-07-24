using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLoopManager : MonoBehaviour
{
    private enum GameState
    {
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
    [SerializeField] private GameObject minigameObject;
    [SerializeField] private GameObject shopObject;
    [SerializeField] private GameObject gameOverObject;

    [Header("References")]
    [SerializeField] private AdSpawner adSpawner;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Button playButton;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayText;

    [Header("Optional")]
    [SerializeField] private TMP_Text shiftText;

    public int CompletedShifts { get; private set; }
    public int CurrentJulyDay => CompletedShifts + 1;

    private GameState currentState;
    private float timeRemaining;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartNextShift);
        }

        OpenInitialShop();
    }

    private void Update()
    {
        if (currentState != GameState.Minigame)
        {
            return;
        }

        UpdateShiftTimer();
    }

    private void OpenInitialShop()
    {
        CompletedShifts = 0;

        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, true);
        SetObjectActive(gameOverObject, false);

        shopManager.RefreshShop();
        SetPlayButtonActive(true);

        currentState = GameState.Shop;

        UpdateDayText();
        UpdateShiftText();
        UpdateTimerText(shiftDuration);
    }

    public void StartNextShift()
    {
        if (currentState == GameState.Minigame)
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

        SetObjectActive(shopObject, false);
        SetObjectActive(minigameObject, true);
        SetObjectActive(gameOverObject, false);

        SetPlayButtonActive(false);

        UpdateDayText();
        UpdateShiftText();
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

        if (adSpawner != null)
        {
            adSpawner.StopSpawning();
            adSpawner.CloseAllAds();
        }

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

        SetObjectActive(shopObject, true);
        SetObjectActive(gameOverObject, false);

        shopManager.RefreshShop();
        SetPlayButtonActive(true);

        UpdateDayText();
        UpdateShiftText();
    }

    private void EndGame()
    {
        currentState = GameState.GameOver;

        if (adSpawner != null)
        {
            adSpawner.StopSpawning();
            adSpawner.CloseAllAds();
        }

        SetObjectActive(minigameObject, false);
        SetObjectActive(shopObject, false);
        SetObjectActive(gameOverObject, true);

        SetPlayButtonActive(false);

        if (dayText != null)
        {
            dayText.text = "AUGUST 1";
        }

        if (shiftText != null)
        {
            shiftText.text = "MONTH COMPLETE";
        }

        UpdateTimerText(0f);
    }

    private void UpdateTimerText(float seconds)
    {
        if (timerText == null)
        {
            return;
        }

        int displayedSeconds =
            Mathf.CeilToInt(Mathf.Max(0f, seconds));

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

    private void UpdateShiftText()
    {
        if (shiftText == null)
        {
            return;
        }

        int displayedShift =
            Mathf.Clamp(
                CompletedShifts + 1,
                1,
                totalShifts
            );

        shiftText.text =
            $"SHIFT {displayedShift}/{totalShifts}";
    }

    private void SetPlayButtonActive(bool isActive)
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(isActive);
            playButton.interactable = isActive;
        }
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
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                StartNextShift
            );
        }
    }
}