using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableStateChangerSocket : XRSocketInteractor
{
    private GrabState currentSocketedItem;

    private string socketLocationType;

    public string SocketLocationType { get => socketLocationType; }

    private bool available = true;

    protected override void Awake()
    {
        base.Awake();

        available = true;
        // gameObject.layer = LayerMask.NameToLayer("Interactable");

        // interactionLayers = InteractionLayerMask.GetMask("Interactable");

        // gameObject.AddComponent<PC_InteractableStateChangerSocket>();
    }

    public GrabState SocketedItem
    {
        get => currentSocketedItem;
        set => currentSocketedItem = value;
    }

    public bool IsAvailable
    {
        get => available;
        set => available = value;
    }

    // An item entered the socket
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        Enter();

        SetToEnd();
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        Remove();

        SetToStart();
    }

    private void Enter()
    {
        if (!available)
            return;

        var socketedItem = interactablesSelected[0].transform.gameObject;

        if (!socketedItem.GetComponent<GrabState>())
            return;

        currentSocketedItem = socketedItem.GetComponent<GrabState>();
        currentSocketedItem.ProgressTracker.SetState(Progress.End);
        currentSocketedItem.CurrentSocket = this;
        
        available = false;
    }

    public void Remove()
    {        
        available = true;
    }

    private void SetToEnd()
    {
        currentSocketedItem.ProgressTracker.SetState(Progress.End);
    }

    private void SetToStart()
    {
        currentSocketedItem.ProgressTracker.SetState(Progress.Start);
    }
}
