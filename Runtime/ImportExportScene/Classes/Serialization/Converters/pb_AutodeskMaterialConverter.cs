using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ImportExportScene.Serialization
{
	[Serializable]
	public class AutodeskShaderProps
	{
		public float useColorMap;
		public Color32 color;
		public TextureRef colorMap;

		// Bumps
		public float useNormalMap;
		public TextureRef bumpMap;

		// Metallic
		public float useMetallicMap;
		public float metallic;
		public TextureRef metallicGlossMap;

		// Roughness
		public float useRoughnessMap;
		public float glossiness;
		public TextureRef specGlossMap;
	}
	
	[Serializable]
	public class TextureRef
	{
		public string name;
		public string hash;
	}
	
    public static class pb_AutodeskMaterialConverter
    {
	    private static readonly int UseColorMap = Shader.PropertyToID("_UseColorMap");
	    private static readonly int ColorMain = Shader.PropertyToID("_Color");
	    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		
	    private static readonly int UseNormalMap = Shader.PropertyToID("_UseNormalMap");
	    private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
		
	    private static readonly int UseMetallicMap = Shader.PropertyToID("_UseMetallicMap");
	    private static readonly int Metallic = Shader.PropertyToID("_Metallic");
	    private static readonly int MetallicGlossMap  = Shader.PropertyToID("_MetallicGlossMap");
		
	    private static readonly int UseRoughnessMap = Shader.PropertyToID("_UseRoughnessMap");
	    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness"); // Roughness
	    private static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap"); // Roughness Map
	    
        private static Texture LoadTextureAssetBundle(TextureRef texRef, List<Texture2D> loadedTextures)
        {		    
		    
	        // Find the correct asset
	        var item = loadedTextures.ToList().Find(asset =>
	        {
		        // Make sure asset name and image hash matches
		        bool nameMatch = asset.name.Equals(texRef.name);
		        //bool hashMatch = asset.imageContentsHash.ToString().Equals(texRef.hash);
		        return nameMatch/* && hashMatch*/;
	        });
		    
	        return item;
        }

        public static JObject SerializeShaderProps(Material mat)
		{
			try
			{
				var props = new AutodeskShaderProps();

				var colorMap = mat.GetTexture(MainTex);
				props.useColorMap = mat.GetFloat(UseColorMap);
				props.color = (Color32)mat.GetColor(ColorMain);
				props.colorMap = new TextureRef
				{
					name = colorMap ? colorMap.name : "",
					//hash = colorMap ? colorMap.imageContentsHash.ToString() : ""
				};
				
				var bumpMap = mat.GetTexture(BumpMap);
				props.useNormalMap = mat.GetFloat(UseNormalMap);
				props.bumpMap = new TextureRef
				{
					name = bumpMap? bumpMap.name : "",
					//hash = bumpMap? bumpMap.imageContentsHash.ToString() : ""
				};
					
				var metalMap = mat.GetTexture(MetallicGlossMap);
				props.useMetallicMap = mat.GetFloat(UseMetallicMap);
				props.metallic = mat.GetFloat(Metallic);
				props.metallicGlossMap = new TextureRef
				{
					name = metalMap ? metalMap.name : "",
					//hash = metalMap ? metalMap.imageContentsHash.ToString() : ""
				};

				var roughnessMap = mat.GetTexture(SpecGlossMap);
				props.useRoughnessMap = mat.GetFloat(UseRoughnessMap);
				props.glossiness = mat.GetFloat(Glossiness);
				props.specGlossMap = new TextureRef
				{
					name = roughnessMap ? roughnessMap.name : "",
					//hash = roughnessMap ? roughnessMap.imageContentsHash.ToString() : ""
				};
				
				return (JObject) JToken.FromObject(props);
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
				throw new Exception("Could not serialize shader");
			}
		}
        
        public static void SetMaterialProps(Material mat, AutodeskShaderProps props, List<Texture2D> loadedTextures)
        {
	        Debug.Log($"");
	        Debug.Log($"Trying to set {mat} material props...");
		    
	        var mainTexture = LoadTextureAssetBundle(props.colorMap, loadedTextures);
	        var bumpTexture = LoadTextureAssetBundle(props.bumpMap, loadedTextures);
	        var metalTexture = LoadTextureAssetBundle(props.metallicGlossMap, loadedTextures);
	        var roughTexture = LoadTextureAssetBundle(props.specGlossMap, loadedTextures);
		    
	        if (mainTexture)
	        {
		        mat.SetFloat(UseColorMap, props.useColorMap);
		        mat.SetColor(ColorMain, props.color);
		        mat.SetTexture(MainTex, mainTexture);
	        }

	        if (bumpTexture)
	        {
		        mat.SetFloat(UseNormalMap, props.useNormalMap);
		        mat.SetTexture(BumpMap, bumpTexture);
	        }

	        if (metalTexture)
	        {
		        mat.SetFloat(UseMetallicMap, props.useMetallicMap);
		        mat.SetFloat(Metallic, props.metallic);
		        mat.SetTexture(MetallicGlossMap, metalTexture);
	        }
		    
	        if (roughTexture) {
		        mat.SetFloat(UseRoughnessMap, props.useRoughnessMap);
		        mat.SetFloat(Glossiness, props.glossiness);
		        mat.SetTexture(SpecGlossMap, roughTexture);
	        }
			
	        Debug.Log("Material updated successful :)");
        }
    }
}