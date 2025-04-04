using UnityEngine;
using ImportExportScene.Serialization;

namespace ImportExportScene
{
	/**
	 * Extension classes that make working with serialized types interchangeably with unity types easier
	 */
	public static class pb_ComponentExtension
	{
		/**
		 * Attempts to add a component from a pb_ISerializable object.  If `component` is not a type
		 * inheriting `UnityEngine.Component`, a null value is returned.  Otherwise the component is
		 * added to the gameObject `go` and it's fields are populated using the values stored in the
		 * dictionary as set by pb_ISerializable.PopulateDictionaryValues().
		 */
		public static Component AddComponent(this GameObject go, Component[] existingComponents, pb_ISerializable component)
		{
			if( !typeof(Component).IsAssignableFrom(component.type) )
			{
				Debug.LogError(component.type + " does not inherit UnityEngine.Component!");
				return null;
			}

			Component c = null;
            for (int i = 0; i < existingComponents.Length; i++)
            {
				Component originalComp = existingComponents[i];
                if (originalComp != null && originalComp.GetType() == component.type)
                {
					c = originalComp;
					existingComponents[i] = null;
					break;
				}
            }
            if (c == null)
            {
				c = go.AddComponent(component.type);
            }

			component.ApplyProperties(c);

			return c;
		}

		/**
		 * Shortcut for if(!GetComponent<T>) AddComponent<T>.
		 */
		public static T DemandComponent<T>(this GameObject go) where T : UnityEngine.Component
		{
			T component = go.GetComponent<T>();

			if(component == null)
				component = go.AddComponent<T>();

			return component;
		}
	}
}
