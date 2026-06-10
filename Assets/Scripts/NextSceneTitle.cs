using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using ZXing;

public class BarcodeScanner : MonoBehaviour
{
    [Header("Scene")]
    public string nextSceneName = "InfoScene";

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    [Header("UI Feedback")]
    public Text statusText;

    [Header("Webcam Preview")]
    public RawImage webcamDisplay;

    [Header("Debug (test uden webcam)")]
    public KeyCode debugScanKey = KeyCode.Space;

    private WebCamTexture webcamTexture;
    private IBarcodeReader reader;
    private bool hasScanned = false;

    IEnumerator Start()
    {
        reader = new BarcodeReader();

((BarcodeReader)reader).Options = new ZXing.Common.DecodingOptions
{
    PossibleFormats = new System.Collections.Generic.List<BarcodeFormat>
    {
        BarcodeFormat.CODE_128,
        BarcodeFormat.EAN_13,
        BarcodeFormat.EAN_8
    },
    TryHarder = true
};

WebCamDevice[] devices = WebCamTexture.devices;

        for (int i = 0; i < devices.Length; i++)
    Debug.Log("Device " + i + ": " + devices[i].name);

        if (devices.Length > 0)
        {
            string selectedDevice = devices[0].name;
foreach (var d in devices)
{
    if (d.name != "DroidCam Video") // skip DroidCam
    {
        selectedDevice = d.name;
        break;
    }
}
webcamTexture = new WebCamTexture(selectedDevice, 640, 480, 30);
            webcamTexture.Play();
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.LogWarning("Intet webcam fundet. Brug Space til at simulere scanning.");
        }

        if (webcamDisplay != null)
            webcamDisplay.texture = webcamTexture;

        if (statusText != null)
            statusText.text = "Hold kortet op foran kameraet...";

        if (fadeGroup != null)
            fadeGroup.alpha = 0f;
    }

    void Update()
    {
        if (hasScanned) return;

        if (Input.GetKeyDown(debugScanKey))
        {
            OnCardScanned("DEBUG_CARD");
            return;
        }

        if (webcamTexture != null && webcamTexture.isPlaying)
            ScanFrame();
    }

    void ScanFrame()
{
    var pixels = webcamTexture.GetPixels32();
    var result = ((BarcodeReader)reader).Decode(pixels, webcamTexture.width, webcamTexture.height);

    if (result != null)
        OnCardScanned(result.Text);
    
       
}

    void OnCardScanned(string cardId)
    {
        if (hasScanned) return;
        hasScanned = true;

        Debug.Log("Kort scannet: " + cardId);

        if (webcamTexture != null)
            webcamTexture.Stop();

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