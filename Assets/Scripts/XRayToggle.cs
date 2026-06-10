using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class XrayToggle : MonoBehaviour
{
    public Transform soilParent;
    public Transform bronzeParent;

    public ObjectCleaner cleaner; 

    public Color soilXrayColor = new Color(0.3f, 1f, 0.3f, 0.6f);
    public Color bronzeXrayColor = new Color(1f, 0.6f, 1f, 1f);

    public Renderer backgroundPlane;
    public Color backgroundXrayColor = Color.black;
    private Color backgroundOriginalColor;

    public Light sceneLight;
    public float xrayLightIntensity = 0.0f;

    private float originalLightIntensity;
    private Color originalAmbientLight;
    private float originalReflectionIntensity;
    private Material originalSkybox;

    public Image xrayButton;
    public Color xrayActiveColor = Color.white;
    public Color xrayInactiveColor = Color.gray;

   public bool xrayOn = false;

    private List<Renderer> soilFragments = new List<Renderer>();
    private List<Renderer> bronzeFragments = new List<Renderer>();

    private List<Color> soilOriginalColors = new List<Color>();
    private List<Color> bronzeOriginalColors = new List<Color>();
    private List<Texture> bronzeOriginalTextures = new List<Texture>();

    void Start()
    {
        if (soilParent != null)
            soilFragments.AddRange(soilParent.GetComponentsInChildren<Renderer>(true));

        if (bronzeParent != null)
            bronzeFragments.AddRange(bronzeParent.GetComponentsInChildren<Renderer>(true));

       
// Til:
foreach (var r in soilFragments)
    soilOriginalColors.Add(r.material.HasProperty("_Color") ? r.material.color : Color.white);

foreach (var r in bronzeFragments)
{
    bronzeOriginalColors.Add(r.material.HasProperty("_Color") ? r.material.color : Color.white);
    bronzeOriginalTextures.Add(null);
}

        if (backgroundPlane != null)
            backgroundOriginalColor = backgroundPlane.material.color;

        if (sceneLight != null)
            originalLightIntensity = sceneLight.intensity;

        originalAmbientLight = RenderSettings.ambientLight;
        originalReflectionIntensity = RenderSettings.reflectionIntensity;
        originalSkybox = RenderSettings.skybox;

        if (xrayButton != null)
            xrayButton.color = xrayInactiveColor;
    }

    public void ToggleXray()
    {
        xrayOn = !xrayOn;

        if (xrayButton != null)
            xrayButton.color = xrayOn ? xrayActiveColor : xrayInactiveColor;

        if (xrayOn)
            ActivateXray();
        else
            DeactivateXray();
    }

    void ActivateXray()
    {
        if (cleaner != null)
    {
        Transform wholeMesh = cleaner.transform.Find("VildsvinWholeMesh");
        if (wholeMesh != null)
            wholeMesh.gameObject.SetActive(false);
    }
        if (sceneLight != null)
            sceneLight.intensity = xrayLightIntensity;

        RenderSettings.ambientLight = Color.black;
        RenderSettings.reflectionIntensity = 0f;
        RenderSettings.skybox = null;

        foreach (var r in soilFragments)
        {
            if (r == null) continue;
            SetTransparent(r.material);
            DisableEmission(r.material);
            r.material.color = soilXrayColor;
        }

        foreach (var r in bronzeFragments)
        {
            if (r == null) continue;
            SetOpaque(r.material);
            r.material.SetTexture("_BaseMap", null);
            r.material.SetTexture("_MainTex", null);
            r.material.color = bronzeXrayColor;
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", Color.white * 1.5f);
        }

        if (backgroundPlane != null)
            backgroundPlane.material.color = backgroundXrayColor;
    }

    void DeactivateXray()
    {
          if (cleaner != null)
    {
        Transform wholeMesh = cleaner.transform.Find("VildsvinWholeMesh");
        if (wholeMesh != null)
            wholeMesh.gameObject.SetActive(true);
    }

        if (sceneLight != null)
            sceneLight.intensity = originalLightIntensity;

        RenderSettings.ambientLight = originalAmbientLight;
        RenderSettings.reflectionIntensity = originalReflectionIntensity;
        RenderSettings.skybox = originalSkybox;

        for (int i = 0; i < soilFragments.Count; i++)
        {
            if (soilFragments[i] == null) continue;
            SetOpaque(soilFragments[i].material);
            soilFragments[i].material.color = soilOriginalColors[i];
        }

        for (int i = 0; i < bronzeFragments.Count; i++)
{
    if (bronzeFragments[i] == null) continue;
    SetOpaque(bronzeFragments[i].material);
    bronzeFragments[i].material.color = bronzeOriginalColors[i];
    // ← fjern de to SetTexture linjer helt
    bronzeFragments[i].material.DisableKeyword("_EMISSION");
    bronzeFragments[i].material.SetColor("_EmissionColor", Color.black);
}

        if (backgroundPlane != null)
            backgroundPlane.material.color = backgroundOriginalColor;
    }

    void DisableEmission(Material mat)
    {
        if (mat == null) return;

        if (mat.IsKeywordEnabled("_EMISSION"))
        {
            mat.DisableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", Color.black);
        }
    }

    void SetTransparent(Material mat)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetFloat("_ZWrite", 0);
            mat.renderQueue = 3000;
        }
        else
        {
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    void SetOpaque(Material mat)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 0);
            mat.SetFloat("_ZWrite", 1);
            mat.renderQueue = -1;
        }
        else
        {
            mat.SetFloat("_Mode", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }
    }
}