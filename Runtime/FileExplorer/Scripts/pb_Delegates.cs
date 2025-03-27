using UnityEngine;

namespace FileDialogSystem
{
    public delegate bool MouseInUse(Vector2 mousePosition);

    public delegate bool KeyInUse();

    public delegate void Callback();

    public delegate void Callback<T>(T value);

    public delegate void Callback<T, S>(T value0, S value1);
}
