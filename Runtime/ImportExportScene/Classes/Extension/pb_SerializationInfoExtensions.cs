using System;
using System.Runtime.Serialization;
using UnityEngine;

public static class pb_SerializationInfoExtensions 
{
    public static T TryGetValue<T>(this SerializationInfo target, string property) where T : class
    {
        var enumerator = target.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Name == property && enumerator.Current.Value != null)
                {
                    return target.GetValue(property, typeof(T)) as T;
                }
            }
        }
        catch (Exception)
        {
            Debug.LogError($"TryGetValue [{property}] {typeof(T)}");
        }
        return null;
    }
}
