using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSwitcher : MonoBehaviour
{
    public enum State { Available = 0, Filled = 1 }

    public State state;

    public void SetState(int toState) => state = (State)toState;
}
