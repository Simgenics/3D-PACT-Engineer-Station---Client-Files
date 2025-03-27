using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public enum Progress { None = -1, Start = 0, Middle = 1, End = 2 }

public class ProgressStateTracker : MonoBehaviour
{
    [SerializeField] private Progress currentState = Progress.None;

    private Progress CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (currentState == value)
                return;

            currentState = value;
            StateChanged?.Invoke();
            OnProgressChanged?.Invoke();
            CustomEvent.Trigger(gameObject, "Progress Changed");

            switch (currentState)
            {
                case Progress.Start:
                    ReachedStart?.Invoke();
                    OnReachedStart?.Invoke();
                    break;
                case Progress.Middle:
                    InMiddle?.Invoke();
                    OnMiddle?.Invoke();
                    break;
                case Progress.End:
                    ReachedEnd?.Invoke();
                    OnReachedEnd?.Invoke();
                    break;
            }
        }
    }

    public void SetState(Progress state)
    {
        CurrentState = state;
    }
    public void SetState(int state) => CurrentState = (Progress)state;
    public Progress GetState() => CurrentState;

    public bool StateEquals(Progress state) => CurrentState == state;

    public UnityEvent ReachedStart, InMiddle, ReachedEnd, StateChanged;
    public System.Action OnReachedStart, OnMiddle, OnReachedEnd, OnProgressChanged;
}
