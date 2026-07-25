using System;
using System.Collections;
using UnityEngine;

public class CalendarManager : MonoBehaviour
{
    [Header("Calendar Paper")]
    [SerializeField] private CalendarPaper calendarPaperPrefab;
    [SerializeField] private Transform paperParent;

    [Header("Paper Positions")]
    [Tooltip(
        "Local position used by the currently visible paper."
    )]
    [SerializeField]
    private Vector3 frontPaperPosition =
        new Vector3(0f, 0f, 0.03f);

    [Tooltip(
        "Local position used when a new paper is spawned behind the current paper."
    )]
    [SerializeField]
    private Vector3 backPaperPosition =
        new Vector3(0f, 0f, 0.035f);

    [Min(0.01f)]
    [SerializeField] private float paperMoveDuration = 0.2f;

    [Header("Game Dates")]
    [SerializeField] private int startYear = 2020;
    [SerializeField] private int startMonth = 7;
    [SerializeField] private int startDay = 1;

    [SerializeField] private int finalYear = 2020;
    [SerializeField] private int finalMonth = 7;
    [SerializeField] private int finalDay = 31;

    public DateTime CurrentDate { get; private set; }

    public bool IsFinalDay =>
        CurrentDate.Date >= FinalDate.Date;

    public bool IsChangingDay { get; private set; }

    private DateTime FinalDate =>
        new DateTime(
            finalYear,
            finalMonth,
            finalDay
        );

    private CalendarPaper currentPaper;

    private void Start()
    {
        InitializeCalendar();
    }

    public void InitializeCalendar()
    {
        CurrentDate =
            new DateTime(
                startYear,
                startMonth,
                startDay
            );

        if (currentPaper != null)
        {
            Destroy(currentPaper.gameObject);
        }

        currentPaper =
            SpawnPaper(
                CurrentDate,
                frontPaperPosition
            );
    }

    public IEnumerator AdvanceToNextWorkday()
    {
        if (IsChangingDay || IsFinalDay)
        {
            yield break;
        }

        IsChangingDay = true;

        /*
         * Find the next playable weekday without creating calendar
         * papers for Saturday or Sunday.
         */
        DateTime nextWorkday =
            CurrentDate.AddDays(1);

        while (IsWeekend(nextWorkday))
        {
            nextWorkday =
                nextWorkday.AddDays(1);
        }

        if (nextWorkday.Date > FinalDate.Date)
        {
            IsChangingDay = false;
            yield break;
        }

        CalendarPaper paperToTear =
            currentPaper;

        if (paperToTear != null)
        {
            SetPaperZ(
                paperToTear,
                frontPaperPosition.z
            );
        }

        /*
         * Spawn only the next workday.
         *
         * Friday therefore creates Monday directly. No Saturday or
         * Sunday papers are instantiated and no weekend animations play.
         */
        CalendarPaper nextPaper =
            SpawnPaper(
                nextWorkday,
                backPaperPosition
            );

        CurrentDate =
            nextWorkday;

        currentPaper =
            nextPaper;

        /*
         * Start the old paper's tear animation independently so the
         * minigame does not wait for the calendar.
         */
        if (paperToTear != null)
        {
            StartCoroutine(
                paperToTear.TearOff()
            );
        }

        if (currentPaper != null)
        {
            yield return MovePaperToFront(
                currentPaper
            );
        }

        IsChangingDay = false;
    }

    private static void SetPaperZ(
        CalendarPaper paper,
        float zPosition
    )
    {
        if (paper == null)
        {
            return;
        }

        Transform paperTransform =
            paper.transform;

        Vector3 position =
            paperTransform.localPosition;

        position.z =
            zPosition;

        paperTransform.localPosition =
            position;
    }

    private IEnumerator MovePaperToFront(
        CalendarPaper paper
    )
    {
        if (paper == null)
        {
            yield break;
        }

        Transform paperTransform =
            paper.transform;

        Vector3 startPosition =
            paperTransform.localPosition;

        Vector3 targetPosition =
            startPosition;

        targetPosition.z =
            frontPaperPosition.z;

        float elapsedTime = 0f;

        while (elapsedTime < paperMoveDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / paperMoveDuration
                );

            float easedProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            paperTransform.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    easedProgress
                );

            yield return null;
        }

        paperTransform.localPosition =
            targetPosition;
    }

    private CalendarPaper SpawnPaper(
        DateTime date,
        Vector3 localPosition
    )
    {
        if (calendarPaperPrefab == null)
        {
            Debug.LogError(
                $"{name}: Calendar Paper Prefab is not assigned.",
                this
            );

            return null;
        }

        Transform parent =
            paperParent != null
                ? paperParent
                : transform;

        CalendarPaper paper =
            Instantiate(
                calendarPaperPrefab,
                parent
            );

        paper.transform.localPosition =
            localPosition;

        paper.transform.localRotation =
            Quaternion.identity;

        paper.transform.localScale =
            calendarPaperPrefab.transform.localScale;

        paper.SetDate(date);

        return paper;
    }

    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday ||
               date.DayOfWeek == DayOfWeek.Sunday;
    }
}