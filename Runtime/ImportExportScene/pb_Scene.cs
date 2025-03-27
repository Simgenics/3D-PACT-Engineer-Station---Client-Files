using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImportExportScene.Serialization;
using LMS.API;
using LMS.Core;
using Newtonsoft.Json;
using SharedAssets;
using UnityEngine;
namespace ImportExportScene
{
	[pb_JsonIgnore]
	[Serializable]
	[RequireComponent(typeof(pb_MetaDataComponent))]
	public class pb_Scene : pb_MonoBehaviourSingleton<pb_Scene>
    {

		/**
		 * Event raised when a level is loaded.
		 */
		public static Action OnLevelLoaded = delegate { };

		/**
		 * Event raised when a level cleared.
		 */
		public static Action OnLevelCleared = delegate { };

		/**
		 * A queue containing the scene nodes of asset bundles that still need to be loaded
		 */
		private static readonly Queue<pb_SceneNode> nodesToLoad = new Queue<pb_SceneNode>();
		
		/**
		 * A reference to the current node that is being processed
		 */
		private static pb_SceneNode currentNode;
		
	    /**
	     * A hash set that stores the loaded <see cref="pb_MetaDataComponent"/>s for easy retrieval later
	     */
		public static Dictionary<string, pb_MetaDataComponent> currentComponents = new ();
	    
	    /**
	     * Indicates if the scene contains child <see cref="pb_MetaDataComponent"/> assets
	     * If the count is one we only have the root Scene Metadata.
	     */
	    public static bool HasAssets => currentComponents.Count > 1;
	    
	    /**
	     * Stores the current scene's config
	     */
	    public static SceneConfig currentConfig;
	    
	    /**
	     * Indicates if the pb_Scene is currently busy loading a SceneConfig
	     */
	    public static bool LoadingSceneConfig = false;

        /**
	     * A single check to see if assets are loaded and setup
	     */
        public static bool AssetsReady => HasAssets && !LoadingSceneConfig;
	    
		/**
		 * This will be set to true by subsystems in Update().
		 * Hotkeys should be tested in late update.
		 * At the end of the frame it will be reseted to false.
		 */
		public static bool BlockHotkeys { get; set; }

		[SerializeField] private Collection<pb_MetaData> collection;

        private void Start()
        {
			StartCoroutine(EndOfFrame());
        }

        /**
		 * Load a saved level into the scene.  This clears the currently open scene.
		 */
        public static void LoadLevel(string levelJson)
		{
			if (nullableInstance != null)
				instance.Clear();
			
			LoadingSceneConfig = true;
			
			// Deserialize
			var rootNode = JsonConvert.DeserializeObject<pb_SceneNode>(levelJson, pb_Serialization.ConverterSettings);
			if (rootNode != null)
			{
				LoadMultipleNodes(rootNode);
			}
			else
			{
				Debug.LogWarning("Could not deserialize root node.");
			}
		}

		/**
		 * Load a scene from a SceneConfig JSON string
		 */
		public static void LoadSceneConfig(string levelJson)
		{
			var sceneConfig = JsonConvert.DeserializeObject<SceneConfig>(levelJson, pb_Serialization.ConverterSettings);
			if (sceneConfig != null)
			{
				if (HasAssets)
				{
					// TODO: Implement method to fetch local asset bundles
					LoadSceneConfig(sceneConfig);
				}
				else
				{
					LoadInitialCondition(sceneConfig);
					AssetLoader.OnObjectCreatedFromBundle += HandleAssetLoaded;
				}
			}
			else
			{
				Debug.LogWarning("Could not deserialize scene config.");
			}
		}
		
		/**
		 * Load a scene using a <see cref="SceneConfig"/>
		 */
		public static void LoadSceneConfig(SceneConfig config)
		{
			Debug.Log("Loading scene config.");
			if (config != null)
			{
				currentConfig = config;
				LoadMultipleNodes(config.initialCondition);
			}
		}

		/**
		 * Load a scene using a <see cref="SceneConfig"/> without auto starting the loading process
		 */
		public static void LoadInitialCondition(SceneConfig config)
		{
			if (config != null)
			{
				Debug.Log("Loading initial condition without autoStart...");
				currentConfig = config;
				LoadMultipleNodes(config.initialCondition, false);
			}
		}
		
