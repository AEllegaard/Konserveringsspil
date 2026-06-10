using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public ObjectRotator rotator;
    public CellChopper chopper;
    public XrayToggle xray;
    public ObjectCleaner cleaner;
    public WaterTankController waterTank;

    public GameObject xrayPopout;
    public GameObject hakPopout;
    public GameObject brushPopout;
    public GameObject waterPopout;
    public WetPainter wetPainter;
    public GameObject sprayPopout;

    [Header("Progress Bars")]
    public Image intactBar;
    public TextMeshProUGUI PercentTextInt;

    public Image conservationBar;
    public TextMeshProUGUI percentTextCons;

    [Header("Scene Transition")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;
    public string endSceneName = "EndScene";

    private bool endTriggered = false;

    void Update()
    {
        if (intactBar != null)
            intactBar.fillAmount = cleaner.IntactProgress;
            if (PercentTextInt != null)
    PercentTextInt.text = Mathf.RoundToInt(cleaner.IntactProgress * 100f) + "%";

        if (conservationBar != null)
        {
            float progress = GetOverallProgress();
            conservationBar.fillAmount = progress;

            if (percentTextCons != null)
                percentTextCons.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (progress >= 1f && !endTriggered)
            {
                endTriggered = true;
                StartCoroutine(FadeToEnd());
            }
        }
    }

    IEnumerator FadeToEnd()
    {
        yield return new WaitForSeconds(3f);

        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeGroup.alpha = 1f;
        }

        SceneManager.LoadScene(endSceneName);
    }

    public float GetOverallProgress()
    {
        float chopProgress = chopper.DirtProgress;
        float conserved    = cleaner.ConservationProgress;
        float sprayed      = wetPainter.SprayProgress * 2f;
        float desalted     = waterTank.isComplete ? 0.5f : 0f;

        return (chopProgress + conserved + sprayed + desalted) / 3.5f;
    }

    public void ClickRotate()
    {
        bool wasActive = rotator.uiButtonActive;
        DeactivateAll();
        if (!wasActive)
        {
            rotator.uiButtonActive = true;
            rotator.UpdateButtonColor();
        }
    }

    public void ClickChop()
    {
        bool wasActive = chopper.chopActive;
        DeactivateAll();
        if (!wasActive)
        {
            chopper.chopActive = true;
            chopper.UpdateButtonColor();
            hakPopout.SetActive(true);
        }
    }

    public void ClickXray()
    {
        bool wasActive = xray.xrayOn;
        DeactivateAll();
        if (!wasActive)
        {
            xray.ToggleXray();
            xrayPopout.SetActive(true);
        }
    }

    public void ClickClean()
    {
        bool wasActive = cleaner.cleaningActive;
        DeactivateAll();
        if (!wasActive)
        {
            cleaner.ActivateCleaning();
            brushPopout.SetActive(true);
        }
    }

    public void ClickSpray()
    {
        bool wasActive = wetPainter.sprayActive;
        DeactivateAll();
        if (!wasActive)
        {
            wetPainter.Activate();
            if (sprayPopout != null) sprayPopout.SetActive(true);
        }
    }

    public void ClickWater()
    {
        if (waterTank.isComplete)
        {
            if (waterPopout != null)
                waterPopout.SetActive(false);
            waterTank.OnActivateButtonPressed();
            return;
        }

        bool wasActive = waterTank.waterActive;
        DeactivateAll();
        if (!wasActive)
        {
            waterTank.OnActivateButtonPressed();
            if (waterPopout != null)
                waterPopout.SetActive(true);
        }
    }

    void DeactivateAll()
    {
        rotator.uiButtonActive = false;
        rotator.UpdateButtonColor();

        chopper.chopActive = false;
        chopper.UpdateButtonColor();

        if (xray.xrayOn)
            xray.ToggleXray();

        cleaner.DeactivateCleaning();

        waterTank.Deactivate();

        xrayPopout.SetActive(false);
        hakPopout.SetActive(false);
        brushPopout.SetActive(false);
        wetPainter.Deactivate();

        if (sprayPopout != null) sprayPopout.SetActive(false);
        if (waterPopout != null) waterPopout.SetActive(false);
    }
}