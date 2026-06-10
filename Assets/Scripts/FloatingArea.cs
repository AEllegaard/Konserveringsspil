using UnityEngine;

public class FloatingAvoider : MonoBehaviour
{
    public float repelDistance = 80f;   // hvor tæt de må komme
    public float repelForce = 200f;     // hvor hårdt de skubber

    void Update()
    {
        var children = GetComponentsInChildren<RectTransform>();

        for (int i = 0; i < children.Length; i++)
        {
            for (int j = i + 1; j < children.Length; j++)
            {
                var a = children[i];
                var b = children[j];

                // spring containeren selv over
                if (a == transform || b == transform) 
                    continue;

                Vector2 delta = a.anchoredPosition - b.anchoredPosition;
                float dist = delta.magnitude;

                // hvis de er tættere end repelDistance → skub dem væk
                if (dist < repelDistance && dist > 0.01f)
                {
                    Vector2 push = delta.normalized * (repelForce * Time.deltaTime);

                    a.anchoredPosition += push;
                    b.anchoredPosition -= push;
                }
            }
        }
    }
}
