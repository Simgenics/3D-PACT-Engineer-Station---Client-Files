using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class PullState : InteractableState
{
    [SerializeField]
    private enum Axis { X, Y, Z };

    [SerializeField]
    [Tooltip("The min limit is the current transform position on the defined axis. " +
        "The max limit is the minimum limit + this value.")]
    private float unitsUntilLimit = 1;

    [SerializeField]
    [Tooltip("Set if you don't want the position to start at the current position.")]
    private float startOffset = 0;

    private Vector3 minLimit, maxLimit;

    [SerializeField]
    private bool useLocal = false;

    [SerializeField]
    private Axis axis = Axis.Z;

    [SerializeField]
    private Transform attachTransform;

    private PullGrabTransformer pullGrabTransformer;
    private Vector3 offsetPos;
    public Vector3 MinLimit() => minLimit;
    public Vector3 MaxLimit() => maxLimit;

    public float UnitsUntilLimit() => unitsUntilLimit;

    private void OnDestroy()
    {
        Interactable.selectExited.RemoveListener(CheckPosition);
    }

    protected override void AddComponents()
    {
        base.AddComponents();

        // Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // Interactable
        XRGrabInteractable interactable = GetComponent<XRGrabInteractable>()
            ? GetComponent<XRGrabInteractable>()
            : gameObject.AddComponent<XRGrabInteractable>();        

        SetInteractable(interactable);

        interactable.trackRotation = false;

        attachTransform = attachTransform ? attachTransform : transform;
        interactable.attachTransform = attachTransform;
        interactable.secondaryAttachTransform = attachTransform;

        // Transformer
        pullGrabTransformer = GetComponent<PullGrabTransformer>()
            ? GetComponent<PullGrabTransformer>()
            : gameObject.AddComponent<PullGrabTransformer>();

        pullGrabTransformer.constrainedAxisDisplacementMode = PullGrabTransformer.ConstrainedAxisDisplacementMode.WorldAxisRelative;
        
        pullGrabTransformer.permittedDisplacementAxes = axis switch
        {
            Axis.X => PullGrabTransformer.ManipulationAxes.X,
            Axis.Y => PullGrabTransformer.ManipulationAxes.Y,
            _ => PullGrabTransformer.ManipulationAxes.Z,
        };
    }

    protected override void Initialize()
    {        
        Interactable.selectExited.AddListener(CheckPosition);

        float startAxisPos = PositionOnAxis(transform.position);
        float endAxisPos = startAxisPos + unitsUntilLimit;

        Vector3 minAxisLimit = AxisVector() * startAxisPos;
        Vector3 maxAxisLimit = AxisVector() * endAxisPos;

        if (unitsUntilLimit > 0)
        {
            minLimit = minAxisLimit + (transform.position - minAxisLimit);
            maxLimit = maxAxisLimit + (transform.position - minAxisLimit);
        }
        else if (unitsUntilLimit < 0)
        {
            minLimit = maxAxisLimit + (transform.position - minAxisLimit);
            maxLimit = minAxisLimit + (transform.position - minAxisLimit);
        }

        // gameObject.AddComponent<PC_PullState>();
    }

    public override void Set()
    {
        base.Set();

        Vector3 startLimit = unitsUntilLimit > 0 ? minLimit : maxLimit;

        offsetPos = AxisVector() * (PositionOnAxis(startLimit) + startOffset);
        transform.position = (offsetPos + transform.position) - Vector3.Scale(transform.position, AxisVector());
    }

    public Vector3 ClampPoint(Vector3 point)
    {
        return ClampProjection(ProjectPoint(point));
    }

    public Vector3 ProjectPoint(Vector3 point)
    {
        return minLimit + Vector3.Project(point - minLimit, maxLimit - minLimit);
    }

    private Vector3 ClampProjection(Vector3 point)
    {
        var toStart = (point - minLimit).sqrMagnitude;
        var toEnd = (point - maxLimit).sqrMagnitude;
        var segment = (minLimit - maxLimit).sqrMagnitude;
        if (toStart > segment || toEnd > segment) return toStart > toEnd ? maxLimit : minLimit;
        return point;
    }

    private float PositionOnAxis(Vector3 position)
    {
        return axis switch
        {
            Axis.X => position.x,
            Axis.Y => position.y,
            _ => position.z,
        };
    }

    private Vector3 AxisVector()
    {
        return axis switch
        {
            Axis.X => useLocal ? transform.right : Vector3.right,
            Axis.Y => useLocal ? transform.up : Vector3.up,
            _ => useLocal ? transform.forward : Vector3.forward,
        };
    }

    private void CheckPosition(SelectExitEventArgs args)
    {
        if (PositionOnAxis(transform.position) <= PositionOnAxis(minLimit))
        {
            ProgressTracker.SetState(unitsUntilLimit > 0 ? Progress.Start : Progress.End);
        }
        else if (PositionOnAxis(transform.position) >= PositionOnAxis(maxLimit))
        {
            ProgressTracker.SetState(unitsUntilLimit > 0 ? Progress.End : Progress.Start);
        }
        else
        {
            ProgressTracker.SetState(Progress.Middle);
        }
    }
}
