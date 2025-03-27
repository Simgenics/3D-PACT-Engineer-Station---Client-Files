using UnityEngine;
using Newtonsoft.Json;

namespace ImportExportScene.Serialization
{
    public static class pb_Serialization 
    {
		public static readonly JsonSerializerSettings ConverterSettings = new JsonSerializerSettings
		{
			MissingMemberHandling = MissingMemberHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			TypeNameHandling = TypeNameHandling.Objects,
			ContractResolver = new pb_ContractResolver(),
#if UNITY_EDITOR // Neat readable JSON
			Formatting = Formatting.Indented, 
#else // Remove formatting to optimize for production
			Formatting = Formatting.None
#endif
		};

		public static readonly JsonSerializer Serializer = JsonSerializer.Create(ConverterSettings);

		/**
		 * Checks for a custom written built-in component converter first, then if none
		 * found returns the default converter.
		 */
		public static pb_ISerializable CreateSerializableObject<T>(T obj)
		{
			return obj switch
			{
				MeshFilter filter => new pb_MeshFilter(filter),
				MeshRenderer renderer => new pb_MeshRenderer(renderer),
				MeshCollider collider => new pb_MeshCollider(collider),
				Camera camera => new pb_CameraComponent(camera),
				// AudioSource source => new pb_AudioSource(source),
				_ => new pb_SerializableObject<T>(obj)
			};
		}
	}
}
