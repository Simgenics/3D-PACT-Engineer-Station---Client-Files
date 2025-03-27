using UnityEngine;

namespace SharedAssets
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T instance { get; protected set; }
        public static bool isInitialised { get { return instance != null; } }

        public virtual void Awake()
        {
            if (instance == null)
            {
                CreateInstance();
            }
            else
            {
                Debug.LogWarning("Existing instance found. Destroying this instance.");
                DestroyImmediate(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                ClearInstance();
            }
        }

        protected virtual void CreateInstance()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("Existing instance found. Destroying old instance.");
                Destroy(instance);
            }

            instance = (T)this;
        }

        protected virtual void ClearInstance()
        {
            instance = null;
        }
    }

}