using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionReadout : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    [SerializeField]
    private InteractableState state;

    [SerializeField]
    private string units;

    private void Awake()
    {
        state.WhileSelecting += SetText;
    }

    private void OnDestroy()
    {
        state.WhileSelecting -= SetText;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    private void SetText(float val)
    {
        text.text = $"{val.ToString("F0")}{units}";
    }
}
