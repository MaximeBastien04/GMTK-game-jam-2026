using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject[] adPrefabs;
    [SerializeField] private RectTransform adContainer;
    [SerializeField] private AdColorVariant[] adColorVariants;

    [Header("Spawn Interval")]
    [SerializeField] private float minimumSpawnTime = 0.5f;
    [SerializeField] private float maximumSpawnTime = 3f;

    private void Start()
    {
        if (adContainer == null)
        {
            Debug.LogError(
                "AdSpawner is missing the Ad Container reference.",
                this
            );

            enabled = false;
            return;
        }

        if (adPrefabs == null || adPrefabs.Length == 0)
        {
            Debug.LogError(
                "AdSpawner does not have any ad prefabs assigned.",
                this
            );

            enabled = false;
            return;
        }

        foreach (GameObject prefab in adPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError(
                    "One of the Ad Prefab entries is empty.",
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

        StartCoroutine(SpawnAds());
    }

    private IEnumerator SpawnAds()
    {
        while (true)
        {
            float waitTime = Random.Range(
                minimumSpawnTime,
                maximumSpawnTime
            );

            yield return new WaitForSeconds(waitTime);

            SpawnAd();
        }
    }

    private void SpawnAd()
    {
        GameObject selectedPrefab =
            adPrefabs[Random.Range(0, adPrefabs.Length)];

        RectTransform prefabRect =
            selectedPrefab.GetComponent<RectTransform>();

        GameObject newAd = Instantiate(
            selectedPrefab,
            adContainer,
            false
        );

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

    // Change the inside button sprite when this ad has one.
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
}