using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateState : InteractableState
{
    [SerializeField]
    private enum Axis { X, Y, Z };

    private bool isGrabbed = false;

    private XRBaseController interactingController;

    [SerializeField]
    private bool useLocal = false;
    [SerializeField]
    private Axis axis = Axis.Z;
    private Vector3 axisVector = Vector3.forward;
    private Vector3 referenceVector;

    private Transform visualTransform;

    [SerializeField]
    [Tooltip("The min limit is the current transform rotation on the defined axis. " +
        "The max limit is the minimum limit + this value. Set it positive for clockwise and negative for anti-clockwise.")]
    private float degreesUntilLimit = 360;

    [SerializeField]
    [Tooltip("Set if you don't want the rotation to start at the current rotation.")]
    private float startOffset = 0;

    private float minLimit = 0, maxLimit = 0;

    [SerializeField]
    private Transform pivotTransform;
    private Vector3 pivotPoint;    

    private Quaternion grabPointOffset;

    private float currentAngle = 0;
    private float cumulativeAngle = 0;

    public Vector3 PivotPoint => pivotTransform.position;
    public Vector3 AxisVector => CalculateAxisVector();
    public float DegreesUntilLimit => degreesUntilLimit;
    public float MinLimit => minLimit;
    public float MaxLimit => maxLimit;
    public Vector3 ReferenceVector => CalculateReferenceVector();

    protected override void AddComponents()
    {
        base.AddComponents();

        XRBaseInteractable interactable = GetComponent<XRSimpleInteractable>()
            ? GetComponent<XRSimpleInteractable>()
            : gameObject.AddComponent<XRSimpleInteractable>();

        SetInteractable(interactable);

        // gameObject.AddComponent<PC_RotateState>();  
    }

    protected override void Initialize()
    {
        visualTransform = visual.transform;

        if (!pivotTransform)
            pivotTransform = visualTransform;

        pivotPoint = pivotTransform.position;

        axisVector = CalculateAxisVector();
        referenceVector = CalculateReferenceVector();               

        float startReferenceAngle = GetRotationOnAxis(visualTransform.rotation);       

        // Clockwise
        if (degreesUntilLimit > 0)
        {
            minLimit = startReferenceAngle;
            maxLimit = startReferenceAngle + degreesUntilLimit;
        }
        // Anti-clockwise
        else if (degreesUntilLimit < 0)
        {
            minLimit = startReferenceAngle + degreesUntilLimit;
            maxLimit = startReferenceAngle;
        }       

        visualTransform.RotateAround(pivotPoint, axisVector, startOffset);

        Interactable.selectEntered.AddListener(StartRotate);
        Interactable.selectExited.AddListener(EndRotate);             
    }
    private void OnDestroy()
    {
        Interactable.selectEntered.RemoveListener(StartRotate);
        Interactable.selectExited.RemoveListener(EndRotate);
    }

    private void StartRotate(SelectEnterEventArgs args)
    {       
        isGrabbed = true;

        GameObject interactorGo = args.interactorObject.transform.gameObject;

        interactingController = interactorGo.GetComponent<XRBaseController>() 
            ? interactorGo.GetComponent<XRBaseController>() 
            : interactorGo.GetComponentInParent<XRBaseController>();

        Vector3 initialDirectionToController = interactingController.transform.position - pivotPoint;

        grabPointOffset = Quaternion.Inverse(Quaternion.LookRotation(axisVector, initialDirectionToController)) * visualTransform.rotation;        
    }

    private void EndRotate(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (currentAngle == minLimit)
        {
            ProgressTracker.SetState(Progress.Start);
        }
        else if (currentAngle == maxLimit)
        {
            ProgressTracker.SetState(Progress.End);
        }
    }

    void Update()
    {
        if (isGrabbed) 
        {
            // Vector from the valve's center to the controller's position
            Vector3 directionToController = interactingController.transform.position - pivotPoint;

            directionToController.Normalize();

            // Calculate the rotation from the grab point
            Quaternion targetRotation = Quaternion.LookRotation(axisVector, directionToController);
            targetRotation *= grabPointOffset;

            float targetAngle = GetRotationOnAxis(targetRotation) - GetRotationOnAxis(Quaternion.identity);

            float angleDelta = Mathf.DeltaAngle(currentAngle, targetAngle);

            cumulativeAngle = currentAngle + angleDelta;

            currentAngle = Mathf.Clamp(cumulativeAngle, minLimit, maxLimit);

            if (Mathf.Abs(angleDelta) > 0.01f)
            {
               visualTransform.RotateAround(pivotPoint, axisVector, currentAngle - GetRotationOnAxis(visualTransform.rotation));
            }

            WhileSelecting?.Invoke(currentAngle);
        }
    }

    public float GetRotationOnAxis(Quaternion rotation)
    {
        axisVector = CalculateAxisVector();
        referenceVector = CalculateReferenceVector();
        
        // Project the rotation onto the plane perpendicular to the axis
        Vector3 projectedDirection = Vector3.ProjectOnPlane(rotation * referenceVector, axisVector).normalized;

        // Compute the signed angle relative to the reference direction
        return Vector3.SignedAngle(referenceVector, projectedDirection, axisVector);
    }

    private Vector3 CalculateAxisVector()
    {
        switch (axis)
        {
            case Axis.X:
                return useLocal ? visualTransform.right : Vector3.right;
            case Axis.Y: 
                return useLocal ? visualTransform.up : Vector3.up;           
            default:
            case Axis.Z: 
                return useLocal ? visualTransform.forward : Vector3.forward;
        }
    }

    private Vector3 CalculateReferenceVector()
    {
        Vector3 localPivotDirection = visualTransform.InverseTransformPoint(pivotPoint).normalized;

        float localX = Mathf.Abs(localPivotDirection.x);
        float localY = Mathf.Abs(localPivotDirection.y);
        float localZ = Mathf.Abs(localPivotDirection.z);

        bool facingY = localY > localX && localY > localZ;
        bool facingZ = localZ > localX && localZ > localY;

        if (axis == Axis.Y)
        {
            return facingZ ? Vector3.forward : Vector3.right;
        }
        else if (axis == Axis.X)
        {
            return facingZ ? Vector3.up : Vector3.forward;
        }
        else
        {
            return facingY ? Vector3.up : Vector3.right;
        }     
    }
}
