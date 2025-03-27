using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketState : MonoBehaviour
{
    private InteractableStateChangerSocket socket;

    [SerializeField]
    private Transform attachTransform;

    [SerializeField]
    private bool showHoverMeshes = true;

    private void Awake()
    {
        socket = gameObject.AddComponent<InteractableStateChangerSocket>();

        attachTransform = attachTransform ? attachTransform : transform;
        socket.attachTransform = attachTransform;

        socket.showInteractableHoverMeshes = showHoverMeshes;
    }
}
