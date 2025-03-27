using UnityEngine;

namespace ImportExportScene
{
	public abstract class Collection<T> : MonoBehaviour
	{
		public abstract GameObject GetMatchingObject(T source);
	}
}
