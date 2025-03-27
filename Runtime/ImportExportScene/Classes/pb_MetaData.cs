using UnityEngine;
using System.Runtime.Serialization;
using ImportExportScene.Serialization;
using Random = UnityEngine.Random;

namespace ImportExportScene
{

	public enum InstanceType
	{
		Waypoint,
		PathNode,
		Particles,
		PlayerSpawnLocation,
		NPCSpawnLocation
	}

	public enum EffectType
	{
		None,
		Blanket,
		Sprinkler,
		Sparks,
		Fire,
		Smoke,
		Explosion,
		WaterEffect
	}

	/**
	 * pb_Metadata stores information about how an asset should be stored and reconstructed.
	 */
	[System.Serializable()]
	public class pb_MetaData : ISerializable
	{
		
		public const string GUID_NOT_FOUND = "MetaData_NoGUIDPresent";
		public const string ASSET_BUNDLE = "MetaData_BundleAsset";
		public const string ASSET_INSTANCE = "MetaData_InstanceAsset";

		// Tags can be used to organize objects in the resource browser.
		public string[] tags = new string[0];

		// If target is a prefab, this stores a diff of component values.
		public pb_ComponentDiff componentDiff;
		
		// Get and set active state without showing it in inspector
		public bool GetActive() => _active;
		public void SetActive(bool active) => _active = active;
		
		/**
		 * Stores the unique ID of the object 
		 */
		public string objectId => _objectId;
		
		/**
		 * The file id that can be used to look up this object (if this is a prefab stored in Resources folder).
		 */
		public string fileId => _fileId;

		/**
		 * Return the asset bundle information (if assetType == AssetType.Bundle) - if not a bundle
		 * type this value will be junk.
		 */
		public pb_AssetBundlePath assetBundlePath => _assetBundlePath;

		/**
		 * Return the type of asset pointed to.
		 */
		public AssetType assetType => _assetType;

		/**
		 * Return the type of instance for this asset
		 */
		public InstanceType instanceType => _instanceType;

		/**
		 * Return the type of particle effect for this asset
		 */
		public EffectType effectType
		{
			get { return _effectType; }
			set { _effectType = value; }
		}

		/**
		 * Indicates if this object is an asset bundle parent
		 */
		public bool bundleParent => _bundleParent;
		
		/**
		 * Returns true if this object is a duplicate of another object
		 */
		public bool duplicate => _duplicate;

		/**
		 * If this object is a duplicate, this stores the 'objectId' of the object it originated from
		 */
		public string originId => _originId;

		/**
		 * Public reference to th eparticle parameters stored in this meta
		 */
		public string particleParameters => _particleParameters;

		/**
		 * Indicates whether the associated gameObject was stored in an active state or not
		 */
		[HideInInspector] private bool _active = true;
		[HideInInspector] private string _particleParameters = "";

		[SerializeField] private string _objectId = GUID_NOT_FOUND;
		[SerializeField] private string _fileId = GUID_NOT_FOUND;
		[SerializeField] private pb_AssetBundlePath _assetBundlePath;
		[SerializeField] private AssetType _assetType = AssetType.Instance;
		[SerializeField] private bool _bundleParent = false;
		[SerializeField] private bool _duplicate = false;
		[SerializeField] private string _originId = "";
		[SerializeField] private InstanceType _instanceType;
		[SerializeField] private EffectType _effectType = EffectType.None;

		private ParticleController controller;

		/**
		 * Basic constructor (used on instance assets).
		 */
		public pb_MetaData()
		{
			_assetType = AssetType.Instance;
			_fileId = GUID_NOT_FOUND;
			_assetBundlePath = null;
			componentDiff = new pb_ComponentDiff();
		}
		
