using UnityEngine;
using UnityEngine.EventSystems;

public class KnapTest : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("KLIK REGISTRERET PÅ KNAP!");
    }
}