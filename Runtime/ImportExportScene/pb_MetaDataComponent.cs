using UnityEngine;
using ImportExportScene.Serialization;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ImportExportScene
{
	/**
	 * Metadata components are used to serialize and deserialize prefabs pointing
	 * to AssetBundle or Resource folder objects.  Can also mark an object as an
	 * instance asset, meaning the serializer will write all the components and
	 * information necessary to rebuild the object on deserialization.  If asset 
	 * is type AssetBundle or Resource it will be loaded from it's respective 
	 * location.
	 */
	[DisallowMultipleComponent]
	[pb_JsonIgnore]
	public class pb_MetaDataComponent : MonoBehaviour
	{
		/// Reference metadata information about a prefab or gameObject.  Used to
		/// serialize and deserialize prefabs/instance objects.
		public pb_MetaData metadata = new pb_MetaData();

		private void Awake() => gameObject.SetActive(metadata.GetActive());

		[Button]
		public void Activate()
		{
			if (metadata.objectId == "MetaData_NoGUIDPresent")
			{
				metadata.SetObjectId();
			}
		}

		private void Start()
        {
			if (metadata.objectId == "MetaData_NoGUIDPresent")
			{
				metadata.SetObjectId();
			}
			if (!pb_Scene.currentComponents.ContainsKey(metadata.objectId))
				pb_Scene.currentComponents.Add(metadata.objectId, this);
        }

        private void OnEnable() => metadata.SetActive(true);

		private void OnDisable() => metadata.SetActive(false);
		
		/**
		 * Set the name and asset path that this object can be found with.
		 */
		public void SetAssetBundleData(string bundleName, string assetPath)
		{
			metadata.SetAssetBundleData(bundleName, assetPath);
#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
#endif
		}

		/**
		 * Set the fileId field if this asset is in the resources folder.
		 */
		public bool UpdateFileId()
		{
			bool modified = false;

#if UNITY_EDITOR
			if (PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.Regular && metadata.assetType != AssetType.Bundle)
			{
				string path = AssetDatabase.GetAssetPath(this.gameObject);
				string guid = AssetDatabase.AssetPathToGUID(path);

				if (!string.IsNullOrEmpty(metadata.fileId) && !guid.Equals(metadata.fileId))
				{
					Debug.Log("Level Editor: Resource fileId changed -> " + this.gameObject.name + " (" + metadata.fileId + " -> " + guid + ")");
					modified = true;
				}

				metadata.SetFileId(guid);

				EditorUtility.SetDirty(this);
			}
#endif
			return modified;
		}

		public string GetFileId()
		{
			return metadata.fileId;
		}

		public void SetMetaParticleEffectType(EffectType type)
		{
			metadata.effectType = type;
		}

		public void DestroyObject()
		{
			pb_Scene.currentComponents.Remove(metadata.objectId);
			Destroy(gameObject);
		}
	}
}
