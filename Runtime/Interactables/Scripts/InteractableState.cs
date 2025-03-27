using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class InteractableState : MonoBehaviour
{
    protected Transform visualHolder;

    private bool useVisualHolderOffset = false;
    private bool useVisualHolderRotation = false;

    private Collider interactCollider;

    [HideInInspector]
    public InteractableVisual visual;
   
    private Transform startParent;

    private Vector3 startPosition, startRotation;

    private XRBaseInteractable interactable;
    public XRBaseInteractable Interactable
    {
        get { return interactable; }
        private set { interactable = value; }
    }

    private ProgressStateTracker progressTracker;

    public ProgressStateTracker ProgressTracker
    {
        get { return progressTracker; }
        set { progressTracker = value; }
    }

    private bool initialized;

    public bool Initialized
    {
        get { return initialized; }
        private set { initialized = value; }
    }

    public Action<float> WhileSelecting;

    private void Awake()
    {
        visualHolder = transform.GetChild(0);
        visualHolder.GetChild(0).gameObject.SetActive(false);
        startParent = transform.parent;

        startPosition = transform.localPosition;
        startRotation = transform.localEulerAngles;       
    }

    private void OnDestroy()
    {
        if (interactable == null) return;

        interactable.hoverEntered.RemoveListener(EnterHover);
        interactable.hoverExited.RemoveListener(ExitHover);
        interactable.selectEntered.RemoveListener(EnterSelect);
        interactable.selectExited.RemoveListener(ExitSelect);
        interactable.onActivate.RemoveListener(Activated);
        interactable.onDeactivate.RemoveListener(Deactivated);
    }

    protected virtual void AddComponents()
    {
        progressTracker = GetComponent<ProgressStateTracker>()
            ? GetComponent<ProgressStateTracker>()
            : gameObject.AddComponent<ProgressStateTracker>();
    }

    protected void SetInteractable(XRBaseInteractable interactable)
    {
        this.interactable = interactable;

        // interactable.interactionLayers = InteractionLayerMask.GetMask("Interactable");
    }

    public void Initialize(InteractableVisual visual)
    {
        AddComponents();
        
        if (interactable == null) return;

        interactable.enabled = false;
        
        this.visual = visual;

        Initialize();
        ActivateInteractionAffordance();
        AddColliders();        
       
        Initialized = true;

        SetLayerInteractable(gameObject);
        SetLayerInteractable(visual.gameObject);

        interactable.hoverEntered.AddListener(EnterHover);
        interactable.hoverExited.AddListener(ExitHover);
        interactable.selectEntered.AddListener(EnterSelect);
        interactable.selectExited.AddListener(ExitSelect);
        interactable.onActivate.AddListener(Activated);
        interactable.onDeactivate.AddListener(Deactivated);
    }

    public virtual void Set()
    {
        DisableInteractable();
        if (visual == null) return;
        transform.SetParent(visual.Parent);

        var offset = visualHolder.localPosition;

        transform.position = visual.transform.position - offset;

        EnableInteractable();
        
        visual.transform.SetParent(visualHolder);

        if(useVisualHolderOffset)
            visual.transform.localPosition = Vector3.zero;

        if(useVisualHolderRotation)
            visual.transform.localEulerAngles = Vector3.zero;       

        StartInteraction();
    }

    public virtual void DisableInteractable()
    {
        if (interactable == null) return;
        interactable.enabled = false;
    }

    public virtual void EnableInteractable()
    {
        if (interactable == null) return;
        interactable.enabled = true;
    }

    private void AddColliders()
    {
        if (interactable == null || visual == null) return;
        if (!visual.GetComponent<Collider>()) 
            return;

        interactCollider = visual.GetComponent<Collider>();
        
        if(!interactable.colliders.Contains(interactCollider))
            interactable.colliders.Add(interactCollider);     
    }

    private void ActivateInteractionAffordance()
    {        
        if (visual.InteractionAffordanceRenderer)
        {           
            visualHolder = transform.GetChild(0);
            XRInteractableAffordanceStateProvider provider = visualHolder.GetChild(0).GetComponent<XRInteractableAffordanceStateProvider>();
            provider.interactableSource = interactable;           

            MaterialPropertyBlockHelper propertyBlock = provider.transform.GetChild(0).GetComponent<MaterialPropertyBlockHelper>();
            propertyBlock.rendererTarget = visual.InteractionAffordanceRenderer;

            visualHolder.GetChild(0).gameObject.SetActive(true);
            propertyBlock.enabled = true;            
        }
    }

    protected virtual void Initialize()
    {
        // Do any interaction-specific initialization here
    }

    protected virtual void StartInteraction()
    {

    }

    public virtual void EndInteraction()
    {
        DisableInteractable();

        transform.SetParent(startParent);
        // When select exits
    }

    public void SetToStartPosition()
    {
        transform.localPosition = startPosition;
    }

    public void SetToStartRotation()
    {
        transform.localEulerAngles = startRotation;
    }

    public void EnableColliders()
    {
        if (interactable == null) return;
        foreach (var collider in interactable.colliders)
        {
            collider.enabled = true;
        }
    }

    public void DisableColliders()
    {
        if (interactable == null) return;
        foreach (var collider in interactable.colliders)
        {
            collider.enabled = false;
        }
    }

    internal void SetLayerInteractable(GameObject go)
    {
        //go.layer = LayerMask.NameToLayer("Interactable");       
    }
    private void EnterHover(HoverEnterEventArgs args)
    {
        CustomEvent.Trigger(gameObject, "Hover Entered");
    }

    private void ExitHover(HoverExitEventArgs args)
    {
        CustomEvent.Trigger(gameObject, "Hover Exited");
    }

    public void EnterSelect(SelectEnterEventArgs args)
    {
        CustomEvent.Trigger(gameObject, "Select Entered");
    }

    private void ExitSelect(SelectExitEventArgs args)
    {
        CustomEvent.Trigger(gameObject, "Select Exited");
    }

    private void Activated(XRBaseInteractor args)
    {
        CustomEvent.Trigger(gameObject, "Activated");
    }

    private void Deactivated(XRBaseInteractor args)
    {
        CustomEvent.Trigger(gameObject, "Deactivated");
    }
}
