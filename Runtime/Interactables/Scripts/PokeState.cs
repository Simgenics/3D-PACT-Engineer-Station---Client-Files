using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PokeState : InteractableState
{
    [SerializeField]
    private enum Axis { X, Y, Z };

    private XRPokeFilter pokeFilter;
    private XRPokeFollowAffordance pokeFollowAffordance;

    [SerializeField]
    private Axis axis = Axis.Y;
    [SerializeField]
    private float pokeDepth = -0.02f;
    [SerializeField]
    private Transform surface;   
    
    private Vector3 initialPosition;
    private Vector3 tweenDepth;

    private float tweenDuration = 0.2f;

    private bool wasPressed, isPressed;
    private Tween currentTween;

    private Vector3 DepthVector()
        => new Vector3[] { new(pokeDepth, 0, 0), new(0, pokeDepth, 0), new(0, 0, pokeDepth) }[(int)axis];

    protected override void AddComponents()
    {
        base.AddComponents();

        // Interactable
        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>() 
            ? GetComponent<XRSimpleInteractable>()
            : gameObject.AddComponent<XRSimpleInteractable>();

        SetInteractable(interactable);

        // Poke Filter
        pokeFilter = GetComponent<XRPokeFilter>()
            ? GetComponent<XRPokeFilter>()
            : gameObject.AddComponent<XRPokeFilter>();

        pokeFilter.pokeInteractable = interactable;
        pokeFilter.pokeCollider = GetComponentInChildren<BoxCollider>();
        SetLayerInteractable(pokeFilter.pokeCollider.gameObject);

        pokeFilter.enabled = false;


        // Follow
        pokeFollowAffordance = GetComponent<XRPokeFollowAffordance>()
            ? GetComponent<XRPokeFollowAffordance>()
            : gameObject.AddComponent<XRPokeFollowAffordance>();

        pokeFollowAffordance.pokeFollowTransform = visualHolder;

        pokeFilter.enabled = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        int relativeAxis = RelativeAxis(LocalDirection(visual.transform));
        pokeFilter.pokeConfiguration.Value.pokeDirection = axis switch
        {
            Axis.X => pokeDepth > 0 ? PokeAxis.X : PokeAxis.NegativeX,
            Axis.Y => pokeDepth > 0 ? PokeAxis.Y : PokeAxis.NegativeY,
            _ => pokeDepth > 0 ? PokeAxis.Z : PokeAxis.NegativeZ,
        };

        Interactable.selectEntered.AddListener(Poke);
        Interactable.selectExited.AddListener(Release);
    }

    public override void Set()
    {
        DisableInteractable();
        if (visual == null) return;

        transform.position = surface.position + DepthVector();       

        if (surface)
        {
            visualHolder.position = surface.position;
        }

        transform.SetParent(visual.Parent);

        EnableInteractable();

        visual.transform.SetParent(visualHolder);

        StartInteraction();

        pokeFollowAffordance.initialPosition = visualHolder.localPosition;
        pokeFollowAffordance.clampToMaxDistance = true;

        initialPosition = transform.localPosition;
        tweenDepth = initialPosition + DepthVector();

        pokeFilter.pokeCollider.transform.rotation = visual.transform.rotation;
        pokeFilter.pokeCollider.transform.localScale = visual.transform.localScale;

        // Collider
        BoxCollider boxPokeCollider = pokeFilter.pokeCollider as BoxCollider;
        BoxCollider visualCollider = visual.GetComponent<BoxCollider>();        

        /// Position
        pokeFilter.pokeCollider.transform.position = surface.position;
        
        /// Size
        boxPokeCollider.size = visualCollider.size;

        /// Centre
        Vector3 visualLocal = LocalDirection(visual.transform);
        int relativeAxis = RelativeAxis(visualLocal);

        Vector3 surfaceRelative = visual.transform.InverseTransformPoint(surface.position);
        Vector3 centreVector = Vector3.zero;
        float sign = surfaceRelative[relativeAxis] > 0 ? -1 : 1;
        centreVector[relativeAxis] = (boxPokeCollider.size[relativeAxis] / 2) * sign;
        boxPokeCollider.center = centreVector;               

        // gameObject.AddComponent<PC_PokeState>();
    }

    private void OnDestroy()
    {
        Interactable.selectEntered.RemoveListener(Poke);
        Interactable.selectExited.RemoveListener(Release);
    }

    private void ToggleState()
    {
        wasPressed = !wasPressed;

        if (ProgressTracker)
            ProgressTracker.SetState(wasPressed ? Progress.End : Progress.Start);
    }

    // Tween button poke if selecting and not actually poking
    private void Poke(SelectEnterEventArgs args)
    {
        float? pokeStrength = pokeFilter.pokeStateData?.Value.interactionStrength;
        
        if (pokeStrength == 0)
        {
            PokeTween();
        }       
    }

    public void PokeTween()
    {
        currentTween?.Kill();

        currentTween = transform.DOLocalMove(tweenDepth, tweenDuration).OnComplete(() =>
        {
            isPressed = true;
        });
    }

    private void Release(SelectExitEventArgs args)
    {
        float? pokeStrength = pokeFilter.pokeStateData?.Value.interactionStrength;
       
        if (pokeStrength == 0)
        {
            ReleaseTween();
        }
        else
        {
            ToggleState();
        }
    }

    public void ReleaseTween()
    {
        currentTween?.Kill();

        if (isPressed)
        {
            ToggleState();
            isPressed = false;
        }

        currentTween = transform.DOLocalMove(initialPosition, tweenDuration);
    }

    private Vector3 LocalDirection(Transform t)
    {
        return axis switch
        {
            Axis.X => t.right,
            Axis.Y => t.up,
            _ => t.forward
        };
    }

    private int RelativeAxis(Vector3 direction)
    {
        Vector3 absDir = new(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));

        if (absDir.x > absDir.y && absDir.x > absDir.z)
        {
            return 0;
        }
        else if (absDir.y > absDir.x && absDir.y > absDir.z)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
}
