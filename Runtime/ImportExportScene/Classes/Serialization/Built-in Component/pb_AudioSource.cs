using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace ImportExportScene.Serialization
{
	/**
	 * Specialized component wrapper for serializing and deserializing Camera components.
	 */
    [System.Serializable]
	public class pb_AudioSource : pb_SerializableObject<AudioSource>
	{
		[JsonConstructor]
		public pb_AudioSource(AudioSource obj) : base(obj)
		{
		}

		public pb_AudioSource(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public override Dictionary<string, object> PopulateSerializableDictionary()
        {
            Dictionary<string, object> properties = base.PopulateSerializableDictionary();
            properties.Remove("clip");
            return properties;
        }
    }
}
