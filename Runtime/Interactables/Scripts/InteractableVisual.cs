using ImportExportScene;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableVisual : MonoBehaviour
{
    [SerializeField]
    private InteractableState[] stateObjects;

    private StateData CurrentState;
    private StateData PreviousState;

    public string StateName { get => CurrentState.name; }

    public int StateIndex { get => CurrentState.index; }

    [SerializeField]
    private Renderer interactionAffordanceRenderer;

    public Renderer InteractionAffordanceRenderer { get => interactionAffordanceRenderer; }

    private Transform parent;

    public Transform Parent { get => parent; }

    private bool setFirstStateOnStart = true;

    public Action<string> OnStateNameChanged = null;

    public InteractableStateChangerSocket CurrentSocket
    {
        get
        {
            for (int i = 0; i < stateObjects.Length; i++)
            {
                if (stateObjects[i].GetType() == typeof(GrabState))
                {
                    var grab = (GrabState)stateObjects[i];

                    if (grab.CurrentSocket)
                        return grab.CurrentSocket;
                }
            }

            return null;
        }
    }

    public ProgressStateTracker ProgressTracker(int stateIndex)
    {
        if (stateObjects != null && stateObjects.Length > 0 && stateObjects[stateIndex] != null)
            return stateObjects[stateIndex].ProgressTracker;
        else
            return null;
    }

    private void Awake()
    {
        parent = transform.parent;
        RegisterInteractable();
        
        //pb_PlayMode.instance.onPlay += SetFirstState;
    }

    private void Start()
    {
        SetFirstState();
    }

    private void OnDestroy()
    {
        UnregisterInteractable();

        //pb_PlayMode.instance.onPlay -= SetFirstState;
    }

    private void SetFirstState()
    {
        if (setFirstStateOnStart && stateObjects != null && stateObjects.Length > 0)
        {
            if (stateObjects[0] == null) return;
            stateObjects[0].Initialize(this);
            SetState(0, StateName);
        }
    }

    private void RegisterInteractable()
    {
        InteractableRegistry.RegisterInteractable(gameObject);

        if(InteractionAffordanceRenderer)
            InteractableRegistry.RegisterInteractable(InteractionAffordanceRenderer);       
    }

    private void UnregisterInteractable()
    {
        InteractableRegistry.UnregisterInteractable(gameObject);

        if (InteractionAffordanceRenderer)
            InteractableRegistry.UnregisterInteractable(InteractionAffordanceRenderer);
    }

    [Serializable]
    public struct StateData
    {
        public int index;
        public string name;

        public StateData(int index, string name)
        {
            this.index = index;
            this.name = name;
        }
    }

    public void SetState(string name)
    {
        if(StateName != name)
        {
            PreviousState = CurrentState;
            CurrentState = new(CurrentState.index, name);

            if (PreviousState.name == string.Empty)
                PreviousState = CurrentState;

            CustomEvent.Trigger(gameObject, "State Name Changed");
            OnStateNameChanged?.Invoke(name);

            // pb_PlayMode.instance.Evaluator.OnChallengeAction(new StateAction(CurrentState, gameObject));
        }      
    }

    public void SetState(int index)
    {
        SetState(index, StateName);
    }

    public void SetState(int index, string name)
    {       
        if(stateObjects.Length >= index)
        {
            PreviousState = CurrentState;            
            CurrentState = new(index, name);
            
            if (PreviousState.name == string.Empty)
                PreviousState = CurrentState;

            HandlePreviousState();

            // Set the visual to the current state object
            var currState = stateObjects[index];

            if (!currState.Initialized)
                currState.Initialize(this);

            currState.Set();
        }
        else
        {
            print("State index out of bounds");
        }
    }

    public InteractableState GetState()
    {
        return stateObjects[CurrentState.index];
    }
    // End the interaction on the previous state and set its progress to none
    private void HandlePreviousState()
    {
        if (PreviousState.Equals(CurrentState))
            return;

        if (stateObjects[PreviousState.index])
        {
            var prevState = stateObjects[PreviousState.index];

            if (prevState.ProgressTracker)
                prevState.ProgressTracker.SetState(Progress.None);

            prevState.EndInteraction();
        }
    }

    public void ActivateSocket(bool activate)
    {
        CurrentSocket.socketActive = activate;
    }

    public void SetSocketAvailable()
    {
        CurrentSocket.Remove();
    }
}
