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
    }

    [Header("Ad Settings")]
    [SerializeField] private GameObject adPrefab;
    [SerializeField] private RectTransform adContainer;
    [SerializeField] private AdColorVariant[] adColorVariants;

    [Header("Spawn Interval")]
    [SerializeField] private float minimumSpawnTime = 0.5f;
    [SerializeField] private float maximumSpawnTime = 3f;

    private Vector2 prefabAdSize;
    private Vector3 prefabAdScale;

    private void Start()
    {
        if (adPrefab == null || adContainer == null)
        {
            Debug.LogError(
                "AdSpawner is missing the Ad Prefab or Ad Container reference."
            );

            enabled = false;
            return;
        }

        RectTransform prefabRect =
            adPrefab.GetComponent<RectTransform>();

        if (prefabRect == null)
        {
            Debug.LogError(
                "The ad prefab must have a RectTransform component."
            );

            enabled = false;
            return;
        }

        prefabAdSize = prefabRect.sizeDelta;
        prefabAdScale = prefabRect.localScale;

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
        GameObject newAd = Instantiate(
            adPrefab,
            adContainer,
            false
        );

        RectTransform adRect =
            newAd.GetComponent<RectTransform>();

        // Preserve the exact prefab dimensions and scale.
        adRect.sizeDelta = prefabAdSize;
        adRect.localScale = prefabAdScale;

        /*
         * Use one anchor point so anchoredPosition behaves consistently.
         * The pivot is left unchanged because it may be intentionally
         * configured on the prefab.
         */
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

        Transform closeButtonTransform =
            newAd.transform.Find("CloseButton");

        if (borderImage == null ||
            closeButtonTransform == null)
        {
            Debug.LogWarning(
                "The spawned ad is missing its border Image " +
                "or a child named CloseButton."
            );

            return;
        }

        Image crossImage =
            closeButtonTransform.GetComponent<Image>();

        if (crossImage == null)
        {
            Debug.LogWarning(
                "CloseButton does not have an Image component."
            );

            return;
        }

        int randomIndex =
            Random.Range(0, adColorVariants.Length);

        AdColorVariant selectedVariant =
            adColorVariants[randomIndex];

        borderImage.sprite =
            selectedVariant.borderSprite;

        crossImage.sprite =
            selectedVariant.crossSprite;
    }

    private void PositionAdInsideContainer(
        RectTransform adRect
    )
    {
        Rect containerRect = adContainer.rect;

        // Calculate the visible dimensions after applying local scale.
        float scaledWidth =
            adRect.rect.width *
            Mathf.Abs(adRect.localScale.x);

        float scaledHeight =
            adRect.rect.height *
            Mathf.Abs(adRect.localScale.y);

        /*
         * Account for the prefab's pivot.
         *
         * For a centered pivot:
         * leftExtent and rightExtent are each half the width.
         */
        float leftExtent =
            scaledWidth * adRect.pivot.x;

        float rightExtent =
            scaledWidth * (1f - adRect.pivot.x);

        float bottomExtent =
            scaledHeight * adRect.pivot.y;

        float topExtent =
            scaledHeight * (1f - adRect.pivot.y);

        // Valid pivot positions in the container's local coordinates.
        float minLocalX =
            containerRect.xMin + leftExtent;

        float maxLocalX =
            containerRect.xMax - rightExtent;

        float minLocalY =
            containerRect.yMin + bottomExtent;

        float maxLocalY =
            containerRect.yMax - topExtent;

        /*
         * The ad is too large if either minimum is greater than its
         * corresponding maximum.
         */
        if (minLocalX > maxLocalX ||
            minLocalY > maxLocalY)
        {
            Debug.LogWarning(
                "The scaled ad is larger than the Ad Container. " +
                "It cannot fit completely inside."
            );

            adRect.anchoredPosition = Vector2.zero;
            return;
        }

        Vector2 randomLocalPosition = new Vector2(
            Random.Range(minLocalX, maxLocalX),
            Random.Range(minLocalY, maxLocalY)
        );

        /*
         * anchoredPosition is relative to the selected anchor point.
         * Since the anchors are centered, calculate that anchor's
         * location inside the parent RectTransform.
         */
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