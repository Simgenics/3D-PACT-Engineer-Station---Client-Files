using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ImportExportScene.Serialization
{
    /**
	 * Container class for Unity component types, used to serialize and reconstitute components.  If you want to override
	 * serialization behavior for your MonoBehaviour, implement the pb_ISerializableComponent interface.
	 */
    [System.Serializable]
    public class pb_SerializableObject<T> : pb_ISerializable
    {
		/// A reference to the component being serialized.  Will be null on deserialization.
		protected T target;

		public Type type { get; set; }

		/// A key-value store of all serializable properties and fields on this object.  Populated on serialization & deserialization.
		protected Dictionary<string, object> reflectedProperties;

		/**
		 * Create a new serializable object from a component.
		 */
		public pb_SerializableObject(T obj)
		{
			this.target = obj;
		}

		/**
		 * Explicit cast return target.  If obj is null but reflectedProperties is valid, a new instance
		 * of T is returned with those properties applied.  The new instance is constructed using default(T).
		 */
		public static explicit operator T(pb_SerializableObject<T> obj)
		{
			if (obj.target == null)
			{
				T val = default(T);
				obj.ApplyProperties(val);
				return val;
			}
			else
			{
				return obj.target;
			}
		}

		/**
		 * Constructor coming from serialization.
		 */
		public pb_SerializableObject(SerializationInfo info, StreamingContext context)
		{
			string typeName = (string)info.GetValue("typeName", typeof(string));
			type = Type.GetType(typeName);
			reflectedProperties = info.TryGetValue<Dictionary<string, object>>("reflectedProperties");
		}

		/**
		 * Serialize data for ISerializable.
		 */
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (target == null)
			{
				Debug.LogWarning("Attempting to serialize a null object.  This is likely a bug.");
				return;
			}
			
			var targetType = target.GetType();
			info.AddValue("typeName", targetType.AssemblyQualifiedName, typeof(string));

			reflectedProperties = PopulateSerializableDictionary();
			info.AddValue("reflectedProperties", reflectedProperties, typeof(Dictionary<string, object>));
		}

		public virtual void ApplyProperties(object obj)
		{
			pb_ISerializableComponent ser = obj as pb_ISerializableComponent;

			if (ser != null)
			{
				ser.ApplyDictionaryValues(reflectedProperties);
			}
			else
			{
				pb_Reflection.ApplyProperties(obj, reflectedProperties);
			}
		}

		public virtual Dictionary<string, object> PopulateSerializableDictionary()
		{
			pb_ISerializableComponent ser = target as pb_ISerializableComponent;

			if (ser != null)
				return ser.PopulateSerializableDictionary();
			else
				return pb_Reflection.ReflectProperties(target);
		}
	}
}
