using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomEventTrigger : MonoBehaviour
{
    private InteractableVisual interactableVisual;

    private void Awake()
    {
        if (GetComponent<InteractableVisual>())
        {
            interactableVisual = GetComponent<InteractableVisual>();
            interactableVisual.OnStateNameChanged += TriggerEventAfterStateChange;
        }            
    }

    private void OnDestroy()
    {
        if (interactableVisual)
        {
            interactableVisual.OnStateNameChanged -= TriggerEventAfterStateChange;
        }
    }
    public void TriggerCustomEvent(string eventName)
    {
        CustomEvent.Trigger(gameObject, eventName);
    }

    private void TriggerEventAfterStateChange(string stateName)
    {
        TriggerCustomEvent(stateName);
    }
}
