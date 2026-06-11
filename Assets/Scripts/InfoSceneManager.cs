using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class InfoSceneManager : MonoBehaviour
{
    [Header("Kort")]
    public RectTransform kortRoot;       // DKkort RectTransform
    public RectTransform svinPin;        // SvinPin RectTransform

    [Header("Zoom")]
    public float zoomDelay = 2f;         // vent før zoom starter
    public float zoomDuration = 1.5f;   // hvor lang tid zoom tager
    public float zoomScale = 3f;         // hvor meget der zoomes ind

    [Header("Infocard")]
    public GameObject infoCard;          // infocard panel
    public CanvasGroup infoCardGroup;    // CanvasGroup på infocard
    public float fadeDuration = 0.5f;

    [Header("Fade til næste scene")]
    public CanvasGroup fadeGroup;        // sort panel over alt
    public string nextSceneName = "SampleScene";

    void Start()
{
    if (Input.GetMouseButtonDown(0))
    Debug.Log("KLIK REGISTRERET");
    
    if (infoCardGroup != null)
    {
        infoCardGroup.alpha = 0f;
        infoCardGroup.blocksRaycasts = false;
        infoCardGroup.interactable = false;
    }

    if (fadeGroup != null) {
    fadeGroup.alpha = 0f;
    fadeGroup.blocksRaycasts = false; // tilføj denne
    fadeGroup.interactable = false;   // og denne
}

    StartCoroutine(RunSequence());
}
void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        if (EventSystem.current == null) 
        {
            Debug.Log("EventSystem er null!");
            return;
        }
        
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);
        foreach (var r in results)
            Debug.Log("Ramt: " + r.gameObject.name);
    }

    if (Input.GetKeyDown(KeyCode.Space))
{
    GoToNextScene();
}
}
    IEnumerator RunSequence()
    {
        // 1. Vent mens hele kortet vises
        yield return new WaitForSeconds(zoomDelay);

        // 2. Zoom ind på SvinPin
        yield return StartCoroutine(ZoomToPin());

        // 3. Vis infocard
        if (infoCard != null)
            infoCard.SetActive(true);

        yield return StartCoroutine(FadeIn(infoCardGroup));
    }

    IEnumerator ZoomToPin()
{
    Vector2 startPos = kortRoot.anchoredPosition;
    Vector3 startScale = kortRoot.localScale;
    Vector3 targetScale = startScale * zoomScale;

    // Centrer svinPin midt på skærmen efter zoom
    Vector2 pinAnchoredPos = svinPin.anchoredPosition;
    Vector2 targetPos = -pinAnchoredPos * (startScale.x * zoomScale);

    float t = 0f;
    while (t < zoomDuration)
    {
        t += Time.deltaTime;
        // EaseInOutQuart - mere dramatisk end SmoothStep
        float ease = t / zoomDuration;
        ease = ease < 0.5f
            ? 8f * ease * ease * ease * ease
            : 1f - Mathf.Pow(-2f * ease + 2f, 4f) / 2f;

        kortRoot.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
        kortRoot.localScale = Vector3.Lerp(startScale, targetScale, ease);

        yield return null;
    }

    kortRoot.anchoredPosition = targetPos;
    kortRoot.localScale = targetScale;
}

    IEnumerator FadeIn(CanvasGroup cg)
{
    if (cg == null) yield break;
    cg.blocksRaycasts = true;
    cg.interactable = true;
    float t = 0f;
    while (t < fadeDuration)
    {
        t += Time.deltaTime;
        cg.alpha = Mathf.Clamp01(t / fadeDuration);
        yield return null;
    }
    cg.alpha = 1f;
}

    public void Klik()
    {
        Debug.Log("KNAP VIRKER");
    }

    public void GoToNextScene()
{
    Debug.Log("GoToNextScene kaldt!");
    StartCoroutine(FadeAndLoad());
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

        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(nextSceneName);
    }
}