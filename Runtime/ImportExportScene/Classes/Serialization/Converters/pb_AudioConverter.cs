using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ImportExportScene.Serialization
{
	[JsonConverter(typeof(pb_AudioSource))]
	public class pb_AudioConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	    {
	        Debug.Log("Write audio JSON");

	    	JObject o = new JObject();
	        o.Add("$type", value.GetType().AssemblyQualifiedName);
	        var xx = value as pb_AudioSource;
	        var dict = xx?.PopulateSerializableDictionary();

	        if (dict != null)
		        foreach (var (key, obj) in dict)
		        {
			        if (key != null && obj != null)
			        {
				        o.Add(key, obj.ToString());
			        }
		        }

	        o.WriteTo(writer, serializer.Converters.ToArray());
	    }
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	    {
		    Debug.Log("Reading audio JSON");
		    JObject o = JObject.Load(reader);

		    var val = o.GetValue("$values");
		    if (val is { HasValues: true })
		    {
				var kids = val.Children().ToList();
				var present = kids.First(kid => kid.HasValues);
				var presentObject = present.ToObject<JObject>();
				if (presentObject is { HasValues: true })
				{
					presentObject.TryGetValue("$type", out var objType);
					if (objType != null)
					{
						if (objType.ToString().Contains(typeof(pb_AudioSource).Namespace!))
						{
							Debug.Log("Audio Source found");
							// var audio = float.Parse(presentObject.GetValue("volume")?.ToString() ?? string.Empty);
							// var pitch = (float) presentObject.GetValue("pitch");
							// var time = (float) presentObject.GetValue("time");
							// src.volume = audio;
							// src.pitch = pitch;
							// src.time = time;
						}
						// else
						// {
							// var other = ReadJson(reader, objectType, existingValue, serializer);
						// }
					}
					else
					{
						Debug.LogError("Could not find type of component");
					}
				}
		    }
		    return null;
	    }
		public override bool CanConvert(Type objectType) => typeof(AudioSource).IsAssignableFrom(objectType);
	}
}