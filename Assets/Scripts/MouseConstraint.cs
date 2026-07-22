using UnityEngine;
using UnityEngine.InputSystem;

public class MouseConstraint : MonoBehaviour
{
    [Header("Virtual Cursor")]
    [SerializeField] private RectTransform virtualCursor;

    [Header("Computer Screen Area")]
    [SerializeField] private RectTransform computerScreen;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 1f;

    private Vector2 virtualMousePosition;

    private void Start()
    {
        // Start the virtual mouse in the center of the computer screen
        virtualMousePosition = computerScreen.rect.center;

        UpdateCursorPosition();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        // Read physical mouse movement
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Move virtual mouse
        virtualMousePosition += mouseDelta * mouseSensitivity;

        // Calculate boundaries
        float minX = computerScreen.rect.xMin;
        float maxX = computerScreen.rect.xMax;

        float minY = computerScreen.rect.yMin;
        float maxY = computerScreen.rect.yMax;

        // Account for cursor size
        float cursorHalfWidth = virtualCursor.rect.width / 2f;
        float cursorHalfHeight = virtualCursor.rect.height / 2f;

        // Clamp cursor inside screen
        virtualMousePosition.x = Mathf.Clamp(
            virtualMousePosition.x,
            minX + cursorHalfWidth,
            maxX - cursorHalfWidth
        );

        virtualMousePosition.y = Mathf.Clamp(
            virtualMousePosition.y,
            minY + cursorHalfHeight,
            maxY - cursorHalfHeight
        );

        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        virtualCursor.anchoredPosition = virtualMousePosition;
    }
}