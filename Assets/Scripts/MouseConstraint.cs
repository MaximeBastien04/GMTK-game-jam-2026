using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MouseConstraint : MonoBehaviour
{
    [Header("Virtual Cursor")]
    [SerializeField] private RectTransform virtualCursor;

    [Header("Computer Screen Area")]
    [SerializeField] private RectTransform computerScreen;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 1f;

    private Vector2 virtualMousePosition;

    private GameObject currentHoveredObject;

    private PointerEventData pointerEventData;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        virtualMousePosition = computerScreen.rect.center;

        pointerEventData =
            new PointerEventData(EventSystem.current);

        pointerEventData.button =
            PointerEventData.InputButton.Left;

        UpdateCursorPosition();
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        virtualMousePosition +=
            mouseDelta * mouseSensitivity;

        float minX =
            computerScreen.rect.xMin;

        float maxX =
            computerScreen.rect.xMax;

        float minY =
            computerScreen.rect.yMin;

        float maxY =
            computerScreen.rect.yMax;

        float cursorHalfWidth =
            virtualCursor.rect.width / 2f;

        float cursorHalfHeight =
            virtualCursor.rect.height / 2f;

        virtualMousePosition.x =
            Mathf.Clamp(
                virtualMousePosition.x,
                minX + cursorHalfWidth,
                maxX - cursorHalfWidth
            );

        virtualMousePosition.y =
            Mathf.Clamp(
                virtualMousePosition.y,
                minY + cursorHalfHeight,
                maxY - cursorHalfHeight
            );

        UpdateCursorPosition();

        UpdateUIHover();

        HandleMouseClick();
    }

    private void UpdateCursorPosition()
    {
        virtualCursor.anchoredPosition =
            virtualMousePosition;
    }

    private void UpdateUIHover()
    {
        pointerEventData.position =
            RectTransformUtility.WorldToScreenPoint(
                computerScreen
                    .GetComponentInParent<Canvas>()
                    .worldCamera,
                virtualCursor.position
            );

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            pointerEventData,
            results
        );

        GameObject hoveredObject = null;

        if (results.Count > 0)
        {
            hoveredObject =
                results[0].gameObject;
        }

        if (hoveredObject != currentHoveredObject)
        {
            if (currentHoveredObject != null)
            {
                ExecuteEvents.Execute(
                    currentHoveredObject,
                    pointerEventData,
                    ExecuteEvents.pointerExitHandler
                );
            }

            if (hoveredObject != null)
            {
                ExecuteEvents.Execute(
                    hoveredObject,
                    pointerEventData,
                    ExecuteEvents.pointerEnterHandler
                );
            }

            currentHoveredObject =
                hoveredObject;
        }
    }

    private void HandleMouseClick()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentHoveredObject != null)
            {
                ExecuteEvents.Execute(
                    currentHoveredObject,
                    pointerEventData,
                    ExecuteEvents.pointerDownHandler
                );
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (currentHoveredObject != null)
            {
                ExecuteEvents.Execute(
                    currentHoveredObject,
                    pointerEventData,
                    ExecuteEvents.pointerUpHandler
                );

                ExecuteEvents.Execute(
                    currentHoveredObject,
                    pointerEventData,
                    ExecuteEvents.pointerClickHandler
                );
            }
        }
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState =
            CursorLockMode.None;
    }
}