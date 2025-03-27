using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualSnapTurn : SnapTurnProviderBase
{
    [SerializeField]
    private XRBaseInteractable leftArrow;
    [SerializeField]
    private XRBaseInteractable rightArrow;

    private Vector2 turnDirection = Vector2.zero;

    protected void OnEnable()
    {
        leftArrow.selectEntered.AddListener(TurnLeft);
        rightArrow.selectEntered.AddListener(TurnRight);

        leftArrow.selectExited.AddListener(StopTurn);
        rightArrow.selectExited.AddListener(StopTurn);
    }

    private void OnDisable()
    {
        leftArrow.selectEntered.RemoveListener(TurnLeft);
        rightArrow.selectEntered.RemoveListener(TurnRight);

        leftArrow.selectExited.RemoveListener(StopTurn);
        rightArrow.selectExited.RemoveListener(StopTurn);
    }

    protected override Vector2 ReadInput()
    {
        return turnDirection;
    }

    private void TurnLeft(SelectEnterEventArgs args)
    {
        turnDirection = Vector2.left;
    }

    private void TurnRight(SelectEnterEventArgs args)
    {
        turnDirection = Vector2.right;
    }

    private void StopTurn(SelectExitEventArgs args)
    {
        turnDirection = Vector2.zero;
    }
}
