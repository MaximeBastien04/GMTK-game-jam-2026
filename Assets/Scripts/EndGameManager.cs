using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    [Header("Win Requirement")]
    [Min(0)]
    [SerializeField] private int requiredMoneyToWin = 10000;

    [Header("Win Sequence")]
    [SerializeField] private GameObject winEndGame;
    [SerializeField] private GameObject familyPicturePrefab;
    [SerializeField] private Transform familyPictureParent;
    [SerializeField] private GameObject bahamasTicketsPrefab;
    [SerializeField] private Transform bahamasTicketsParent;
    [SerializeField] private Button winHomeButton;

    [Header("Lose Sequence")]
    [SerializeField] private GameObject looseEndGame;
    [SerializeField] private GameObject bahamasMagazinePrefab;
    [SerializeField] private Transform bahamasMagazineParent;

    [Tooltip("The textbox background GameObject.")]
    [SerializeField] private GameObject looseTextBox;

    [Tooltip("The TextMeshPro component inside the textbox.")]
    [SerializeField] private TMP_Text looseMessageText;

    [TextArea(2, 5)]
    [SerializeField]
    private string looseMessage =
        "What a nice dream...\nMaybe next year!";

    [Min(1f)]
    [SerializeField] private float charactersPerSecond = 30f;

    [SerializeField] private Button looseHomeButton;

    [Header("Timing")]
    [Min(0f)]
    [SerializeField] private float fadeDuration = 1f;

    [Tooltip("Delay after the end screen finishes fading before the first prefab appears.")]
    [Min(0f)]
    [SerializeField] private float firstRevealDelay = 2f;

    [Min(0f)]
    [SerializeField] private float winTicketsDelay = 0.5f;

    [Min(0f)]
    [SerializeField] private float winHomeDelay = 5f;

    [Tooltip("Delay after the magazine appears before the textbox appears.")]
    [Min(0f)]
    [SerializeField] private float looseTextBoxDelay = 4f;

    [Tooltip("Delay after the typewriter finishes before the Home button appears.")]
    [Min(0f)]
    [SerializeField] private float looseHomeDelay = 5f;

    [Header("Mouse")]
    [SerializeField] private MouseConstraint mouseConstraint;

    public bool HasEndGameStarted { get; private set; }

    private Image winEndGameImage;
    private Image looseEndGameImage;
    private Coroutine endGameRoutine;

    private void Awake()
    {
        CacheEndScreenImages();
        PrepareSequenceObjects();

        if (winHomeButton != null)
        {
            winHomeButton.onClick.AddListener(ReturnHome);
        }

        if (looseHomeButton != null)
        {
            looseHomeButton.onClick.AddListener(ReturnHome);
        }
    }

    private void OnDestroy()
    {
        if (winHomeButton != null)
        {
            winHomeButton.onClick.RemoveListener(ReturnHome);
        }

        if (looseHomeButton != null)
        {
            looseHomeButton.onClick.RemoveListener(ReturnHome);
        }
    }

    /// <summary>
    /// Call this after July 31 has been completed and the player presses Play again.
    /// </summary>
    public void BeginEndGame(int finalMoney)
    {
        if (HasEndGameStarted)
        {
            return;
        }

        HasEndGameStarted = true;

        SwitchToSystemCursor();

        bool playerWon =
            finalMoney >= requiredMoneyToWin;

        Debug.Log(
            $"Final money: €{finalMoney}. " +
            $"Required money: €{requiredMoneyToWin}. " +
            $"Result: {(playerWon ? "WIN" : "LOSE")}",
            this
        );

        endGameRoutine = StartCoroutine(
            playerWon
                ? PlayWinSequence()
                : PlayLooseSequence()
        );
    }

    private IEnumerator PlayWinSequence()
    {
        SetEndScreenActive(looseEndGame, false);

        yield return ActivateEndScreenForFade(
            winEndGame,
            winEndGameImage
        );

        yield return FadeImage(
            winEndGameImage,
            0f,
            1f,
            fadeDuration
        );

        yield return WaitRealtime(firstRevealDelay);

        SpawnPrefab(
            familyPicturePrefab,
            familyPictureParent
        );

        yield return WaitRealtime(winTicketsDelay);

        SpawnPrefab(
            bahamasTicketsPrefab,
            bahamasTicketsParent
        );

        yield return WaitRealtime(winHomeDelay);

        ShowButton(winHomeButton);
    }

    private IEnumerator PlayLooseSequence()
    {
        SetEndScreenActive(winEndGame, false);

        yield return ActivateEndScreenForFade(
            looseEndGame,
            looseEndGameImage
        );

        yield return FadeImage(
            looseEndGameImage,
            0f,
            1f,
            fadeDuration
        );

        yield return WaitRealtime(firstRevealDelay);

        SpawnPrefab(
            bahamasMagazinePrefab,
            bahamasMagazineParent
        );

        yield return WaitRealtime(looseTextBoxDelay);

        if (looseTextBox != null)
        {
            looseTextBox.SetActive(true);
        }

        yield return TypeLooseMessage();

        yield return WaitRealtime(looseHomeDelay);

        ShowButton(looseHomeButton);
    }

    private static IEnumerator ActivateEndScreenForFade(
    GameObject endScreen,
    Image endScreenImage
)
    {
        if (endScreen == null)
        {
            yield break;
        }

        if (endScreenImage != null)
        {
            Color color = endScreenImage.color;
            color.a = 0f;
            endScreenImage.color = color;
        }

        endScreen.SetActive(true);

        Canvas.ForceUpdateCanvases();

        // Allow Unity to render the newly activated object once at alpha 0.
        yield return null;
    }

    private IEnumerator TypeLooseMessage()
    {
        if (looseMessageText == null)
        {
            yield break;
        }

        looseMessageText.text = looseMessage;
        looseMessageText.maxVisibleCharacters = 0;
        looseMessageText.ForceMeshUpdate();

        int characterCount =
            looseMessageText.textInfo.characterCount;

        if (charactersPerSecond <= 0f)
        {
            looseMessageText.maxVisibleCharacters =
                characterCount;

            yield break;
        }

        float secondsPerCharacter =
            1f / charactersPerSecond;

        for (int visibleCharacters = 1;
             visibleCharacters <= characterCount;
             visibleCharacters++)
        {
            looseMessageText.maxVisibleCharacters =
                visibleCharacters;

            yield return new WaitForSecondsRealtime(
                secondsPerCharacter
            );
        }
    }

    private void CacheEndScreenImages()
    {
        winEndGameImage =
            GetImageFromEndScreen(
                winEndGame,
                "WinEndGame"
            );

        looseEndGameImage =
            GetImageFromEndScreen(
                looseEndGame,
                "LooseEndGame"
            );
    }

    private Image GetImageFromEndScreen(
        GameObject endScreen,
        string fieldName
    )
    {
        if (endScreen == null)
        {
            Debug.LogWarning(
                $"{name}: {fieldName} is not assigned.",
                this
            );

            return null;
        }

        Image image =
            endScreen.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogWarning(
                $"{name}: {fieldName} needs an Image component on its root GameObject.",
                endScreen
            );
        }

        return image;
    }

    private void PrepareSequenceObjects()
    {
        PrepareEndScreen(
            winEndGame,
            winEndGameImage
        );

        PrepareEndScreen(
            looseEndGame,
            looseEndGameImage
        );

        if (looseTextBox != null)
        {
            looseTextBox.SetActive(false);
        }

        if (looseMessageText != null)
        {
            looseMessageText.text = string.Empty;
            looseMessageText.maxVisibleCharacters = 0;
        }

        HideButton(winHomeButton);
        HideButton(looseHomeButton);
    }

    private static void PrepareEndScreen(
        GameObject endScreen,
        Image image
    )
    {
        if (endScreen == null)
        {
            return;
        }

        if (image != null)
        {
            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }

        endScreen.SetActive(false);
    }

    private static void SetEndScreenActive(
        GameObject endScreen,
        bool active
    )
    {
        if (endScreen != null)
        {
            endScreen.SetActive(active);
        }
    }

    private static IEnumerator FadeImage(
        Image image,
        float startAlpha,
        float targetAlpha,
        float duration
    )
    {
        if (image == null)
        {
            yield break;
        }

        Color color = image.color;
        color.a = startAlpha;
        image.color = color;

        if (duration <= 0f)
        {
            color.a = targetAlpha;
            image.color = color;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / duration
                );

            color.a =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

            image.color = color;

            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }

    private static IEnumerator WaitRealtime(
        float duration
    )
    {
        if (duration <= 0f)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(
            duration
        );
    }

    private static void ShowButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(true);
        button.interactable = true;
    }

    private static void HideButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = false;
        button.gameObject.SetActive(false);
    }

    private static GameObject SpawnPrefab(
        GameObject prefab,
        Transform parent
    )
    {
        if (prefab == null)
        {
            return null;
        }

        return Instantiate(
            prefab,
            parent,
            false
        );
    }

    public void ReturnHome()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    private void SwitchToSystemCursor()
    {
        if (mouseConstraint != null)
        {
            mouseConstraint.DisableVirtualMouse();
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}