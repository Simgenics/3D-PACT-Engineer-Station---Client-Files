using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualJoystick : ContinuousMoveProviderBase
{
    [SerializeField]
    private XRBaseInteractable joystick;

    [SerializeField]
    private Transform joystickHome;

    protected void OnEnable()
    {
        joystick.selectExited.AddListener(ReturnHome);
    }

    protected void OnDisable()
    {
        joystick.selectExited.RemoveListener(ReturnHome);
    }

    private void ReturnHome(SelectExitEventArgs args)
    {
        joystick.transform.DOLocalMove(joystickHome.localPosition, 0.2f);
    }

    protected override Vector2 ReadInput()
    {
        var input = Vector2.zero;
        Vector2 relativePos = GetRelativePosition(joystickHome, joystick.transform.localPosition);

        if (joystick.isSelected)
        {
            input += relativePos;
        }

        return input;
    }

    private Vector3 GetRelativePosition(Transform origin, Vector3 position)
    {
        Vector3 distance = position - origin.position;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
        relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
        relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

        return relativePosition;
    }
}
