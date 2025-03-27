#if UNITY_EDITOR
using System.Collections;
using UnityEditor;

public class EditorCoroutine
{
    public static EditorCoroutine Start(IEnumerator routine)
    {
        EditorCoroutine coroutine = new EditorCoroutine(routine);
        coroutine.Start();
        return coroutine;
    }

    readonly IEnumerator routine;
    bool completed;
    EditorCoroutine(IEnumerator _routine)
    {
        routine = _routine;
        completed = false;
    }

    void Start()
    {
        EditorApplication.update += Update;
    }
    public void Stop()
    {
        EditorApplication.update -= Update;
        completed = true;
    }

    public bool GetCompleted()
    {
        return completed;
    }

    void Update()
    {
        if (!routine.MoveNext())
        {
            Stop();
        }
    }
}
#endif