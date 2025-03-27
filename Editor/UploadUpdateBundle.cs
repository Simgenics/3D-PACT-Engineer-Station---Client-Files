#if UNITY_EDITOR
using ImportExportScene;
using LMS.API;
using SharedAssets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThreeDPactXR.Licensing;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UploadUpdateBundle : SolitaryWindow
{
	private Texture2D backButtonTexture;
	private GUIStyle backButtonStyle = null;

	private Texture2D updateButtonTexture;
	private GUIStyle updateButtonStyle = null;

	private Texture2D textFieldTexture;
	private GUIStyle textFieldStyle = null;

	private Texture2D labelTexture;
	private GUIStyle labelStyle = null;

	private Texture2D profileButtonTexture;
	private GUIStyle profileButtonStyle;

	private string selectedBundleStatus;
	private string bundleName;
	private string userInitial;
	private GameObject startupObject;
	private bool uploading;

	private void OnGUI()
	{
		if (backButtonTexture == null)
		{
			backButtonTexture = Resources.Load<Texture2D>("Sprites/TransparentButton");
			backButtonStyle = new GUIStyle(GUI.skin.button);
			backButtonStyle.normal.background = backButtonTexture;

			updateButtonTexture = Resources.Load<Texture2D>("Sprites/SelectedButton");
			updateButtonStyle = new GUIStyle(GUI.skin.button);
			updateButtonStyle.normal.background = updateButtonTexture;

			textFieldTexture = Resources.Load<Texture2D>("Sprites/TextField");
			textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.alignment = TextAnchor.MiddleLeft;
			textFieldStyle.padding = new RectOffset(10, 0, 0, 0);
			textFieldStyle.fontStyle = FontStyle.Bold;
			textFieldStyle.normal.textColor = Color.white;
			textFieldStyle.normal.background = textFieldTexture;

			labelTexture = Resources.Load<Texture2D>("Sprites/NormalButton");
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.normal.background = labelTexture;
			labelStyle.normal.textColor = Color.white;
			labelStyle.padding = new RectOffset(10, 0, 0, 0);

			profileButtonTexture = Resources.Load<Texture2D>("Sprites/ProfileButton");
			profileButtonStyle = new GUIStyle(GUI.skin.button);
			profileButtonStyle.alignment = TextAnchor.MiddleCenter;
			profileButtonStyle.fontSize = 12;
			profileButtonStyle.normal.background = profileButtonTexture;

			userInitial = EditorPrefs.GetString("UserInitial", "J");
			selectedBundleStatus = EditorPrefs.GetString("SelectedBundleStatus", "");
			bundleName = EditorPrefs.GetString("BUNDLE_NAME");

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}
		GUIStyle headStyle = new GUIStyle();
		using (var scope = new GUILayout.AreaScope(new Rect(405, 10, 50, 50)))
		{
			if (GUILayout.Button(userInitial, profileButtonStyle, GUILayout.Width(35), GUILayout.Height(30)))
			{

			}
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 20, 455, 285)))
		{
			EditorGUILayout.LabelField("3D PACT Engineer Station");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("<", GUILayout.Width(15), GUILayout.Height(15));
			GUILayout.Button("Export/Update", backButtonStyle, GUILayout.Width(100), GUILayout.Height(20));
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20);
			bundleName = GUILayout.TextField(bundleName, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("State", GUILayout.Width(40), GUILayout.Height(15));
			headStyle.fontSize = 17;
			headStyle.normal.textColor = Color.white;
			GUILayout.Label(selectedBundleStatus, labelStyle, GUILayout.Width(350), GUILayout.Height(30));
			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.Space(250);
			if (GUILayout.Button("Update", updateButtonStyle, GUILayout.Width(100), GUILayout.Height(20)))
			{
				CheckedOutLicense license = RLMManager.Instance.CheckoutLicense("3dpxr_Engineer", "0.1", 1);
				int licStatus = RLMManager.GetLicenseStatus(license);
				if (licStatus != 0)
				{
					if (LicensingWindow.IsOpen)
					{
						LicensingWindow.ShowWindow();
					}
					Debug.Log("License checkout failed, confirm license settings in 3DPactVR->Licensing");
					this.Close();
				}
				else if(!uploading)
				{
					uploading = true;
					EditorPrefs.SetString("BUNDLE_NAME", bundleName);
					EditorPrefs.SetBool("UpdatingBundle", true);
					var token = EditorPrefs.GetString("LMSToken", "");
					AssetLoader.instance.SetAuthToken(token);
					AddMetaDataComponents();
					if (EditorPrefs.GetString("SelectedBundleStatus") == "Locked")
						BuildBundle();
					else
						BuildPackage();
				}
			}
			GUILayout.EndHorizontal();
		}
	}

	void AddMetaDataComponents()
	{
		var objects = EditorSceneManager.GetActiveScene().GetRootGameObjects(); ;
		foreach (var obj in objects)
		{
			pb_MetaDataComponent md = obj.GetComponent<pb_MetaDataComponent>();
			if (md == null)
				md = obj.AddComponent<pb_MetaDataComponent>();
			if (md)
			{
				md.metadata.SetAssetBundleData("", $"");

				// Set children file path data
				if (obj.transform.parent != null)
				{
					md.metadata.assetBundlePath.filePath = $"{obj.transform.parent.name}Model[{obj.transform.name}]";
				}
				md.Activate();
			}
		}
	}

	private static AddressableAssetSettings settings;
	private static string FOLDER_NAME = "/BundleFiles";
	private static string PACKAGE_FOLDER_NAME = "/PackageFiles";
	void BuildBundle()
	{
		Directory.CreateDirectory(Application.streamingAssetsPath + FOLDER_NAME);
		Scene openedScene = SceneManager.GetActiveScene();
		
		settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
		settings.buildSettings.bundleBuildPath = Application.streamingAssetsPath + FOLDER_NAME;
		foreach (var g in settings.groups)
		{
			PlayerDataGroupSchema playerDataGroupSchema = g.GetSchema<PlayerDataGroupSchema>();
			if (playerDataGroupSchema != null)
			{
				playerDataGroupSchema.IncludeBuildSettingsScenes = false;
				playerDataGroupSchema.IncludeResourcesFolders = false;
			}
			BundledAssetGroupSchema bundledAssetGroupSchema = g.GetSchema<BundledAssetGroupSchema>();
			if (bundledAssetGroupSchema != null && bundledAssetGroupSchema.IncludeInBuild != false)
			{
				bundledAssetGroupSchema.IncludeInBuild = false;
				EditorUtility.SetDirty(g);
			}
		}

		var group = settings.CreateGroup(bundleName, false, false, true, null, typeof(ContentUpdateGroupSchema));
		group.Settings.profileSettings.SetValue(group.Settings.activeProfileId, "Local.BuildPath", Application.streamingAssetsPath + FOLDER_NAME);
		group.Settings.profileSettings.SetValue(group.Settings.activeProfileId, "Local.LoadPath", Application.streamingAssetsPath + FOLDER_NAME);

		var guid = AssetDatabase.AssetPathToGUID(openedScene.path);

		var e = settings.CreateOrMoveEntry(guid, group, false, false);
		var entriesAdded = new List<AddressableAssetEntry> { e };

		group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, entriesAdded, false, true);
		settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, entriesAdded, true, false);

		BundledAssetGroupSchema b = group.AddSchema<BundledAssetGroupSchema>();

		b.IncludeInBuild = true;
		b.BuildPath.SetVariableByName(settings, "Local.BuildPath");
		b.LoadPath.SetVariableByName(settings, "Local.LoadPath");

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		AddressableAssetSettings
			.BuildPlayerContent(out AddressablesPlayerBuildResult result);
		bool success = string.IsNullOrEmpty(result.Error);

		if (!success)
		{
			Debug.Log($"<color={"#e0370d"}>{"Addressables build error encountered: " + result.Error}</color>\n");
			return;
		}
		EditorPrefs.SetString("BUNDLE_PATH_NAME", Path.GetFileName(result.AssetBundleBuildResults[1].FilePath));
		Debug.Log(result.AssetBundleBuildResults[1].FilePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		SpawnStartupPrefab(false);
		EditorApplication.isPlaying = true;
	}

	void BuildPackage()
	{
		Scene openedScene = SceneManager.GetActiveScene();
		string newPath = openedScene.path.Replace(openedScene.name, bundleName);
		//EditorSceneManager.SaveScene(openedScene, newPath, true);
		Directory.CreateDirectory(Application.streamingAssetsPath + PACKAGE_FOLDER_NAME);
		var path = Application.streamingAssetsPath + PACKAGE_FOLDER_NAME;

		var exportedPackageAssetList = new List<string>();
		exportedPackageAssetList.Add(openedScene.path);

		AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), path + "/" + bundleName + ".unitypackage",
			ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		SpawnStartupPrefab(false);
		EditorApplication.isPlaying = true;
	}

	static string prefabName = "StartUp";
	void SpawnStartupPrefab(bool hide)
	{
		this.minSize = new Vector2(0, 0);
		this.maxSize = new Vector2(0, 0);
		bool hasStartup = false;
		var objects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (var obj in objects)
		{
			if (obj.name == prefabName)
			{
				hasStartup = true;
				break;
			}
		}
		if (hasStartup) return;
		if (startupObject == null)
		{
			var obj = Resources.Load(prefabName);
			startupObject = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
			startupObject.name = prefabName;
		}
	}

	private void Update()
	{
		EditorApplication.QueuePlayerLoopUpdate();
		SceneView.RepaintAll();
		if (StartUp.IsUploading) return;
		EditorApplication.isPlaying = false;
	}

	private void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredEditMode)
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			var objects = SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (var obj in objects)
			{
				if (obj.name == prefabName)
				{
					DestroyImmediate(obj);
					break;
				}
			}
			Close();
		}
	}

	private void OnDestroy()
	{
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		var objects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (var obj in objects)
		{
			if (obj.name == prefabName)
			{
				DestroyImmediate(obj);
				break;
			}
		}
	}
}
#endif