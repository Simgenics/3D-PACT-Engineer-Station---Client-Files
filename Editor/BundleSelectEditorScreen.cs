#if UNITY_EDITOR
using LMS.API;
using LMS.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Build;
using System.IO;

public class BundleSelectEditorScreen : SolitaryWindow
{
	bool gotCompanies;
	bool gettingCompanies;

	bool gotFacilities;
	bool gettingFacilities;

	bool gotBundles;
	bool gettingBundles;

	bool lockBundle;

	private List<CompanyDto> listCompanies = new List<CompanyDto>();
	private List<FacilityDto> listFacilities = new List<FacilityDto>();
	private List<GetAssetBundleForViewDto> listAssets = new List<GetAssetBundleForViewDto>();

	private FacilityDto selectedFacility;
	private AssetBundleDto selectedBundle;

	private Vector2 companyScrollPos;
	private Vector2 facilityScrollPos;
	private Vector2 assetScrollPos;

	private void OnGUI()
	{
		if (!gotCompanies)
		{
			gotCompanies = true;
			gettingCompanies = true;
			GetCompanies();
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(25, 10, 300, 350)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(75);
			GUILayout.Label("Select a Company");
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (gotCompanies && !gettingCompanies)
			{
				companyScrollPos = EditorGUILayout.BeginScrollView(companyScrollPos, false, true);
				foreach (var company in listCompanies)
				{
					if (GUILayout.Button(company.name, GUILayout.Width(250), GUILayout.Height(40)))
					{
						CompanySelected(company.id);
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(350, 10, 300, 350)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(75);
			GUILayout.Label("Select a Facility");
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (gotFacilities && !gettingFacilities)
			{
				facilityScrollPos = EditorGUILayout.BeginScrollView(facilityScrollPos, false, true);
				foreach (var facility in listFacilities)
				{
					if (GUILayout.Button(facility.name, GUILayout.Width(250), GUILayout.Height(40)))
					{
						FacilitySelected(facility);
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(675, 10, 300, 350)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(75);
			GUILayout.Label("Select a Bundle");
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (gotBundles && !gettingBundles)
			{
				assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos, false, true);
				foreach (var bundle in listAssets)
				{
					if (GUILayout.Button(bundle.assetBundle.name, GUILayout.Width(250), GUILayout.Height(40)))
					{
						BundleSelected(bundle.assetBundle);
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(750, 400, 200, 30)))
		{
			if (selectedBundle != null)
			{
				if (GUILayout.Button("LOAD", GUILayout.Width(200), GUILayout.Height(30)))
				{
					LoadBundle();
				}
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(500, 400, 250, 30)))
		{
			if (GUILayout.Button("BUILD AND UPLOAD", GUILayout.Width(200), GUILayout.Height(30)))
			{
				Build();
			}
		}
	}

	public void GetCompanies()
	{
		EditorCoroutine getCompaniesRoutine = EditorCoroutine.Start(CompaniesAdapter.instance.GetFacilitiesForUser(companiesResult =>
		{
			if (companiesResult != null)
			{
				listCompanies = companiesResult.items;
				gettingCompanies = false;
			}
		}));
	}

	void CompanySelected(int id)
	{
		if (!gettingFacilities)
		{
			gotFacilities = true;
			gettingFacilities = true;
			GetFacilities(id);
		}
	}

	void GetFacilities(int companyID)
	{
		var query = new FacilityQuery() { OrganizationalUnit = companyID };
		EditorCoroutine getfacilitiesRoutine = EditorCoroutine.Start(FacilitiesAdapter.instance.GetAllFacilities(result =>
		{
			listFacilities = result.items.Where(i => i.companyId == companyID).ToList();
			gettingFacilities = false;
		}, new FacilityQuery()));
	}

	void FacilitySelected(FacilityDto dto)
	{
		if (!gettingBundles)
		{
			gotBundles = true;
			gettingBundles = true;
			selectedFacility = dto;
			PlayerPrefs.SetInt("selectedFacilityID", selectedFacility.id);
			GetBundles(dto.id);
		}
	}

	void GetBundles(int facilityID)
	{
		EditorCoroutine getfacilitiesRoutine = EditorCoroutine.Start(AssetBundleAdapter.instance.GetAllAssetBundles((result) =>
		{
			foreach (var item in result.items)
			{
				if (item.assetBundle.status == StatusState.Unlocked)
					listAssets.Add(item);
			}
			gettingBundles = false;
		}, new AssetBundleQuery() { FacilityId = facilityID, MaxResultCount = int.MaxValue }));
	}

	void BundleSelected(AssetBundleDto dto)
	{
		selectedBundle = dto;
	}

	#region LOAD
	List<string> allScenes = new List<string>();
	void LoadBundle()
	{
		allScenes = Directory.GetFiles(Application.dataPath + "/Scenes").Where(f => !f.Contains(".meta")).ToList();
		var fileQuery = new FileQuery() { container = "assets", file = selectedBundle.assetPath };
		FileAdapter.instance.DownloadFile(result =>
		{
			string path = Application.streamingAssetsPath + "/Loading/";
			Directory.CreateDirectory(path);
			File.WriteAllBytes(path + selectedBundle.assetPath, result);
			AssetDatabase.Refresh();
			AssetDatabase.importPackageCompleted += ImportPackageComplete;
			AssetDatabase.ImportPackage(path + selectedBundle.assetPath, false);
		}, fileQuery, LMSAssetType.UnityPackage);
	}

	void ImportPackageComplete(string packageName)
	{
		Debug.Log("package name imported = " + packageName);
		string path = Application.streamingAssetsPath + "/Loading/" + selectedBundle.name;
		if (File.Exists(path))
			File.Delete(path);
		string[] files = Directory.GetFiles(Application.dataPath + "/Scenes").Where(f => !f.Contains(".meta")).ToArray();
		foreach (var file in files)
		{
			if (!allScenes.Contains(file))
			{
				var fileName = Path.GetFileName(file);
				EditorSceneManager.OpenScene("Assets/Scenes/" + fileName);
				break;
			}
		}
		this.Close();
	}
	#endregion

	#region BUILD
	public static string profile_name = "ProfileDataSourceSettings";

	private static AddressableAssetSettings settings;

	private static string BUNDLE_NAME = "facility_rps_test_new";
	private static string FOLDER_NAME = "/BundleFiles/";

	void Build()
	{
		var obj = GameObject.Find(prefabName);
		DestroyImmediate(obj);

		var path = Application.streamingAssetsPath + FOLDER_NAME;
		Directory.CreateDirectory(path);

		BuildPackage();
		if (lockBundle)
			BuildBundle();
		else
			EditorPrefs.SetString("BUNDLE_NAME", "");
		var objects = SceneManager.GetActiveScene().GetRootGameObjects();
		bool hasStartup = false;
		foreach (var o in objects)
		{
			if (o.name == prefabName)
			{
				hasStartup = true;
				break;
			}
		}
		if (!hasStartup)
			SpawnStartup();
		UploadFiles();
	}

	void BuildPackage()
	{
		Scene openedScene = SceneManager.GetActiveScene();
		string newPath = openedScene.path.Replace(openedScene.name, BUNDLE_NAME);
		EditorSceneManager.SaveScene(openedScene, newPath, true);

		var path = Application.streamingAssetsPath + FOLDER_NAME;

		var exportedPackageAssetList = new List<string>();
		exportedPackageAssetList.Add(newPath);

		AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), path + BUNDLE_NAME + ".unitypackage",
			ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void BuildBundle()
	{
		Scene openedScene = SceneManager.GetActiveScene();
		EditorSceneManager.SaveScene(openedScene, openedScene.path);

		settings = AddressableAssetSettingsDefaultObject.Settings;

		foreach (var g in settings.groups)
		{
			BundledAssetGroupSchema bundledAssetGroupSchema = g.GetSchema<BundledAssetGroupSchema>();
			if (bundledAssetGroupSchema != null && bundledAssetGroupSchema.IncludeInBuild != false)
			{
				bundledAssetGroupSchema.IncludeInBuild = false;
				EditorUtility.SetDirty(g);
			}
		}

		var group = settings.CreateGroup(BUNDLE_NAME, false, false, true, null, typeof(ContentUpdateGroupSchema));
		group.Settings.profileSettings.SetValue(group.Settings.activeProfileId, "LocalBuildPath", Application.streamingAssetsPath + FOLDER_NAME);
		group.Settings.profileSettings.SetValue(group.Settings.activeProfileId, "LocalLoadPath", Application.streamingAssetsPath + FOLDER_NAME);

		var guid = AssetDatabase.AssetPathToGUID(openedScene.path);

		var e = settings.CreateOrMoveEntry(guid, group, false, false);
		var entriesAdded = new List<AddressableAssetEntry> { e };

		group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
		settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

		BundledAssetGroupSchema b = group.AddSchema<BundledAssetGroupSchema>();

		b.IncludeInBuild = true;
		b.BuildPath.SetVariableByName(settings, "LocalBuildPath");
		b.LoadPath.SetVariableByName(settings, "LocalLoadPath");

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		AddressableAssetSettings
			.BuildPlayerContent(out AddressablesPlayerBuildResult result);
		bool success = string.IsNullOrEmpty(result.Error);

		if (!success)
		{
			Debug.Log("Addressables build error encountered: " + result.Error);
		}
		EditorPrefs.SetString("BUNDLE_NAME", Path.GetFileName(result.AssetBundleBuildResults[1].FilePath));
		Debug.Log(result.AssetBundleBuildResults[1].FilePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void UploadFiles()
	{
		this.Close();
		EditorApplication.isPlaying = true;
	}

	static string prefabName = "StartUp";
	void SpawnStartup()
	{
		var obj = Resources.Load(prefabName);
		var startupObject = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
		startupObject.name = prefabName;
	}
	#endregion
}
#endif