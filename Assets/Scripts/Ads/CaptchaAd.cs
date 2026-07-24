using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaptchaAd : Ad
{
    [Serializable]
    public class CaptchaImageSet
    {
        [Tooltip(
            "Name shown to the player, such as Blue, Circle, or Number 4."
        )]
        public string displayName;

        [Tooltip("Sprite that the player must click.")]
        public Sprite correctSprite;

        [Tooltip("Sprite that the player must not click.")]
        public Sprite wrongSprite;
    }

    [Serializable]
    public class CaptchaSlot
    {
        [Tooltip(
            "The complete slot object. This will be hidden when clicked correctly."
        )]
        public GameObject slotObject;

        [Tooltip("The Image displaying the CAPTCHA sprite.")]
        public Image image;

        [Tooltip("The Button placed over the CAPTCHA image.")]
        public Button button;
    }

    protected override int RewardAmount => 6;

    [Header("Captcha Slots")]
    [SerializeField] private CaptchaSlot[] captchaSlots;

    [Header("Captcha Image Sets")]
    [SerializeField]
    private List<CaptchaImageSet> imageSets =
        new List<CaptchaImageSet>();

    [Header("Correct Image Amount")]
    [Min(1)]
    [SerializeField] private int minimumCorrectImages = 3;

    [Min(1)]
    [SerializeField] private int maximumCorrectImages = 5;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.1f;

    [Header("UI")]
    [SerializeField] private TMP_Text dispNameText;

    private bool[] correctSlots;
    private bool[] clickedSlots;

    private int correctImagesRequired;
    private int correctImagesClicked;

    private CaptchaImageSet selectedImageSet;

    private void Awake()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        GenerateCaptcha();
    }

    private bool ValidateSetup()
    {
        if (captchaSlots == null || captchaSlots.Length == 0)
        {
            Debug.LogError(
                $"{name}: No CAPTCHA slots have been assigned.",
                this
            );

            return false;
        }

        if (captchaSlots.Length != 9)
        {
            Debug.LogWarning(
                $"{name}: A CAPTCHA normally needs exactly 9 slots. " +
                $"The current amount is {captchaSlots.Length}.",
                this
            );
        }

        if (imageSets == null || imageSets.Count == 0)
        {
            Debug.LogError(
                $"{name}: No CAPTCHA image sets have been assigned.",
                this
            );

            return false;
        }

        for (int i = 0; i < captchaSlots.Length; i++)
        {
            CaptchaSlot slot = captchaSlots[i];

            if (slot == null ||
                slot.slotObject == null ||
                slot.image == null ||
                slot.button == null)
            {
                Debug.LogError(
                    $"{name}: CAPTCHA slot {i} is missing its " +
                    "Slot Object, Image, or Button reference.",
                    this
                );

                return false;
            }
        }

        for (int i = 0; i < imageSets.Count; i++)
        {
            CaptchaImageSet imageSet = imageSets[i];

            if (imageSet == null ||
                imageSet.correctSprite == null ||
                imageSet.wrongSprite == null)
            {
                Debug.LogError(
                    $"{name}: CAPTCHA image set {i} is missing " +
                    "a correct or wrong sprite.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private void GenerateCaptcha()
    {
        correctImagesClicked = 0;

        correctSlots =
            new bool[captchaSlots.Length];

        clickedSlots =
            new bool[captchaSlots.Length];

        selectedImageSet =
            imageSets[UnityEngine.Random.Range(0, imageSets.Count)];

        int minimum = Mathf.Clamp(
            minimumCorrectImages,
            1,
            captchaSlots.Length
        );

        int maximum = Mathf.Clamp(
            maximumCorrectImages,
            minimum,
            captchaSlots.Length
        );

        // Integer Random.Range uses an exclusive maximum,
        // so add 1 to include maximumCorrectImages.
        correctImagesRequired =
            UnityEngine.Random.Range(minimum, maximum + 1);

        List<int> shuffledIndices = new List<int>();

        for (int i = 0; i < captchaSlots.Length; i++)
        {
            shuffledIndices.Add(i);
        }

        Shuffle(shuffledIndices);

        // Mark a random selection of slots as correct.
        for (int i = 0; i < correctImagesRequired; i++)
        {
            int correctIndex = shuffledIndices[i];
            correctSlots[correctIndex] = true;
        }

        ConfigureSlots();
        UpdateText();
    }

    private void ConfigureSlots()
    {
        for (int i = 0; i < captchaSlots.Length; i++)
        {
            int slotIndex = i;
            CaptchaSlot slot = captchaSlots[slotIndex];

            slot.slotObject.SetActive(true);
            slot.button.interactable = true;

            Color imageColor =
                slot.image.color;

            imageColor.a = 1f;

            slot.image.color =
                imageColor;

            slot.image.sprite = correctSlots[slotIndex]
                ? selectedImageSet.correctSprite
                : selectedImageSet.wrongSprite;

            slot.button.onClick.RemoveAllListeners();

            slot.button.onClick.AddListener(
                () => HandleSlotClicked(slotIndex)
            );
        }
    }

    private void HandleSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= captchaSlots.Length)
        {
            return;
        }

        if (clickedSlots[slotIndex])
        {
            return;
        }

        if (!correctSlots[slotIndex])
        {
            return;
        }

        clickedSlots[slotIndex] = true;
        correctImagesClicked++;

        CaptchaSlot selectedSlot =
            captchaSlots[slotIndex];

        selectedSlot.button.interactable = false;

        StartCoroutine(
            FadeOutSlot(selectedSlot)
        );

        UpdateText();

        if (correctImagesClicked >= correctImagesRequired)
        {
            StartCoroutine(
                CompleteCaptchaAfterFade()
            );
        }
    }

    private void CompleteCaptcha()
    {
        // CloseAd rewards the player and destroys the ad.
        CloseAd();
    }

    private void UpdateText()
    {
        if (dispNameText != null)
        {
            dispNameText.text =
                selectedImageSet.displayName.ToUpper();
        }
    }

    private void Shuffle(List<int> indices)
    {
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int randomIndex =
                UnityEngine.Random.Range(0, i + 1);

            (indices[i], indices[randomIndex]) =
                (indices[randomIndex], indices[i]);
        }
    }

    private void OnDestroy()
    {
        if (captchaSlots == null)
        {
            return;
        }

        foreach (CaptchaSlot slot in captchaSlots)
        {
            if (slot?.button != null)
            {
                slot.button.onClick.RemoveAllListeners();
            }
        }
    }

    private IEnumerator FadeOutSlot(
    CaptchaSlot slot
)
    {
        float elapsedTime = 0f;

        Color startColor =
            slot.image.color;

        Color targetColor =
            startColor;

        targetColor.a = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / fadeDuration
                );

            slot.image.color =
                Color.Lerp(
                    startColor,
                    targetColor,
                    progress
                );

            yield return null;
        }

        slot.image.color =
            targetColor;
    }

    private IEnumerator CompleteCaptchaAfterFade()
    {
        yield return new WaitForSeconds(
            fadeDuration
        );

        CloseAd();
    }
}