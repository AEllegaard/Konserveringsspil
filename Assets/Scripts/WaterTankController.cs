using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class WaterTankController : MonoBehaviour
{
    [Header("Vandkar bevægelse")]
    public Transform vandkar;
    public Vector3 raisedOffset = new Vector3(0f, 3f, 0f);
    public float raiseDuration = 1.5f;

    [Header("Desaltering")]
    public float desaltDuration = 10f;

    [Header("Particle System")]
    public ParticleSystem waterParticles;

    [Header("Forudsætninger")]
    public CellChopper chopper;
    public ObjectCleaner cleaner;
    public GameObject notReadyText;   // tekst der vises hvis opgaver ikke er færdige
    public GameObject alreadyDoneText; // tekst der vises hvis man prøver igen efter completion
    public float feedbackTextDuration = 2f;
    public Renderer targetRenderer;

    [Header("UI")]
    public Button activateButton;
    public Image waterButton;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;
    public Image loadingBarFill;
    public GameObject loadingBarRoot;
    public TextMeshProUGUI percentText;
    public TextMeshProUGUI barLabel;
    public GameObject completionTextRoot;

    public bool waterActive = false;
    public bool isComplete = false;

    private bool hasRunOnce = false;
    private bool isRunning = false;
    private Vector3 startPos;

    void Start()
    {
        if (vandkar != null)
            startPos = vandkar.localPosition;

        if (loadingBarRoot != null)
            loadingBarRoot.SetActive(false);

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        if (percentText != null)
            percentText.text = "0%";

        if (waterParticles != null)
            waterParticles.Stop();

        if (completionTextRoot != null)
            completionTextRoot.SetActive(false);

        if (notReadyText != null)
            notReadyText.SetActive(false);

        if (alreadyDoneText != null)
            alreadyDoneText.SetActive(false);

        if (waterButton != null)
            waterButton.color = inactiveColor;
    }

    public void UpdateButtonColor()
    {
        if (waterButton != null)
            waterButton.color = waterActive ? activeColor : inactiveColor;
    }

    public void OnActivateButtonPressed()
    {
        // Allerede kørt én gang → vis "allerede gjort" tekst
        if (hasRunOnce && isComplete)
        {
            StartCoroutine(ShowFeedbackText(alreadyDoneText));
            return;
        }

        // Forudsætninger ikke opfyldt → vis "ikke klar" tekst
        if (!chopper.allCellsChopped || !cleaner.cleaningComplete)
        {
            StartCoroutine(ShowFeedbackText(notReadyText));
            return;
        }

        if (isRunning) return;

        waterActive = true;
        UpdateButtonColor();
        activateButton.interactable = false;
        StartCoroutine(RunSequence());
    }

    public void Deactivate()
    {
        waterActive = false;
        UpdateButtonColor();
    }

    IEnumerator ShowFeedbackText(GameObject textObj)
    {
        if (textObj == null) yield break;
        textObj.SetActive(true);
        yield return new WaitForSeconds(feedbackTextDuration);
        textObj.SetActive(false);
    }

    IEnumerator RunSequence()
    {
        isRunning = true;
        hasRunOnce = true;

        // --- 1. Hæv vandkaret smooth ---
        Vector3 targetPos = startPos + raisedOffset;
        float t = 0f;

        while (t < raiseDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / raiseDuration);
            vandkar.localPosition = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }

        vandkar.localPosition = targetPos;

        // --- 2. Start particles og loading bar ---
        if (waterParticles != null)
            waterParticles.Play();

        if (loadingBarRoot != null)
            loadingBarRoot.SetActive(true);

        // --- 3. Fyld loading bar ---
        float elapsed = 0f;

        while (elapsed < desaltDuration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / desaltDuration);

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = fill;

            if (percentText != null)
                percentText.text = Mathf.RoundToInt(fill * 100f) + "%";

            yield return null;
        }

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 1f;

        if (percentText != null)
            percentText.text = "100%";

        if (waterParticles != null)
            waterParticles.Stop();

        OnDesaltingComplete();
    }

    void OnDesaltingComplete()
{
    isRunning = false;
    isComplete = true;

    if (percentText != null)
        percentText.gameObject.SetActive(false);

    if (barLabel != null)
        barLabel.gameObject.SetActive(false);

    if (completionTextRoot != null)
        completionTextRoot.SetActive(true);

    activateButton.interactable = true;
    
    StartCoroutine(ResetAll()); // tilføj denne linje
}

    IEnumerator ResetAll()
{
    // isComplete = false  <-- SLET DENNE LINJE
    waterActive = false;
    UpdateButtonColor();
    activateButton.interactable = false;

    if (completionTextRoot != null)
        completionTextRoot.SetActive(false);

    if (loadingBarRoot != null)
        loadingBarRoot.SetActive(false);

    if (loadingBarFill != null)
        loadingBarFill.fillAmount = 0f;

    if (percentText != null)
    {
        percentText.text = "0%";
        percentText.gameObject.SetActive(true);
    }

    if (barLabel != null)
        barLabel.gameObject.SetActive(true);

    // --- Sænk vandkaret smooth ---
    Vector3 raisedPos = startPos + raisedOffset;
    float t = 0f;

    while (t < raiseDuration)
    {
        t += Time.deltaTime;
        float progress = Mathf.SmoothStep(0f, 1f, t / raiseDuration);
        vandkar.localPosition = Vector3.Lerp(raisedPos, startPos, progress);
        yield return null;
    }

    vandkar.localPosition = startPos;
    activateButton.interactable = true; // knappen aktiv igen, men isComplete = true stopper den
}
}