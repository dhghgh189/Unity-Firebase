using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerClickHandler
{
    public UnityAction<PointerEventData> OnPointerClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClicked?.Invoke(eventData);
    }
}
