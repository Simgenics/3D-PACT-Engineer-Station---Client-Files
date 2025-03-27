#if UNITY_EDITOR
using LMS.API;
using LMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanyFacilityBundleWindow : SolitaryWindow
{
	bool gotCompanies;
	bool gettingCompanies;

	bool gotFacilities;
	bool gettingFacilities;

	bool gotBundles;
	bool gettingBundles;

	private List<CompanyDto> listCompanies = new List<CompanyDto>();
	private List<FacilityDto> listFacilities = new List<FacilityDto>();
	private List<GetAssetBundleForViewDto> listAssets = new List<GetAssetBundleForViewDto>();

	private CompanyDto selectedCompany;
	private FacilityDto selectedFacility;
	private AssetBundleDto selectedBundle;

	private Vector2 companyScrollPos;
	private Vector2 facilityScrollPos;
	private Vector2 assetScrollPos;

	private Vector2 companyDetailsDescriptionScrollview;
	private Vector2 facilityDetailsDescriptionScrollview;

	private GUIStyle detailsBoxStyle = null;
	private GUIStyle selectedButton = null;
	private GUIStyle normalButton = null;

	private Texture2D companyDetailsTexture;
	private Texture2D activeTexture;
	private Texture2D inactiveTexture;
	private Texture2D selectedButtonTexture;
	private Texture2D normalButtonTexture;

	private void InitStyles()
	{
		if (companyDetailsTexture == null)
		{
			companyDetailsTexture = Resources.Load<Texture2D>("Sprites/CompanyDetailsNew");
			activeTexture = Resources.Load<Texture2D>("Sprites/Active");
			inactiveTexture = Resources.Load<Texture2D>("Sprites/Inactive");
			selectedButtonTexture = Resources.Load<Texture2D>("Sprites/SelectedButton");
			normalButtonTexture = Resources.Load<Texture2D>("Sprites/NormalButton");
		}
		if (detailsBoxStyle == null)
		{
			detailsBoxStyle = new GUIStyle(GUI.skin.box);
			detailsBoxStyle.normal.background = companyDetailsTexture;
		}
		if (selectedButton == null)
		{
			selectedButton = new GUIStyle(GUI.skin.button);
			selectedButton.onNormal.textColor = Color.white;
			selectedButton.normal.background = selectedButtonTexture;
			selectedButton.normal.scaledBackgrounds = new Texture2D[] { selectedButtonTexture };
		}
		if (normalButton == null)
		{
			normalButton = new GUIStyle(GUI.skin.button);
			normalButton.normal.textColor = Color.white;
			normalButton.normal.background = normalButtonTexture;
			normalButton.normal.scaledBackgrounds = new Texture2D[] { normalButtonTexture };
		}
	}

	private void OnGUI()
	{
		InitStyles();
		if (!gotCompanies && !gettingCompanies)
		{
			gettingCompanies = true;
			GetCompanies();
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(25, 10, 411, 350)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(130);
			GUILayout.Label("Select a Company");
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (gotCompanies && !gettingCompanies)
			{
				
				companyScrollPos = EditorGUILayout.BeginScrollView(companyScrollPos, false, true);
				foreach (var company in listCompanies)
				{
					if (selectedCompany != null)
					{
						if (company.id == selectedCompany.id)
						{
							if (GUILayout.Button(company.name, selectedButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								if (!loading)
									CompanySelected(company);
							}
							ShowCompanyDetails();
						}
						else
						{
							if (GUILayout.Button(company.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								if (!loading)
									CompanySelected(company);
							}
						}
					}
					else
					{
						if (GUILayout.Button(company.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
						{
							if (!loading)
								CompanySelected(company);
						}
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(460, 10, 411, 350)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(130);
			GUILayout.Label("Select a Facility");
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (gotFacilities && !gettingFacilities)
			{
				facilityScrollPos = EditorGUILayout.BeginScrollView(facilityScrollPos, false, true);
				foreach (var facility in listFacilities)
				{
					if (selectedFacility != null)
					{
						if (facility.id == selectedFacility.id)
						{
							if (GUILayout.Button(facility.name, selectedButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								if (!loading)
									FacilitySelected(facility);
							}
							ShowFacilityDetails();
						}
						else
						{
							if (GUILayout.Button(facility.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								if (!loading)
									FacilitySelected(facility);
							}
						}
					}
					else
					{
						if (GUILayout.Button(facility.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
						{
							if (!loading)
								FacilitySelected(facility);
						}
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(897, 10, 411, 350)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(130);
			GUILayout.Label("Select a Package");
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (gotBundles && !gettingBundles)
			{
				assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos, false, true);
				foreach (var bundle in listAssets)
				{
					if (selectedBundle != null)
					{
						if (bundle.assetBundle.id == selectedBundle.id)
						{
							if (GUILayout.Button(bundle.assetBundle.name, selectedButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								if (!loading)
									BundleSelected(bundle.assetBundle);
							}
						}
						else
						{
							if (GUILayout.Button(bundle.assetBundle.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								if (!loading)
									BundleSelected(bundle.assetBundle);
							}
						}
					}
					else
					{
						if (GUILayout.Button(bundle.assetBundle.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
						{
							if (!loading)
								BundleSelected(bundle.assetBundle);
						}
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(1100, 400, 200, 30)))
		{
			if (selectedBundle != null)
			{
				if (GUILayout.Button("Next", GUILayout.Width(200), GUILayout.Height(30)))
				{
					if (!loading)
						LoadBundle();
				}
			}
		}
	}

	void ShowCompanyDetails()
	{
		if (selectedCompany == null) return;
		EditorGUILayout.BeginVertical();
		GUILayout.Box("", detailsBoxStyle, GUILayout.Width(386), GUILayout.Height(286));
		GUILayout.Space(-245);
		GUI.skin.label.wordWrap = true;
		GUI.skin.label.fontSize = 12;
		//Description
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		string desc = selectedCompany.description.Replace("\n", "");
		desc = desc.Replace("\r", "");
		desc = desc.Replace("\r\n", "");
		companyDetailsDescriptionScrollview = EditorGUILayout.BeginScrollView(companyDetailsDescriptionScrollview, false, true, GUILayout.Width(336), GUILayout.Height(55));
		GUILayout.Label(desc, GUILayout.Width(336), GUILayout.Height(240));
		EditorGUILayout.EndScrollView();
		GUI.skin.label.wordWrap = false;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(30);
		//Email
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUILayout.Label(selectedCompany.email, GUILayout.Width(120), GUILayout.Height(20));
		//contact number
		GUILayout.Space(60);
		GUILayout.Label(selectedCompany.contactNumber, GUILayout.Width(120), GUILayout.Height(20));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(35);
		//address
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUILayout.Label(selectedCompany.address, GUILayout.Width(360), GUILayout.Height(20));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(35);
		//status
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(35);
		GUILayout.Label(selectedCompany.status == true ? activeTexture : inactiveTexture, GUILayout.Width(20), GUILayout.Height(20));
		GUILayout.Space(15);
		GUILayout.Label(selectedCompany.status == true ? "Active" : "Inactive", GUILayout.Width(220), GUILayout.Height(20));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.EndVertical();
	}

	void ShowFacilityDetails()
	{
		if (selectedFacility == null) return;
		EditorGUILayout.BeginVertical();
		GUILayout.Box("", detailsBoxStyle, GUILayout.Width(386), GUILayout.Height(286));
		GUILayout.Space(-245);
		GUI.skin.label.wordWrap = true;
		GUI.skin.label.fontSize = 12;
		//Description
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		string desc = "";
		if (!string.IsNullOrEmpty(selectedFacility.description))
			desc = selectedFacility.description.Replace("\n", "");
		desc = desc.Replace("\r", "");
		desc = desc.Replace("\r\n", "");
		facilityDetailsDescriptionScrollview = EditorGUILayout.BeginScrollView(facilityDetailsDescriptionScrollview, false, true, GUILayout.Width(336), GUILayout.Height(55));
		GUILayout.Label(desc, GUILayout.Width(336), GUILayout.Height(240));
		EditorGUILayout.EndScrollView();
		GUI.skin.label.wordWrap = false;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(30);
		//Email
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUILayout.Label(selectedFacility.emailAddress, GUILayout.Width(120), GUILayout.Height(20));
		//contact number
		GUILayout.Space(60);
		GUILayout.Label(selectedFacility.contactNumber, GUILayout.Width(120), GUILayout.Height(20));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(35);
		//address
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUILayout.Label(selectedFacility.address, GUILayout.Width(360), GUILayout.Height(20));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(35);
		//status
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(35);
		GUILayout.Label(selectedFacility.status == true ? activeTexture : inactiveTexture, GUILayout.Width(20), GUILayout.Height(20));
		GUILayout.Space(15);
		GUILayout.Label(selectedFacility.status == true ? "Active" : "Inactive", GUILayout.Width(220), GUILayout.Height(20));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.EndVertical();
	}

	public void GetCompanies()
	{
		EditorCoroutine getCompaniesRoutine = EditorCoroutine.Start(CompaniesAdapter.instance.GetFacilitiesForUser(companiesResult =>
		{
			if (companiesResult != null)
			{
				listCompanies = companiesResult.items;
				gettingCompanies = false;
				gotCompanies = true;
			}
			else
			{
				var wind = GetWindow<LoginEditorWindow>(false, "Log in");

				// Limit size of the window
				wind.minSize = new Vector2(450, 350);
				wind.maxSize = new Vector2(455, 355);
				Close();
			}
		}));
	}

	void CompanySelected(CompanyDto dto)
	{
		if (selectedCompany == dto)
		{
			gotFacilities = false;
			gettingFacilities = false;
			selectedFacility = null;
			selectedCompany = null;
			return;
		}
		selectedCompany = dto;
		if (!gettingFacilities)
		{
			gotFacilities = true;
			gettingFacilities = true;
			GetFacilities(selectedCompany.id);
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
		if (selectedFacility == dto)
		{
			gotBundles = false;
			gettingBundles = false;
			selectedFacility = null;
			return;
		}
		selectedFacility = dto;
		if (!gettingBundles)
		{
			gotBundles = true;
			gettingBundles = true;
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
		if (selectedBundle == dto)
		{
			selectedBundle = null;
			return;
		}
		selectedBundle = dto;
	}

	#region LOAD
	bool loading;
	List<string> allScenes = new List<string>();
	void LoadBundle()
	{
		loading = true;
		allScenes = Directory.GetFiles(Application.dataPath + "/Scenes").Where(f => !f.Contains(".meta")).ToList();
		var fileQuery = new FileQuery() { container = "assets", file = selectedBundle.assetPath };
		FileAdapter.instance.DownloadFile(result =>
		{
			string path = Application.streamingAssetsPath + "/Loading/";
			Directory.CreateDirectory(path);
			File.WriteAllBytes(path + selectedBundle.assetPath, result);
			AssetDatabase.Refresh();
			AssetDatabase.importPackageCompleted -= ImportPackageComplete;
			AssetDatabase.importPackageCompleted += ImportPackageComplete;
			AssetDatabase.ImportPackage(path + selectedBundle.assetPath, false);
		}, fileQuery, LMSAssetType.UnityPackage);
	}

	private static bool StartsWithFast(string str, string prefix)
	{
		int aLen = str.Length;
		int bLen = prefix.Length;
		int ap = 0; int bp = 0;
		while (ap < aLen && bp < bLen && str[ap] == prefix[bp])
		{
			ap++;
			bp++;
		}

		return bp == bLen;
	}

	private static bool EndsWithFast(string str, string postfix)
	{
		int aLen = str.Length - 1;
		int bLen = postfix.Length - 1;
		while (aLen >= 0 && bLen >= 0 && str[aLen] == postfix[bLen])
		{
			aLen--;
			bLen--;
		}

		return bLen < 0;
	}

	void ImportPackageComplete(string packageName)
	{
		Debug.Log("package name imported = " + packageName);
		string path = Application.streamingAssetsPath + "/Loading/" + packageName + ".unitypackage";
		
		string[] files = Directory.GetFiles(Application.dataPath + "/Scenes").Where(f => !f.Contains(".meta")).ToArray();
		string fileName = "";
		foreach (var file in files)
		{
			if (!allScenes.Contains(file))
			{
				fileName = "Assets/Scenes/" + Path.GetFileName(file);
				break;
			}
		}
		if (string.IsNullOrEmpty(fileName))
			fileName = GetSceneName(path);
		if (!string.IsNullOrEmpty(fileName))
			EditorSceneManager.OpenScene(fileName);
		else
		{
			Debug.Log("No scene found in package");
		}
		if (File.Exists(path))
		{
			File.Delete(path);
			if (File.Exists(path + ".meta"))
			{
				File.Delete(path + ".meta");
			}
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		loading = false;
		this.Close();
	}

	string GetSceneName(string path)
	{
		List<string> contentNames = new List<string>();
		using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
		using (GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress))
		{
			byte[] buffer = new byte[4096];
			while (true)
			{
				gzip.Read(buffer, 0, 512);

				string name = Encoding.ASCII.GetString(buffer, 0, 100).Trim('\0').Trim();
				if (name.Length == 0)
					break;
				else if (StartsWithFast(name, "./"))
				{
					if (name.Length == 2)
						continue;

					name = name.Substring(2);
				}

				long size = Convert.ToInt64(Encoding.ASCII.GetString(buffer, 124, 12).Trim('\0', ' '), 8);
				if (size > 0) // This is a file entry
				{
					if (Path.GetFileName(name) == "pathname")
					{
						// Fetch asset's path
						using (MemoryStream memoryStream = new MemoryStream(128))
						{
							long remaining = size;
							int bytesRead;
							while ((bytesRead = gzip.Read(buffer, 0, remaining >= 4096 ? 4096 : (int)remaining)) > 0)
							{
								memoryStream.Write(buffer, 0, bytesRead);
								remaining -= bytesRead;
							}

							string assetPath = Encoding.UTF8.GetString(memoryStream.ToArray());
							int newLineIndex = assetPath.IndexOf('\n');
							if (newLineIndex > 0)
								assetPath = assetPath.Substring(0, newLineIndex);

							contentNames.Add(assetPath);
						}
					}
					else
					{
						if (EndsWithFast(name, "/asset"))
						{
						}

						// Skip the file contents
						long remaining = size;
						int bytesRead;
						while ((bytesRead = gzip.Read(buffer, 0, remaining >= 4096 ? 4096 : (int)remaining)) > 0)
							remaining -= bytesRead;
					}
				}

				int offset = 512 - (int)(size % 512);
				if (offset > 0 && offset < 512)
					gzip.Read(buffer, 0, offset);
			}
		}
		string sceneName = "";
		if (contentNames.Where(c => c.EndsWith(".unity")).Any())
			sceneName = contentNames.Where(c => c.EndsWith(".unity")).First();
		Debug.Log("Scene name = " + sceneName);
		return sceneName;
	}
	#endregion

	static string prefabName = "StartUp";
	private void OnDestroy()
	{
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

	void Update()
	{
		if (!Application.isPlaying)
		{
			EditorApplication.QueuePlayerLoopUpdate();
			SceneView.RepaintAll();
		}
	}
}
#endif