		/**
		 * Create a queue to load asset bundles in a sequence. This ensures that an asset bundle gets
		 * unloaded before it gets reused in the future
		 */
		private static void LoadMultipleNodes(pb_SceneNode node, bool autoStart = true)
		{			
			foreach (var pbSceneNode in node.children)
			{
				nodesToLoad.Enqueue(pbSceneNode);
			}

			if (nodesToLoad.Count > 0)
			{
				// Load the first node
				currentNode = nodesToLoad.Dequeue();
			}
			
			if (autoStart)
			{
				Debug.Log($"Loading level. {nodesToLoad.Count} nodes detected.");
				LoadAssetByType(currentNode);
			}
		}
		
		/**
		 * Load a level using a deserialized <see cref="pb_SceneNode"/>
		 */
		public static void LoadLevelNode(pb_SceneNode node)
		{
			currentNode = node;
			LoadAssetByType(node);
			Debug.Log($"Loading level. {nodesToLoad.Count} nodes detected.");
		}
		
		/**
		 * Save the current level. Returns a JSON formatted string with the entire scene-graph serialized.
		 */
		public static string SaveSceneConfigString()
		{
			var config = SaveSceneConfig();
			return JsonConvert.SerializeObject(config, pb_Serialization.ConverterSettings);
		}

		public static SceneConfig SaveSceneConfig()
		{
			var config = GetCurrentConfig();
			if (config != null)
			{
				config.id = Guid.NewGuid();
				return config;
			}
			return null;
		}
		
		/**
		 * Get the current active <see cref="SceneConfig"/>, or creates one if none exists
		 */
		public static SceneConfig GetCurrentConfig()
		{
#if INSTRUCTOR_STATION
			// Ensure deleted components get serialized and clean up after
			pb_SceneDeleted.Instance.ReturnToSource();
			var rootNode = new pb_SceneNode(instance.gameObject);
			pb_SceneDeleted.Instance.ReapplyDeleted();
#else
			var rootNode = new pb_SceneNode(instance.gameObject);
#endif
			
			if (currentConfig == null)
			{
				try
				{
					var sceneConfig = new SceneConfig
					{
						id = Guid.NewGuid(),
						initialCondition = rootNode,
					};
					currentConfig = sceneConfig;
					return sceneConfig;
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return null;
				}
			}
			
			// Overwrite and return existing scene
			currentConfig.initialCondition = rootNode;
			return currentConfig;
		}
		
		/**
		 * Retrieves the cached <see cref="pb_MetaDataComponent"/> that has the specified objectId
		 */
		public static pb_MetaDataComponent GetObject(string objectID)
		{
			if (!string.IsNullOrEmpty(objectID) && currentComponents.ContainsKey(objectID))
			{
				return currentComponents[objectID];
			}
			//Debug.LogWarning($"Could not find pb_MetaDataComponent with objectId {objectID}");
			return null;
		}

		/**
		 * Retrieves the cached <see cref="pb_MetaDataComponent"/> that has the specified objectId,
		 * and returns a component from it.
		 */
		public static T GetObject<T>(string objectID) where T : class
		{
			pb_MetaDataComponent component = GetObject(objectID);
			if (component != null)
			{
				return component.GetComponent<T>();
			}
			return null;
		}

		public static GameObject GetGameObject(string objectID)
		{
			Transform component = GetObject<Transform>(objectID);
			if (component != null)
			{
				return component.gameObject;
			}
			return null;
		}

		/**
		 * Destroy all children in the scene.
		 */
		public void Clear()
		{
			foreach (Transform t in transform)
				pb_ObjectUtility.Destroy(t.gameObject);

			currentConfig = null;
			currentComponents.Clear();
			OnLevelCleared.Invoke();
		}

		public static pb_MetaDataComponent SpawnObject(pb_MetaDataComponent toSpaw, GameObject replacement)
		{
			pb_MetaDataComponent replacementMeta = replacement.GetComponent<pb_MetaDataComponent>();
			if (replacementMeta == null)
            {
				replacementMeta = replacement.AddComponent<pb_MetaDataComponent>();
			}

			replacementMeta.metadata = toSpaw.metadata;
			toSpaw.DestroyObject();
			return replacementMeta;
		}

		/**
		 * Handler for when a bundle has finished loading and instantiating.
		 * Load additional asset bundles if the queue is not empty
		 */
		public static void HandleAssetLoaded(GameObject obj)
		{
#if ASSET_LOADER
			// Setup bundle game object
			SetupInstantiatedBundle(obj);
			Debug.Log("Instantiated bundle setup done");
#endif
			LoadNextNode();
		}
		
		/**
		 * Load more nodes if there are any
		 */
		public static void LoadNextNode()
		{
			if (nodesToLoad.Count > 0)
			{
				currentNode = nodesToLoad.Dequeue();
				LoadAssetByType(currentNode);
			}
			else
			{
				Debug.Log("All nodes loaded!");
			}
		}

