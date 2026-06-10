using UnityEngine;
using UnityEngine.UI;

public class ObjectRotator : MonoBehaviour
{
    public float sensitivity = 0.4f;

    public bool uiButtonActive = false;
    public Image button;             
    public Color activeColor;
    public Color inactiveColor;

    private bool isDragging = false;
    private Vector3 lastMousePos;
    private float xRotation = 0f;

    void Start()
    {
        button.color = inactiveColor;
    }

    void Update()
    {
        if (!uiButtonActive)
            return;

        if (Input.GetKey(KeyCode.C))
        {
            isDragging = false;
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            isDragging = false;
        }

        if (isDragging)
{
    Vector3 delta = Input.mousePosition - lastMousePos;

    // Y-rotation på parent
    transform.Rotate(Vector3.up, -delta.x * sensitivity, Space.World);

    // X-rotation på ALLE children
    xRotation -= delta.y * sensitivity;
    xRotation = Mathf.Clamp(xRotation, -80f, 80f);

    foreach (Transform t in transform)
    {
        t.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    lastMousePos = Input.mousePosition;
}

    }

    void OnMouseDown()
    {
        if (!uiButtonActive) return;

        if (!Input.GetKey(KeyCode.C))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    public void ToggleUIActive()
    {
        uiButtonActive = !uiButtonActive;

        // Skift farve manuelt – ingen interactable
        button.color = uiButtonActive ? activeColor : inactiveColor;
    }

    public void UpdateButtonColor()
{
    button.color = uiButtonActive ? activeColor : inactiveColor;
}
}
