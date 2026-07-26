using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdSpawner : MonoBehaviour
{
    [System.Serializable]
    public class AdColorVariant
    {
        public Sprite borderSprite;
        public Sprite crossSprite;
        public Sprite insideButtonSprite;
    }

    [Header("Ad Settings")]
    [SerializeField] private RectTransform adContainer;
    [SerializeField] private AdColorVariant[] adColorVariants;

    [Header("Calendar Progression")]
    [SerializeField] private CalendarManager calendarManager;

    [Tooltip("Available from July 1.")]
    [SerializeField] private GameObject closeButtonAdPrefab;

    [Tooltip("Available from July 1.")]
    [SerializeField] private GameObject insideButtonAdPrefab;

    [Tooltip("Available from July 13.")]
    [SerializeField] private GameObject multipleClickAdPrefab;

    [Tooltip("Available from July 20.")]
    [SerializeField] private GameObject captchaAdPrefab;


    [Header("Spawn Interval")]
    private Coroutine spawnRoutine;
    [SerializeField] private float minimumSpawnTime = 0.5f;
    [SerializeField] private float phaseStartMaximumSpawnTime = 2f;
    [SerializeField] private float phaseEndMaximumSpawnTime = 1f;


    [Header("Ad Spawn Sound")]
    [SerializeField] private AudioClip adSpawnClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private AudioSource audioSource;

    private readonly List<Ad> spawnedAds = new List<Ad>();


    private void Start()
    {

        audioSource = GetComponent<AudioSource>();

        if (adContainer == null)
        {
            Debug.LogError(
                "AdSpawner is missing the Ad Container reference.",
                this
            );

            enabled = false;
            return;
        }

        if (calendarManager == null)
        {
            Debug.LogError(
                "AdSpawner is missing the CalendarManager reference.",
                this
            );

            enabled = false;
            return;
        }

        GameObject[] progressionPrefabs =
        {
            closeButtonAdPrefab,
            insideButtonAdPrefab,
            multipleClickAdPrefab,
            captchaAdPrefab
        };

        foreach (GameObject prefab in progressionPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError(
                    "One of the progression ad prefabs is not assigned.",
                    this
                );

                enabled = false;
                return;
            }

            if (prefab.GetComponent<RectTransform>() == null)
            {
                Debug.LogError(
                    $"The ad prefab '{prefab.name}' must have a RectTransform.",
                    prefab
                );

                enabled = false;
                return;
            }
        }
    }

    public void StartSpawning()
    {
        if (!enabled || spawnRoutine != null)
        {
            return;
        }

        spawnRoutine = StartCoroutine(SpawnAds());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public void CloseAllAds()
    {
        RemoveMissingAds();

        List<Ad> adsToClose =
            new List<Ad>(spawnedAds);

        foreach (Ad ad in adsToClose)
        {
            if (ad != null)
            {
                ad.ForceCloseWithoutReward();
            }
        }

        spawnedAds.Clear();
    }

    private void OnDisable()
    {
        StopSpawning();
    }


    private IEnumerator SpawnAds()
    {
        while (true)
        {
            float currentMaximumSpawnTime =
                GetMaximumSpawnTimeForCurrentDay();

            float waitTime = Random.Range(
                minimumSpawnTime,
                Mathf.Max(minimumSpawnTime, currentMaximumSpawnTime)
            );

            yield return new WaitForSeconds(waitTime);

            SpawnAd();
        }
    }

    private void SpawnAd()
    {
        GameObject[] availablePrefabs =
            GetAvailablePrefabsForCurrentDay();

        GameObject selectedPrefab =
            availablePrefabs[
                Random.Range(0, availablePrefabs.Length)
            ];

        RectTransform prefabRect =
            selectedPrefab.GetComponent<RectTransform>();

        GameObject newAd = Instantiate(selectedPrefab, adContainer, false);

        RectTransform adRect =
            newAd.GetComponent<RectTransform>();

        // Preserve the selected prefab's exact dimensions and scale.
        adRect.sizeDelta = prefabRect.sizeDelta;
        adRect.localScale = prefabRect.localScale;

        // Use a fixed anchor so random positioning is predictable.
        adRect.anchorMin = new Vector2(0.5f, 0.5f);
        adRect.anchorMax = new Vector2(0.5f, 0.5f);

        ApplyRandomColor(newAd);

        PositionAdInsideContainer(adRect);

        Ad adComponent = newAd.GetComponent<Ad>();

        if (adComponent != null)
        {
            spawnedAds.Add(adComponent);

            adComponent.AdClosed += HandleAdClosed;
        }

        if (ItemEffectManager.Instance != null)
        {
            ItemEffectManager.Instance.ApplyActiveEffects(newAd);
        }

        PlaySpawnSound();
    }

    private GameObject[] GetAvailablePrefabsForCurrentDay()
    {
        int currentDay = calendarManager.CurrentDate.Day;

        if (currentDay >= 20)
        {
            return new[]
            {
                closeButtonAdPrefab,
                insideButtonAdPrefab,
                multipleClickAdPrefab,
                captchaAdPrefab
            };
        }

        if (currentDay >= 13)
        {
            return new[]
            {
                closeButtonAdPrefab,
                insideButtonAdPrefab,
                multipleClickAdPrefab
            };
        }

        return new[]
        {
            closeButtonAdPrefab,
            insideButtonAdPrefab
        };
    }

    private float GetMaximumSpawnTimeForCurrentDay()
    {
        int currentDay = calendarManager.CurrentDate.Day;

        if (currentDay <= 10)
        {
            return InterpolateMaximumSpawnTime(currentDay, 1, 10);
        }

        if (currentDay < 13)
        {
            return phaseEndMaximumSpawnTime;
        }

        if (currentDay <= 17)
        {
            return InterpolateMaximumSpawnTime(currentDay, 13, 17);
        }

        if (currentDay < 20)
        {
            return phaseEndMaximumSpawnTime;
        }

        return InterpolateMaximumSpawnTime(currentDay, 20, 31);
    }

    private float InterpolateMaximumSpawnTime(
        int currentDay,
        int phaseStartDay,
        int phaseEndDay
    )
    {
        float progress = Mathf.InverseLerp(
            phaseStartDay,
            phaseEndDay,
            currentDay
        );

        return Mathf.Lerp(
            phaseStartMaximumSpawnTime,
            phaseEndMaximumSpawnTime,
            progress
        );
    }

    private void ApplyRandomColor(GameObject newAd)
    {
        if (adColorVariants == null ||
            adColorVariants.Length == 0)
        {
            return;
        }

        Image borderImage =
            newAd.GetComponent<Image>();

        if (borderImage == null)
        {
            Debug.LogWarning(
                $"The spawned ad '{newAd.name}' has no Image on its root.",
                newAd
            );

            return;
        }

        int randomIndex =
            Random.Range(0, adColorVariants.Length);

        AdColorVariant selectedVariant =
            adColorVariants[randomIndex];

        // Change the border sprite.
        if (selectedVariant.borderSprite != null)
        {
            borderImage.sprite =
                selectedVariant.borderSprite;
        }

        // Change the cross button sprite when this ad has one.
        Transform closeButtonTransform =
            newAd.transform.Find("CloseButton");

        if (closeButtonTransform != null)
        {
            Image crossImage =
                closeButtonTransform.GetComponent<Image>();

            if (crossImage != null &&
                selectedVariant.crossSprite != null)
            {
                crossImage.sprite =
                    selectedVariant.crossSprite;
            }
        }

        /*
         * CloseButtonAd has two separate Image components for its
         * inside button. Let the ad update both with the same sprite.
         */
        CloseButtonAd closeButtonAd =
            newAd.GetComponent<CloseButtonAd>();

        if (closeButtonAd != null)
        {
            closeButtonAd.ApplyInsideButtonSprite(
                selectedVariant.insideButtonSprite
            );
        }
        else
        {
            // Keep support for other ad types with an InsideButton child.
            Transform insideButtonTransform =
                newAd.transform.Find("InsideButton");

            if (insideButtonTransform != null)
            {
                Image insideButtonImage =
                    insideButtonTransform.GetComponent<Image>();

                if (insideButtonImage != null &&
                    selectedVariant.insideButtonSprite != null)
                {
                    insideButtonImage.sprite =
                        selectedVariant.insideButtonSprite;
                }
            }
        }
    }
    private void PositionAdInsideContainer(
        RectTransform adRect
    )
    {
        Rect containerRect =
            adContainer.rect;

        float scaledWidth =
            adRect.rect.width *
            Mathf.Abs(adRect.localScale.x);

        float scaledHeight =
            adRect.rect.height *
            Mathf.Abs(adRect.localScale.y);

        float leftExtent =
            scaledWidth * adRect.pivot.x;

        float rightExtent =
            scaledWidth * (1f - adRect.pivot.x);

        float bottomExtent =
            scaledHeight * adRect.pivot.y;

        float topExtent =
            scaledHeight * (1f - adRect.pivot.y);

        float minLocalX =
            containerRect.xMin + leftExtent;

        float maxLocalX =
            containerRect.xMax - rightExtent;

        float minLocalY =
            containerRect.yMin + bottomExtent;

        float maxLocalY =
            containerRect.yMax - topExtent;

        if (minLocalX > maxLocalX ||
            minLocalY > maxLocalY)
        {
            Debug.LogWarning(
                $"The ad '{adRect.name}' is larger than the Ad Container.",
                adRect
            );

            adRect.anchoredPosition = Vector2.zero;
            return;
        }

        Vector2 randomLocalPosition = new Vector2(
            Random.Range(minLocalX, maxLocalX),
            Random.Range(minLocalY, maxLocalY)
        );

        Vector2 anchorReference = new Vector2(
            containerRect.xMin +
            containerRect.width * adRect.anchorMin.x,

            containerRect.yMin +
            containerRect.height * adRect.anchorMin.y
        );

        adRect.anchoredPosition =
            randomLocalPosition - anchorReference;
    }

    private void HandleAdClosed(Ad closedAd)
    {
        if (closedAd == null)
        {
            return;
        }

        closedAd.AdClosed -=
            HandleAdClosed;

        spawnedAds.Remove(closedAd);
    }

    public int CloseLastAdsAndReward(int amount)
    {
        RemoveMissingAds();

        int amountToClose =
            Mathf.Min(amount, spawnedAds.Count);

        if (amountToClose <= 0)
        {
            return 0;
        }

        List<Ad> adsToClose =
            new List<Ad>();

        // Start at the end because those are the newest ads.
        for (int i = spawnedAds.Count - 1;
             i >= 0 &&
             adsToClose.Count < amountToClose;
             i--)
        {
            Ad ad = spawnedAds[i];

            if (ad != null &&
                !ad.HasBeenClosed)
            {
                adsToClose.Add(ad);
            }
        }

        foreach (Ad ad in adsToClose)
        {
            ad.ForceCloseAndReward();
        }

        return adsToClose.Count;
    }

    private void RemoveMissingAds()
    {
        spawnedAds.RemoveAll(
            ad => ad == null || ad.HasBeenClosed
        );
    }

    public void PlaySpawnSound()
    {
        if (adSpawnClip == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(
            pitchRange.x,
            pitchRange.y
        );

        audioSource.PlayOneShot(
            adSpawnClip,
            volume
        );
    }
}