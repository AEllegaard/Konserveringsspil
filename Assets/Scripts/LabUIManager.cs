using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LaboratorietUI : MonoBehaviour
{
    [System.Serializable]
    public class ToolEntry
    {
        public string toolName;
        [TextArea(3, 6)]
        public string description;
        public Sprite icon;
    }

    [Header("Data")]
    public List<ToolEntry> tools = new List<ToolEntry>();

    [Header("Tab buttons")]
    public List<Button> tabButtons = new List<Button>();

    [Header("Panel")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    [Header("Navigation")]
    public Button nextSceneButton;

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Colors")]
    public Color activeColor   = new Color(0.07f, 0.25f, 0.96f);
    public Color inactiveColor = new Color(0.39f, 0.51f, 0.98f);

    private int currentIndex = -1;

    void Start()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int idx = i;
            tabButtons[i].onClick.AddListener(() => SelectTool(idx));
        }

        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(() => StartCoroutine(FadeAndLoad("SampleScene")));

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        Initialize();
        SelectTool(0);
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    void Initialize()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            var cb = tabButtons[i].colors;
            cb.normalColor      = inactiveColor;
            cb.highlightedColor = inactiveColor;
            cb.selectedColor    = inactiveColor;
            tabButtons[i].colors = cb;
        }
    }

    public void SelectTool(int index)
    {
        if (index == currentIndex) return;
        currentIndex = index;

        for (int i = 0; i < tabButtons.Count; i++)
        {
            var cb = tabButtons[i].colors;
            Color c = (i == index) ? activeColor : inactiveColor;
            cb.normalColor      = c;
            cb.highlightedColor = c;
            cb.selectedColor    = c;
            tabButtons[i].colors = cb;
        }

        if (index < tools.Count)
        {
            nameText.text = tools[index].toolName;
            descText.text = tools[index].description;
        }
    }
}