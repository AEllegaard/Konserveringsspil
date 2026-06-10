using UnityEngine;

public class FloatingImage : MonoBehaviour
{
    public RectTransform area;
    public float speed = 20f;
    public float noiseScale = 0.5f;
    public float smoothness = 5f; // hvor smooth retningen er

    RectTransform rt;
    float noiseOffsetX;
    float noiseOffsetY;
    Vector2 smoothedDirection;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        noiseOffsetX = Random.value * 100f;
        noiseOffsetY = Random.value * 100f;

        smoothedDirection = Vector2.zero;
    }

    void Update()
    {
        float t = Time.time * noiseScale;

        // rå Perlin-retning
        Vector2 rawDir = new Vector2(
            Mathf.PerlinNoise(noiseOffsetX, t) - 0.5f,
            Mathf.PerlinNoise(noiseOffsetY, t) - 0.5f
        ).normalized;

        // smooth retning → fjerner jitter
        smoothedDirection = Vector2.Lerp(
            smoothedDirection,
            rawDir,
            Time.deltaTime * smoothness
        );

        rt.anchoredPosition += smoothedDirection * speed * Time.deltaTime;

        KeepInsideArea();
    }

    void KeepInsideArea()
    {
        Vector2 pos = rt.anchoredPosition;
        Vector2 halfSize = area.rect.size / 2f;
        Vector2 myHalf = rt.rect.size / 2f;

        pos.x = Mathf.Clamp(pos.x, -halfSize.x + myHalf.x, halfSize.x - myHalf.x);
        pos.y = Mathf.Clamp(pos.y, -halfSize.y + myHalf.y, halfSize.y - myHalf.y);

        rt.anchoredPosition = pos;
    }
}
