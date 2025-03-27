using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ProgressStateTrackerGroup : MonoBehaviour
{
    [SerializeField] private string groupState;
    [SerializeField] private List<ProgressGroup> progressGroups = new();

    public string GetGroupStateName() => groupState;

    [Serializable]
    // When tracker conditions are met, call OnProgressReached
    public class ProgressGroup
    {
        public string name;
        public List<TrackerCondition> conditions;
        public UnityEvent OnProgressReached;
    }

    [Serializable]
    // What progress state should this tracker have?
    public struct TrackerCondition
    {
        public ProgressStateTracker tracker;
        public Progress progress;
    }

    private void Awake()
    {
        // When the tracker's progress changes, check if the desired progress state was reached
        foreach (var group in progressGroups)
        {
            foreach (var condition in group.conditions)
                condition.tracker.OnProgressChanged += CheckProgressReached;
        }
    }

    private void OnDestroy()
    {
        foreach (var group in progressGroups)
        {
            foreach (var condition in group.conditions)
                condition.tracker.OnProgressChanged -= CheckProgressReached;
        }
    }

    private void CheckProgressReached()
    {
        foreach (var group in progressGroups)
        {
            if (ProgressReached(group))
            {
                group.OnProgressReached?.Invoke();
                groupState = group.name;
                CustomEvent.Trigger(gameObject, "Progress Changed");
            }
        }
    }

    private bool ProgressReached(ProgressGroup group)
    {
        foreach (var condition in group.conditions)
        {
            if (condition.tracker.GetState() != condition.progress)
                return false;
        }

        return true;
    }
}