		/**
		 * Serialization override.
		 */
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_objectId", _objectId, typeof(string));
			info.AddValue("_fileId", _fileId, typeof(string));
			info.AddValue("_assetBundlePath", _assetBundlePath, typeof(pb_AssetBundlePath));
			info.AddValue("_assetType", _assetType, typeof(AssetType));
			info.AddValue("_bundleParent", bundleParent, typeof(bool));
			info.AddValue("componentDiff", componentDiff, typeof(pb_ComponentDiff));
			info.AddValue("_active", _active, typeof(bool));
			info.AddValue("_duplicate", duplicate, typeof(bool));
			info.AddValue("_originId", originId, typeof(string));
			info.AddValue("_effectType", _effectType, typeof(EffectType));

			if (_instanceType == InstanceType.Particles)
			{
				string parameters = controller.GetSerializedData();
				info.AddValue("_particleParameters", parameters, typeof(string));
			}

			if (_assetType == AssetType.Instance)
			{
				info.AddValue("_instanceType", _instanceType, typeof(InstanceType));
			}
		}

		/**
		 * Serialized constructor.
		 */
		public pb_MetaData(SerializationInfo info, StreamingContext context)
		{
			_objectId = (string)info.GetValue("_objectId", typeof(string));
			_fileId = (string)info.GetValue("_fileId", typeof(string));
			_assetBundlePath = (pb_AssetBundlePath)info.GetValue("_assetBundlePath", typeof(pb_AssetBundlePath));
			_assetType = (AssetType)info.GetValue("_assetType", typeof(AssetType));
			_bundleParent = (bool)info.GetValue("_bundleParent", typeof(bool));
			componentDiff = (pb_ComponentDiff)info.GetValue("componentDiff", typeof(pb_ComponentDiff));
			_active = (bool)info.GetValue("_active", typeof(bool));
			_duplicate = (bool)info.GetValue("_duplicate", typeof(bool));
			_originId = (string)info.GetValue("_originId", typeof(string));
			//_effectType = (EffectType)info.GetValue("_effectType", typeof(EffectType));

			if (_assetType == AssetType.Instance)
			{
				_instanceType = (InstanceType)info.GetValue("_instanceType", typeof(InstanceType));
			}

			if (_instanceType == InstanceType.Particles)
			{
				_particleParameters = (string)info.GetValue("_particleParameters", typeof(string));
			}
		}

		/**
		 * Set the asset bundle information.
		 */
		public void SetAssetBundleData(string bundleName, string assetPath, bool parent = false)
		{
			SetObjectId();
			_fileId = ASSET_BUNDLE;
			_assetType = AssetType.Bundle;
			_assetBundlePath = new pb_AssetBundlePath(bundleName, assetPath);
			_bundleParent = parent;
		}

		/**
		 * Set the fileID field if this asset is in the resources folder.
		 */
		public void SetFileId(string id)
		{
			SetObjectId();
			_assetType = AssetType.Resource;
			_assetBundlePath = null;
			_fileId = id;
		}

		public void SetDuplicate(string originRef)
		{
			_bundleParent = false;
			_originId = originRef;
			_duplicate = true;
			SetObjectId();
		}
		
		public void SetObjectId() => _objectId = SetNewID();

		public string SetNewID()
		{
			if (_objectId != GUID_NOT_FOUND) return _objectId;
			//const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
			//string newID = string.Empty;
			//int charAmount = Random.Range(10, 25);

			//for (int i = 0; i < charAmount; i++)
			//{
			//	newID += glyphs[Random.Range(0, glyphs.Length)];
			//}

			//return newID;
			return System.Guid.NewGuid().ToString();
		}

		public void OverrideID()
		{
			//const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
			//string newID = string.Empty;
			//int charAmount = Random.Range(10, 25);

			//for (int i = 0; i < charAmount; i++)
			//{
			//	newID += glyphs[Random.Range(0, glyphs.Length)];
			//}
			//_objectId = newID;
			_objectId = System.Guid.NewGuid().ToString();
		}

		public void SetParticleController(ParticleController controller, EffectType type)
		{
			this.controller = controller;
			_effectType = type;
		}
	}
}
