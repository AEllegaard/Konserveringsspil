using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WetPainter : MonoBehaviour
{
    [Header("UI")]
    public Image sprayButton;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Setup")]
    public Camera cam;
    public LayerMask paintLayer;
    public Material brushMat;
    public float brushSize = 0.15f;
    public float dryDelay = 0.5f;
    public float dryDuration = 2f;
    public Renderer wholeMeshRenderer;

    [Header("Coverage Completion")]
    public float coverageThreshold = 0.4f;       // 80% af UV skal dækkes
    public float coverageCheckInterval = 0.5f;   // check hvert halve sekund
    public GameObject completionText;
    public float completionTextDuration = 3f;
    public float SprayProgress { get; private set; }
    public bool sprayActive = false;
    public bool sprayComplete = false;

    private float wetStrength = 0f;
    private Coroutine dryCoroutine;
    private MaterialPropertyBlock propBlock;
    public RenderTexture wetMask;

    // Permanent akkumuleringsmaske - tørrer aldrig ud
    private RenderTexture coverageMask;
    private Texture2D readbackTex;
    private bool completionTriggered = false;

    void Awake()
    {
        // Wet-maske (visuel, tørrer ud)
        wetMask = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        wetMask.Create();
        RenderTexture.active = wetMask;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        // Coverage-maske (permanent, bruges kun til måling)
        // Starter sort - WetBrush akkumulerer hvide pixels oppå
        coverageMask = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        coverageMask.Create();
        RenderTexture.active = coverageMask;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        // Lille texture til CPU-readback (64x64 er nok til 80%-estimat)
        readbackTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
    }

    void Start()
    {
        Debug.Log("WetPainter Start kørt");

        if (wholeMeshRenderer == null)
        {
            Debug.LogError("WholeMeshRenderer er null!");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        wholeMeshRenderer.sharedMaterial.SetTexture("_WetMask", wetMask);
        wholeMeshRenderer.sharedMaterial.SetFloat("_WetStrength", 0f);

        if (completionText != null)
            completionText.SetActive(false);

        StartCoroutine(CoverageCheckLoop());

        Debug.Log("WetMask sat via sharedMaterial");
    }

    void Update()
    {
        if (!sprayActive) return;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, paintLayer))
            {
                Debug.Log("Ramte: " + hit.collider.gameObject.name);
                PaintWet(hit.textureCoord);

                if (dryCoroutine != null)
                    StopCoroutine(dryCoroutine);

                wetStrength = Mathf.Min(1f, wetStrength + Time.deltaTime * 3f);

                wholeMeshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_WetStrength", wetStrength);
                wholeMeshRenderer.SetPropertyBlock(propBlock);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (dryCoroutine != null)
                StopCoroutine(dryCoroutine);
            dryCoroutine = StartCoroutine(DryOut());
        }
    }

    void PaintWet(Vector2 uv)
    {
       
        brushMat.SetVector("_UV", new Vector4(uv.x, uv.y, brushSize, 0));

        // Mal på den visuelle wet-maske
        RenderTexture temp = RenderTexture.GetTemporary(wetMask.width, wetMask.height);
        Graphics.Blit(wetMask, temp);
        Graphics.Blit(temp, wetMask, brushMat);
        RenderTexture.ReleaseTemporary(temp);

        // Mal også på den permanente coverage-maske (samme brush, akkumulerer)
        RenderTexture tempCov = RenderTexture.GetTemporary(coverageMask.width, coverageMask.height);
        Graphics.Blit(coverageMask, tempCov);
        Graphics.Blit(tempCov, coverageMask, brushMat);
        RenderTexture.ReleaseTemporary(tempCov);
    }

    // Måler coverage hvert coverageCheckInterval sekund
    IEnumerator CoverageCheckLoop()
    {
        while (!completionTriggered)
        {
            yield return new WaitForSeconds(coverageCheckInterval);

            if (completionTriggered) yield break;

            float coverage = MeasureCoverage();
            SprayProgress = coverage;
Debug.Log($"WetPainter coverage: {coverage:P0} | threshold: {coverageThreshold:P0} | triggered: {completionTriggered}");

            if (coverage >= coverageThreshold)
{
    Debug.Log("Threshold nået - sætter completionTriggered = true");
    completionTriggered = true;
    sprayComplete = true;
    Debug.Log("completionText er null: " + (completionText == null));
    StartCoroutine(ShowCompletionText());
}
        }
    }

    // Læser coverageMask ned til 64x64 og tæller hvide pixels
    float MeasureCoverage()
    {
        RenderTexture prev = RenderTexture.active;

        // Downsample til 64x64 for hurtig CPU-læsning
        RenderTexture small = RenderTexture.GetTemporary(64, 64, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(coverageMask, small);

        RenderTexture.active = small;
        readbackTex.ReadPixels(new Rect(0, 0, 64, 64), 0, 0, false);
        readbackTex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(small);

        Color32[] pixels = readbackTex.GetPixels32();
        int painted = 0;
        foreach (Color32 p in pixels)
        {
            // WetBrush skriver hvidt (høj R) på painted områder
            if (p.r > 128) painted++;
        }

        return (float)painted / pixels.Length;
    }

    IEnumerator ShowCompletionText()
    {
        if (completionText == null) yield break;
        completionText.SetActive(true);
        yield return new WaitForSeconds(completionTextDuration);
        completionText.SetActive(false);
    }

    IEnumerator DryOut()
    {
        yield return new WaitForSeconds(dryDelay);

        float elapsed = 0f;
        float startStrength = wetStrength;

        while (elapsed < dryDuration)
        {
            elapsed += Time.deltaTime;
            wetStrength = Mathf.Lerp(startStrength, 0f, elapsed / dryDuration);

            wholeMeshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_WetStrength", wetStrength);
            wholeMeshRenderer.SetPropertyBlock(propBlock);

            yield return null;
        }

        wetStrength = 0f;
        wholeMeshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_WetStrength", 0f);
        wholeMeshRenderer.SetPropertyBlock(propBlock);

        // Kun wetMask ryddes - coverageMask bevares
        RenderTexture.active = wetMask;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }

    public void Activate()
    {
        sprayActive = true;
        if (sprayButton != null) sprayButton.color = activeColor;
    }

    public void Deactivate()
    {
        sprayActive = false;
        if (sprayButton != null) sprayButton.color = inactiveColor;
    }
}