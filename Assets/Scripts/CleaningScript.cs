using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectCleaner : MonoBehaviour
{
    [Header("Setup")]
    public Material cleanMat;
    public Material dirtyMat;
    public Material brushMat;
    public float brushSize = 0.2f;

    [Header("Brush Speed")]
    public float maxBrushSpeed = 500f;
    public float breakCooldownTime = 1f;
    public Transform vildsvinCeller;

    [Header("Button")]
    public Image button;
    public Color activeColor;
    public Color inactiveColor;

    [Header("Brush Particles")]
    public ParticleSystem brushParticles;

    [Header("Completion")]
    public float cleanTimeRequired = 10f;
    public bool cleaningComplete = false;
    public GameObject completionText;
    public float completionTextDuration = 3f;
    public GameObject fastBrushText;
    public float fastBrushTextDuration = 2f;

    [Header("Wet System")]
    public WetPainter wetPainter;

    private RenderTexture maskRT;
    public bool cleaningActive = false;
    private Vector3 lastMousePos;
    private float breakCooldown = 0f;
    private float brushTime = 0f;

public float ConservationProgress => cleaningComplete 
    ? 1f 
    : Mathf.Clamp01(brushTime / cleanTimeRequired);
    
    private int totalVildsvinCells = 0;

public float IntactProgress => vildsvinCeller != null && totalVildsvinCells > 0
    ? (float)vildsvinCeller.childCount / totalVildsvinCells
    : 1f;

    void Start()
    {
        button.color = inactiveColor;

        maskRT = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        maskRT.Create();

        RenderTexture.active = maskRT;
        GL.Clear(true, true, Color.white);
        RenderTexture.active = null;

        Transform wholeMesh = transform.Find("VildsvinWholeMesh");
        if (wholeMesh != null)
        {
            Renderer r = wholeMesh.GetComponent<Renderer>();
            r.material = dirtyMat;
            dirtyMat.SetTexture("_MaskTex", maskRT);
        }

        if (completionText != null)
            completionText.SetActive(false);

        if (fastBrushText != null)
            fastBrushText.SetActive(false);

        if (brushParticles != null)
            brushParticles.Stop();

            if (vildsvinCeller != null)
    totalVildsvinCells = vildsvinCeller.childCount;
    }

    void Update()
    {
        breakCooldown -= Time.deltaTime;

        if (cleaningActive && Input.GetMouseButton(0))
        {
            float speed = (Input.mousePosition - lastMousePos).magnitude / Time.deltaTime;

            if (speed > maxBrushSpeed && breakCooldown <= 0f)
            {
                BreakOffCell();
                breakCooldown = breakCooldownTime;
            }

            TryClean();
            UpdateBrushParticles();

            if (!cleaningComplete)
            {
                brushTime += Time.deltaTime;
                if (brushTime >= cleanTimeRequired)
                    TriggerComplete();
            }
        }

        if (brushParticles != null && !Input.GetMouseButton(0))
        {
            if (brushParticles.isPlaying)
                brushParticles.Stop();
        }

        lastMousePos = Input.mousePosition;
    }

    void BreakOffCell()
{
    Debug.Log($"BreakOffCell kaldt | vildsvinCeller childCount: {vildsvinCeller.childCount}");
    if (vildsvinCeller == null) return;

    Transform[] celler = vildsvinCeller.GetComponentsInChildren<Transform>();

    foreach (Transform celle in celler)
    {
        if (celle == vildsvinCeller) continue;
        if (celle.GetComponent<Rigidbody>() != null) continue;

        Rigidbody rb = celle.gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.AddForce(Vector3.down * 200f + Random.insideUnitSphere * 100f, ForceMode.Impulse);

        celle.SetParent(null); // DENNE LINJE MANGLER

        StartCoroutine(ShowFastBrushText());
        break;
    }
}
    void UpdateBrushParticles()
    {
        if (brushParticles == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layer = LayerMask.GetMask("Bronze");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            brushParticles.transform.position = hit.point;
            brushParticles.transform.rotation = Quaternion.LookRotation(hit.normal);

            if (!brushParticles.isPlaying)
                brushParticles.Play();
        }
        else
        {
            if (brushParticles.isPlaying)
                brushParticles.Stop();
        }
    }

    void TryClean()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layer = LayerMask.GetMask("Bronze");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            if (wetPainter != null && wetPainter.wetMask != null)
                brushMat.SetTexture("_WetMask", wetPainter.wetMask);

            Vector2 uv = hit.textureCoord;
            brushMat.SetVector("_UV", new Vector4(uv.x, uv.y, brushSize, 0));

            RenderTexture temp = RenderTexture.GetTemporary(maskRT.width, maskRT.height);
            Graphics.Blit(maskRT, temp);
            Graphics.Blit(temp, maskRT, brushMat);
            RenderTexture.ReleaseTemporary(temp);
        }
    }

    void TriggerComplete()
    {
        cleaningComplete = true;

        Transform wholeMesh = transform.Find("VildsvinWholeMesh");
        if (wholeMesh != null)
            StartCoroutine(FadeOutMesh(wholeMesh.gameObject));

        StartCoroutine(ShowCompletionText());
    }

IEnumerator FadeOutMesh(GameObject meshObj)
{
    float duration = 3f;
    float t = 0f;

    Renderer r = meshObj.GetComponent<Renderer>();
    Material mat = r.material;

    while (t < duration)
    {
        t += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, t / duration);
        mat.SetFloat("_Fade", alpha);
        yield return null;
    }

    mat.SetFloat("_Fade", 0f);
}

    IEnumerator ShowFastBrushText()
    {
        if (fastBrushText == null) yield break;
        fastBrushText.SetActive(true);
        yield return new WaitForSeconds(fastBrushTextDuration);
        fastBrushText.SetActive(false);
    }

    IEnumerator ShowCompletionText()
    {
        if (completionText == null) yield break;
        completionText.SetActive(true);
        yield return new WaitForSeconds(completionTextDuration);
        completionText.SetActive(false);
    }

    public void ActivateCleaning()
    {
        cleaningActive = true;
        UpdateButtonColor();
    }

    public void DeactivateCleaning()
    {
        cleaningActive = false;
        UpdateButtonColor();
    }

    public void UpdateButtonColor()
    {
        button.color = cleaningActive ? activeColor : inactiveColor;
    }
}