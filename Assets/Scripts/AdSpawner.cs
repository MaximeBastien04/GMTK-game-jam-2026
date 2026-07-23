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

    private void Start()
    {
        // Get the exact size of the prefab
        RectTransform prefabRect =
            adPrefab.GetComponent<RectTransform>();

        prefabAdSize = prefabRect.sizeDelta;

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
            adContainer
        );

        RectTransform adRect =
            newAd.GetComponent<RectTransform>();

        // Force the spawned ad to use the exact prefab dimensions
        adRect.sizeDelta = prefabAdSize;

        Image borderImage =
            newAd.GetComponent<Image>();

        Image crossImage =
            newAd.transform
                .Find("CloseButton")
                .GetComponent<Image>();

        // Select a random color variant
        int randomIndex = Random.Range(
            0,
            adColorVariants.Length
        );

        AdColorVariant selectedVariant =
            adColorVariants[randomIndex];

        // Apply sprites
        borderImage.sprite =
            selectedVariant.borderSprite;

        crossImage.sprite =
            selectedVariant.crossSprite;

        // Do NOT call SetNativeSize()
        // because we want to preserve the prefab size.

        // Get the screen dimensions
        Rect screenRect =
            adContainer.rect;

        float adWidth =
            adRect.rect.width;

        float adHeight =
            adRect.rect.height;

        // Calculate valid spawn area
        float minX =
            screenRect.xMin +
            adWidth / 2f;

        float maxX =
            screenRect.xMax -
            adWidth / 2f;

        float minY =
            screenRect.yMin +
            adHeight / 2f;

        float maxY =
            screenRect.yMax -
            adHeight / 2f;

        // Spawn completely inside the screen
        adRect.anchoredPosition = new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );
    }
}