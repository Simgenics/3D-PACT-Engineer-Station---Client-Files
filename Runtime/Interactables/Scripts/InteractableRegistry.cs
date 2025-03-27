using System.Collections.Generic;
using UnityEngine;

public class InteractableRegistry : MonoBehaviour
{
    public static List<GameObject> AllInteractableGameObjects = new();
    public static List<Renderer> AllInteractableRenderers = new();

    public static System.Action<GameObject> OnRegisterInteractableGameObject;
    public static System.Action<Renderer> OnRegisterInteractableRenderer;

    public static void RegisterInteractable(GameObject interactable)
    {       
        if (!AllInteractableGameObjects.Contains(interactable))
        {
            AllInteractableGameObjects.Add(interactable);

            OnRegisterInteractableGameObject?.Invoke(interactable);
        }
    }

    public static void UnregisterInteractable(GameObject interactable)
    {
        if (!AllInteractableGameObjects.Contains(interactable))
        {
            AllInteractableGameObjects.Add(interactable);
        }
    }

    public static void RegisterInteractable(Renderer interactable)
    {
        if (!AllInteractableRenderers.Contains(interactable))
        {
            AllInteractableRenderers.Add(interactable);

            OnRegisterInteractableRenderer?.Invoke(interactable);
        }
    }

    public static void UnregisterInteractable(Renderer interactable)
    {
        if (!AllInteractableRenderers.Contains(interactable))
        {
            AllInteractableRenderers.Add(interactable);
        }
    }
}
