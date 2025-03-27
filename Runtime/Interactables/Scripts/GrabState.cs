using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class GrabState : InteractableState
{
    [SerializeField]
    private bool useGravity = true;

    [SerializeField]
    private bool useLocal = false;

    [SerializeField]
    private XRGeneralGrabTransformer.ManipulationAxes axes = XRGeneralGrabTransformer.ManipulationAxes.All;

    [SerializeField]
    private bool trackRotation = false;

    [SerializeField]
    private Transform attachTransform = null;

    private InteractableStateChangerSocket currentSocket;

    public InteractableStateChangerSocket CurrentSocket { get => currentSocket; set => currentSocket = value; }

    private Rigidbody rb;
    private XRGeneralGrabTransformer grabTransformer;

    protected override void AddComponents()
    {
        base.AddComponents();

        // Rigidbody
        rb = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.isKinematic = !useGravity;

        // Grab Interactable
        XRGrabInteractable interactable = GetComponent<XRGrabInteractable>()
            ? GetComponent<XRGrabInteractable>()
            : gameObject.AddComponent<XRGrabInteractable>();

        SetInteractable(interactable);

        interactable.trackRotation = trackRotation;

        attachTransform = attachTransform ? attachTransform : transform;
        interactable.attachTransform = attachTransform;
        interactable.secondaryAttachTransform = attachTransform;

        // Grab Transformer
        grabTransformer = GetComponent<XRGeneralGrabTransformer>()
            ? GetComponent<XRGeneralGrabTransformer>()
            : gameObject.AddComponent<XRGeneralGrabTransformer>();

        grabTransformer.permittedDisplacementAxes = axes;
        grabTransformer.constrainedAxisDisplacementMode = useLocal
            ? XRGeneralGrabTransformer.ConstrainedAxisDisplacementMode.ObjectRelative
            : XRGeneralGrabTransformer.ConstrainedAxisDisplacementMode.WorldAxisRelative;

        // gameObject.AddComponent<PC_GrabState>();
    }

    protected override void Initialize()
    {
        Interactable.selectEntered.AddListener(Grab);                                                        
    }

    private void OnDestroy()
    {
        Interactable.selectEntered.RemoveListener(Grab);
    }

    private void Grab(SelectEnterEventArgs args)
    {
        ActivateSocket();
    }

    private void ActivateSocket()
    {
        if (!CurrentSocket)
            ProgressTracker.SetState(Progress.Start);

        if(CurrentSocket && !CurrentSocket.socketActive)
            CurrentSocket.socketActive = true;
    }
}
