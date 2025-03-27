using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ImportExportScene.Serialization
{
	public class pb_MaterialConverter : pb_UnityTypeConverter<Material>
	{
		public override void WriteObjectJson(JsonWriter writer, object value, JsonSerializer serializer)
	    {
	    	JObject o = new JObject();

	    	Material mat = ((Material) value);
			if (mat != null && mat.shader != null)
			{
				Shader shader = mat.shader;

				o.Add("name", mat.name);
				o.Add("shader", shader.name);

				// Ignore material 'Custom/Outline' with Shader 'Custom/Outline' (doesn't have a color property '_Color')
				if (shader.name != "Custom/Outline" && shader.name != "TextMeshPro/Distance Field" && shader.name != "TextMeshPro/Mobile/Distance Field" && shader.name != "TextMeshPro/Distance Field Overlay")
				{
					o.Add("color", colorToString(mat.color));
				}

				// Serialize shader props, these need to be explicitly defined
				if (shader.name == "Universal Render Pipeline/Autodesk Interactive/Autodesk Interactive")
				{
					o.Add("shaderProps", pb_AutodeskMaterialConverter.SerializeShaderProps(mat));
				}
				Debug.Log("Successfully stored material data for " + mat.name);
			}
			
	    	o.WriteTo(writer, serializer.Converters.ToArray());
	    }

		public override object ReadJsonObject(JObject obj, Type objectType, object existingValue, JsonSerializer serializer)
	    {
	    	try
	    	{
	    		string name = obj.GetValue("name").ToObject<string>();
	    		string shader = obj.GetValue("shader").ToObject<string>();
	            string color = obj.GetValue("color").ToObject<string>();

	            Material mat = new Material(Shader.Find(shader));
				mat.color = stringToColor(color);
	    		mat.name = name;
	            
#if ASSET_LOADER		            
	            // Manually reapply shader textures from loaded asset bundle
	            if (shader == "Universal Render Pipeline/Autodesk Interactive/Autodesk Interactive")
	            {
		            var props = obj.GetValue("shaderProps").ToObject<AutodeskShaderProps>();
					pb_AutodeskMaterialConverter.SetMaterialProps(mat, props, AssetLoader.instance.GetLoadedTextures);
	            }
#endif
	    		
	    		return mat;
	    	}
	    	catch (Exception e)
	    	{
		        Debug.LogWarning("Could not read JSON material:");
		        Debug.LogWarning(e);
	    		return null;
	    	}
	    }

		private static string colorToString(Color color)
		{
			return color.r + "," + color.g + "," + color.b + "," + color.a;
		}

	    private static Color stringToColor(string colorString)
		{
			try
			{
				string[] colors = colorString.Split(',');
				return new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3]));
			}
			catch
			{
				return Color.white;
			}
		}
	}
}