		/**
		 * This function loads a scene node by the metadata assetType.
		 */
		private static void LoadAssetByType(pb_SceneNode rootChild) 
		{
			switch (rootChild.metadata.assetType)
            {
	            case AssetType.Bundle:
		            Debug.LogWarning("pb_Scene does not load Asset Bundles. Please use AssetLoader");
		            break;
	            case AssetType.Instance:
		            LoadAssetInstance(rootChild);
		            break;
	            default:
	            case AssetType.Resource:
		            Debug.LogWarning("Asset Type not supported. Please use asset bundles");
		            break;
            }
        }
		
		/**
		 * This function loads a scene node by the metadata instanceType.
		 */
		private static void LoadAssetInstance(pb_SceneNode rootChild)
		{
			Debug.Log("Instance type found : " + rootChild.metadata.instanceType);

			switch (rootChild.metadata.instanceType)
			{
				case InstanceType.Particles:
				case InstanceType.Waypoint:
				case InstanceType.PathNode:
				case InstanceType.NPCSpawnLocation:
				case InstanceType.PlayerSpawnLocation:
					CreateInstanceObject(rootChild.metadata, rootChild);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		/**
		 * Creates an instantiated object according to the metadata and set its previous settings
		 */
		private static void CreateInstanceObject(pb_MetaData meta, pb_SceneNode storedNode)
		{
			var obj = instance.collection.GetMatchingObject(meta);
			if (obj != null)
			{
				var createdObj = Instantiate(obj, instance.transform);
				var newMeta = createdObj.GetComponent<pb_MetaDataComponent>();
				SetupTransforms(newMeta, storedNode);
				SetupParticles(newMeta);
				SetupObjectComponents(new Dictionary<pb_MetaDataComponent, pb_SceneNode>() { { newMeta, storedNode } });
				LoadNextNode();
			}
		}

		/**
		 * An asset bundle has been loaded, and its parent gameObject instantiated.
		 * Setup the objects child nodes.
		 */
		public static void SetupInstantiatedBundle(GameObject obj)
		{
			// Get spawned metadata
			var spawnedNode = obj.GetComponent<pb_MetaDataComponent>();
			var isDuplicate = spawnedNode.metadata.duplicate;
			
			try
			{
				// Get stored children nodes and remove parent node
				var storedNodes = currentNode.children;
				storedNodes.Insert(0, currentNode);
				
				// Get new node metadata
				var newChildren = obj.GetComponentsInChildren<pb_MetaDataComponent>(true).ToList();
				
				// Setup new nodes
				SetupSpawnedChildren(storedNodes, newChildren, isDuplicate);

				// Evoke loaded event
				OnLevelLoaded.Invoke();
				LoadingSceneConfig = false;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
		
		/**
		 * This function sets up a newly instantiated group of gameObjects <see cref="newChildren"/> and loads the components states
		 * according to the group of <see cref="storedChildren"/> loaded from JSON
		 */
		private static void SetupSpawnedChildren(List<pb_SceneNode> storedChildren, List<pb_MetaDataComponent> newChildren, bool isDuplicate)
		{
			// Store a temp list of stored duplicates, and gameObject which still needs components setup
			var duplicates = new List<pb_SceneNode>();
			var initGameObjects = new Dictionary<pb_MetaDataComponent, pb_SceneNode>();

			// Crucial prerequisite: Set all the newly instantiated metadata to the stored object's metadata
			foreach (var child in newChildren)
			{
				var match = storedChildren.Find(storedChild =>
				{
					var storedDup = storedChild.metadata.duplicate;
					var storedObj = storedDup ? storedChild.metadata.originId : storedChild.metadata.objectId;
					return storedObj == child.metadata.objectId;
				});
				
				if (match != null)
				{
					child.metadata = match.metadata;
				}
			}
			
			// 1. Setup transforms/metadata of asset bundle gameObjects
			SetupBundleTransforms(storedChildren, newChildren, duplicates, initGameObjects, isDuplicate);

			// 2. Setup transforms/metadata of duplicated gameObjects
			if (duplicates.Count > 0)
			{
				SetupDuplicateObjects(newChildren, duplicates, initGameObjects);
			}
			
			// 3. Setup component data
			if (initGameObjects.Count > 0)
			{
				SetupObjectComponents(initGameObjects);
			}
		}
		
		/**
		 * Iterates through the stored sceneNodes to match them up with their newly instantiated counterparts. Once the match
		 * has been found, the new gameObject gets updated with the stored data.
		 * Duplicates are extracted and cloned from existing gameObjects after all the asset bundle assets have been setup.
		 */
		private static void SetupBundleTransforms(
			List<pb_SceneNode> storedChildren, 
			IReadOnlyCollection<pb_MetaDataComponent> newChildren, 
			ICollection<pb_SceneNode> duplicates, 
			IDictionary<pb_MetaDataComponent, pb_SceneNode> initGameObjects,
			bool isDuplicate
		) {
			// For each stored gameObject
			foreach (var storedChild in storedChildren)
			{
				// Find the newly instantiated object which has the same ID as the stored object
				var newMatch = newChildren
					.Where(child => {
						var childId = isDuplicate ? storedChild.metadata.originId : storedChild.metadata.objectId;
						return child.metadata.objectId == childId;
					})
					.ToList();

				// Process matches based on amount found
				var matches = newMatch.Count();
				switch (matches)
				{
					case 1:
						// Single match found - proceed to setup gameObject
						var match = newMatch.First();
						SetupTransforms(match, storedChild);
						var added = initGameObjects.TryAdd(match, storedChild);
						if (!added) Debug.LogWarning("Could not add new game-object to dictionary because it already exists: " + match.name);
						break;
					case <= 0:
						// No matches found - possible duplicate
						var dupe = storedChild.metadata.duplicate;
						if (dupe)
						{
							// Store to setup after asset bundle gameObjects
							duplicates.Add(storedChild);
							continue;
						}
						Debug.LogWarning("No match found for stored child: " + storedChild.name);
						break;
					case > 1:
						// More than one match found - log error
						Debug.LogError($"!! Multiple matches found of child {storedChild.name} - this is not supposed to happen !!");
						break;
				}
			}
		}
		
		/**
		 * Sets up an instantiated gameObjects' components according to the stored scene nodes
		 */
		private static void SetupObjectComponents(Dictionary<pb_MetaDataComponent, pb_SceneNode> initGameObjects)
		{
			foreach (var (initMeta, storedNode) in initGameObjects)
			{
				// Load the rest of the component data
				var originComponents = storedNode.components;
				if (originComponents == null)
				{
					Debug.LogWarning("No originComponents");
				}
				else
				{
					var hasComponents = originComponents.Count > 0; 
					if (hasComponents)
					{
						Component[] existingComponents = initMeta.gameObject.GetComponents<Component>();
						foreach (var comp in originComponents)
						{
							var type = comp.type;
							if (type != null)
							{
								initMeta.gameObject.AddComponent(existingComponents, comp);
							}
						}
					}
				}
				initMeta.metadata = storedNode.metadata;
			}
		}

		/**
		 * Set up the identified duplicate objects by cloning their referenced originId
		 */
		private static void SetupDuplicateObjects(List<pb_MetaDataComponent> newChildren, List<pb_SceneNode> duplicates, Dictionary<pb_MetaDataComponent, pb_SceneNode> initGameObjects)
		{
			foreach (var duplicate in duplicates)
			{
				try
				{
					// Find instantiated object that has the duplicate's originId
					var newMatch = newChildren.First(child => child.metadata.objectId == duplicate.metadata.originId);

					// Clone gameObject
					var duplicatedMeta = Instantiate(newMatch.gameObject, newMatch.transform.parent).GetComponent<pb_MetaDataComponent>();
					
					// Setup cloned gameObject
					if (duplicatedMeta != null)
					{
						SetupTransforms(duplicatedMeta, duplicate);
						initGameObjects.Add(duplicatedMeta, duplicate);
					}
				}
				catch (InvalidOperationException e)
				{
					Debug.LogError("Could not find match: " + e);
				}
			}
		}

		/**
		 * Set up a gameObjects transform data, active state, and metadata
		 */
		private static void SetupTransforms(pb_MetaDataComponent match, pb_SceneNode storedChild)
		{
			// Set transform data
			match.transform.SetTRS(storedChild.transform);

			// Set object active state
			match.gameObject.SetActive(storedChild.metadata.GetActive());
			
			// Apply metadata
			match.metadata = storedChild.metadata;
		}

		private static void SetupParticles(pb_MetaDataComponent match)
		{
			var controller = match.transform.GetComponent<ParticleController>();
			if (controller != null)
			{
				controller.DeserializeData(match);
			}
		}

		private IEnumerator EndOfFrame()
		{
            while (true)
            {
				yield return new WaitForEndOfFrame();
				BlockHotkeys = false;
            }
		}
    }
}