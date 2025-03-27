using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectState : InteractableState 
{
    private bool selected = false;

    [SerializeField]
    private bool allowGazeSelect = false;

    protected override void AddComponents()
    {
        base.AddComponents();

        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>()
            ? GetComponent<XRSimpleInteractable>()
            : gameObject.AddComponent<XRSimpleInteractable>();

        SetInteractable(interactable);

        // Gaze
        interactable.allowGazeInteraction = allowGazeSelect;
        interactable.allowGazeSelect = allowGazeSelect;
        interactable.allowGazeAssistance = allowGazeSelect;
    }

    protected override void Initialize()
    {
        if (!allowGazeSelect)
        {
            Interactable.selectExited.AddListener(ToggleSelected);
        }
        else
        {
            Interactable.selectEntered.AddListener(Select);
            Interactable.hoverExited.AddListener(Deselect);
        }
        

        // gameObject.AddComponent<PC_SelectState>();
    }

    private void OnDestroy()
    {
        if (!Initialized) return;

        if (!allowGazeSelect)
        {
            Interactable.selectExited.RemoveListener(ToggleSelected);
        }
        else
        {
            Interactable.selectEntered.RemoveListener(Select);
            Interactable.hoverExited.RemoveListener(Deselect);
        }
    }

    public void ToggleSelected(SelectExitEventArgs args)
    {
        selected = !selected;

        if (ProgressTracker)
            ProgressTracker.SetState(selected ? Progress.End : Progress.Start);
    }

    private void Select(SelectEnterEventArgs args)
    {
        selected = true;
        ProgressTracker.SetState(Progress.End);
    }

    private void Deselect(HoverExitEventArgs args)
    {
        if (selected)
        {
            selected = false;
            ProgressTracker.SetState(Progress.Start);
        }
    }

    public override void EndInteraction()
    {
        base.EndInteraction();

        selected = false;
    }
}
