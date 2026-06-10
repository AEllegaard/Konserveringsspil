using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CellChopper : MonoBehaviour
{
    public bool chopActive = false;
    public bool allCellsChopped = false;

    public Image chopButton;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    public LayerMask soilLayer;
    public Transform jordklumb;

    [Header("Vildsvin Intact Tracking")]
    public Transform vildsvinCeller;
    private int totalVildsvinCells = 0;

    [Header("Completion")]
    public float chopThreshold = 0.8f;
    public GameObject completionText;
    public float completionTextDuration = 3f;

    private int totalCells = 0;
    private bool completionTriggered = false;

    public float IntactProgress => vildsvinCeller != null && totalVildsvinCells > 0
        ? (float)vildsvinCeller.childCount / totalVildsvinCells
        : 1f;

    void Start()
    {
        if (chopButton != null)
            chopButton.color = inactiveColor;

        if (completionText != null)
            completionText.SetActive(false);

        if (jordklumb != null)
            totalCells = jordklumb.childCount;

        if (vildsvinCeller != null)
            totalVildsvinCells = vildsvinCeller.childCount;
    }

    void Update()
    {
        if (!chopActive)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryChopCell();
        }
    }

    void TryChopCell()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, soilLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.mass = 1f;
                hit.collider.enabled = false;
                hit.collider.transform.SetParent(null);
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(Vector3.down * 500f, ForceMode.Impulse);

                CheckChopProgress();
            }
        }
    }
    public float DirtProgress => jordklumb != null && totalCells > 0
    ? (float)(totalCells - jordklumb.childCount) / totalCells
    : 0f;
    public void CheckChopProgress()
    {
        if (completionTriggered) return;
        if (jordklumb == null || totalCells == 0) return;

        int remaining = jordklumb.childCount;
        int chopped = totalCells - remaining;
        float ratio = (float)chopped / totalCells;

        if (ratio >= chopThreshold)
        {
            completionTriggered = true;
            allCellsChopped = true;

            DropRemainingCells();
            StartCoroutine(ShowCompletionText());
        }
    }

    void DropRemainingCells()
    {
        if (jordklumb == null) return;

        List<Transform> remaining = new List<Transform>();
        foreach (Transform child in jordklumb)
            remaining.Add(child);

        foreach (Transform celle in remaining)
        {
            Rigidbody rb = celle.GetComponent<Rigidbody>();
            if (rb == null)
                rb = celle.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 1f;

            Collider col = celle.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            celle.SetParent(null);
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.down * 500f, ForceMode.Impulse);
        }
    }

    IEnumerator ShowCompletionText()
    {
        if (completionText == null) yield break;
        completionText.SetActive(true);
        yield return new WaitForSeconds(completionTextDuration);
        completionText.SetActive(false);
    }

    public void ToggleChop()
    {
        chopActive = !chopActive;
        if (chopButton != null)
            chopButton.color = chopActive ? activeColor : inactiveColor;
    }

    public void UpdateButtonColor()
    {
        if (chopButton != null)
            chopButton.color = chopActive ? activeColor : inactiveColor;
    }
}