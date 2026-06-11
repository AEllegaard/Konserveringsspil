using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndSceneUI : MonoBehaviour
{
    public Image intactBar;
    public Image conservationBar;
    public TextMeshProUGUI intactText;
    public TextMeshProUGUI conservationText;

    [Header("Navigation")]
    public Button restartButton;
    public string startSceneName = "StartScene";

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    void Start()
    {
        float intact = PlayerPrefs.GetFloat("IntactProgress", 1f);
        float conservation = PlayerPrefs.GetFloat("ConservationProgress", 0f);

        intactBar.fillAmount = intact;
        conservationBar.fillAmount = conservation;
        intactText.text = Mathf.RoundToInt(intact * 100f) + "%";
        conservationText.text = Mathf.RoundToInt(conservation * 100f) + "%";

        if (restartButton != null)
            restartButton.onClick.AddListener(() => StartCoroutine(FadeAndLoad()));

        if (fadeGroup != null)
            fadeGroup.alpha = 0f;
    }

    IEnumerator FadeAndLoad()
    {
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

        PlayerPrefs.DeleteKey("IntactProgress");
        PlayerPrefs.DeleteKey("ConservationProgress");

        SceneManager.LoadScene(startSceneName);
    }
}