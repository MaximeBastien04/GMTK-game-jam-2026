using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Range(0f, 1f)]
    [SerializeField] private float targetVolume = 0.6f;

    [Min(0f)]
    [SerializeField] private float fadeInDuration = 0.75f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private bool hasStarted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = 0f;
    }

    /// <summary>
    /// Connect this method to the login button's OnClick event.
    /// The soundtrack starts only the first time the button is pressed.
    /// </summary>
    public void StartBackgroundMusic()
    {
        if (hasStarted)
        {
            return;
        }

        if (backgroundMusic == null)
        {
            Debug.LogWarning(
                $"{name}: No background music clip is assigned.",
                this
            );

            return;
        }

        hasStarted = true;

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.Play();

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            FadeVolume(
                0f,
                targetVolume,
                fadeInDuration
            )
        );
    }

    private IEnumerator FadeVolume(
        float startVolume,
        float endVolume,
        float duration
    )
    {
        if (duration <= 0f)
        {
            audioSource.volume = endVolume;
            fadeCoroutine = null;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / duration
                );

            audioSource.volume =
                Mathf.Lerp(
                    startVolume,
                    endVolume,
                    progress
                );

            yield return null;
        }

        audioSource.volume = endVolume;
        fadeCoroutine = null;
    }
}
