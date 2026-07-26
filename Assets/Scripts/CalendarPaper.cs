using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CalendarPaper : MonoBehaviour
{
    private DateTime displayedDate;

    [Header("Text")]
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private TMP_Text dayNumberText;
    [SerializeField] private TMP_Text dayText;

    [Header("Tear Animation")]
    [SerializeField] private Animator animator;

    [Tooltip("Animator trigger used for the first tear animation.")]
    [SerializeField] private string firstTearTriggerName = "Tear";

    [Tooltip("Animator trigger used for the second tear animation.")]
    [SerializeField] private string secondTearTriggerName = "TearAlternate";

    [Min(0f)]
    [SerializeField] private float tearAnimationDuration = 0.75f;

    public void SetDate(DateTime date)
    {
        displayedDate = date;

        if (monthText != null)
        {
            monthText.text = "JULY";
        }

        if (dayNumberText != null)
        {
            dayNumberText.text =
                date.Day.ToString();
        }

        if (dayText != null)
        {
            dayText.text =
                GetAbbreviatedDay(date.DayOfWeek);
        }
    }

    public IEnumerator TearOff()
    {
        PlayTearAnimation();

        if (tearAnimationDuration > 0f)
        {
            yield return new WaitForSeconds(
                tearAnimationDuration
            );
        }

        Destroy(gameObject);
    }

    private void PlayTearAnimation()
    {
        if (animator == null)
        {
            return;
        }

        /*
         * Alternate by calendar day:
         * odd-numbered dates use the first animation,
         * even-numbered dates use the second animation.
         *
         * This stays consistent even when weekend pages animate
         * in quick succession.
         */
        bool useFirstAnimation =
            displayedDate.Day % 2 != 0;

        string triggerName =
            useFirstAnimation
                ? firstTearTriggerName
                : secondTearTriggerName;

        if (string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        /*
         * Clear both triggers first so a previously queued trigger
         * cannot accidentally select the wrong transition.
         */
        if (!string.IsNullOrWhiteSpace(firstTearTriggerName))
        {
            animator.ResetTrigger(
                firstTearTriggerName
            );
        }

        if (!string.IsNullOrWhiteSpace(secondTearTriggerName))
        {
            animator.ResetTrigger(
                secondTearTriggerName
            );
        }

        animator.SetTrigger(triggerName);
    }

    private static string GetAbbreviatedDay(
        DayOfWeek dayOfWeek
    )
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Mon",
            DayOfWeek.Tuesday => "Tue",
            DayOfWeek.Wednesday => "Wed",
            DayOfWeek.Thursday => "Thu",
            DayOfWeek.Friday => "Fri",
            DayOfWeek.Saturday => "Sat",
            DayOfWeek.Sunday => "Sun",
            _ => string.Empty
        };
    }

}