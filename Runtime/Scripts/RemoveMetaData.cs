using ImportExportScene;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAllMetaData : MonoBehaviour
{
    [Button]
    public void RemoveMetaDataInChildren()
    {
        List<Transform> allChildren = new();
        GetChildrenRecursive(transform, allChildren);

        for (int i = 0; i < allChildren.Count; i++)
        {
            if (allChildren[i].GetComponent<pb_MetaDataComponent>() == null)
                continue;

            DestroyImmediate(allChildren[i].GetComponent<pb_MetaDataComponent>());
        }
    }

    private void GetChildrenRecursive(Transform parent, List<Transform> children)
    {
        foreach (Transform child in parent)
        {
            children.Add(child);
            GetChildrenRecursive(child, children);
        }
    }
}
