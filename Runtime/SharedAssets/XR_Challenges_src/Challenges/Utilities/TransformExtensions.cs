using UnityEngine;

namespace NodeEditorFramework.Utilities
{
    public static class TransformExtensions
    {
        public static Transform DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
            return transform;
        }

        public static Transform DestroyChildrenWithComponent<T>(this Transform transform) where T : Component
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<T>() != null)
                {
                    Object.Destroy(child.gameObject);
                }
            }
            return transform;
        }

        public static Transform ReverseChildOrder(this Transform transform)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount - 1; i++)
            {
                transform.GetChild(childCount - 1).SetAsFirstSibling();
            }
            return transform;
        }
    }
}