using LMS.API;
using LMS.Models;
using SharedAssets;
using System.Collections.Generic;
using System.Linq;
using ThreeDPactXR.Licensing;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdateBundleWindow : SolitaryWindow
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
	private GameObject startupObject;

	private void InitStyles()
	{
		if (companyDetailsTexture == null)
		{
			companyDetailsTexture = Resources.Load<Texture2D>("Sprites/CompanyDetailsNew");
			activeTexture = Resources.Load<Texture2D>("Sprites/Active");
			inactiveTexture = Resources.Load<Texture2D>("Sprites/Inactive");
			selectedButtonTexture = Resources.Load<Texture2D>("Sprites/SelectedButton");
			normalButtonTexture = Resources.Load<Texture2D>("Sprites/NormalButton");
			SpawnStartupPrefab(false);
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

	static string prefabName = "StartUp";
	void SpawnStartupPrefab(bool hide)
	{
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
		var auth = startupObject.GetComponent(typeof(AuthAdapter));
		var singleton = auth as SingletonBehaviour<AuthAdapter>;
		singleton.Awake();

		var roles = startupObject.GetComponent(typeof(RolesAdapter));
		var single = roles as SingletonBehaviour<RolesAdapter>;
		single.Awake();

		ConnectionStrings strings = startupObject.GetComponent(typeof(ConnectionStrings)) as ConnectionStrings;
		strings.Awake();

		AssetLoader assLoader = startupObject.GetComponent(typeof(AssetLoader)) as AssetLoader;
		assLoader.Awake();

		var facAdapter = startupObject.GetComponent(typeof(FacilitiesAdapter));
		var si = facAdapter as SingletonBehaviour<FacilitiesAdapter>;
		si.Awake();

		var compAdapter = startupObject.GetComponent(typeof(CompaniesAdapter));
		var s = compAdapter as SingletonBehaviour<CompaniesAdapter>;
		s.Awake();

		var assAdapter = startupObject.GetComponent(typeof(AssetBundleAdapter));
		var a = assAdapter as SingletonBehaviour<AssetBundleAdapter>;
		a.Awake();

		var fileAdapter = startupObject.GetComponent(typeof(FileAdapter));
		var f = fileAdapter as SingletonBehaviour<FileAdapter>;
		f.Awake();

		var scenariosAdapter = startupObject.GetComponent(typeof(ScenariosAdapter));
		var sc = scenariosAdapter as SingletonBehaviour<ScenariosAdapter>;
		sc.Awake();
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
								CompanySelected(company);
							}
							ShowCompanyDetails();
						}
						else
						{
							if (GUILayout.Button(company.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								CompanySelected(company);
							}
						}
					}
					else
					{
						if (GUILayout.Button(company.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
						{
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
								FacilitySelected(facility);
							}
							ShowFacilityDetails();
						}
						else
						{
							if (GUILayout.Button(facility.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								FacilitySelected(facility);
							}
						}
					}
					else
					{
						if (GUILayout.Button(facility.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
						{
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
			GUILayout.Label("Select a Bundle or Package");
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
								BundleSelected(bundle.assetBundle);
							}
						}
						else
						{
							if (GUILayout.Button(bundle.assetBundle.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
							{
								BundleSelected(bundle.assetBundle);
							}
						}
					}
					else
					{
						if (GUILayout.Button(bundle.assetBundle.name, normalButton, GUILayout.Width(386), GUILayout.Height(40)))
						{
							BundleSelected(bundle.assetBundle);
						}
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(30, 400, 200, 30)))
		{
			if (GUILayout.Button("Previous", GUILayout.Width(200), GUILayout.Height(30)))
			{

			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(1110, 400, 200, 30)))
		{
			if (selectedBundle != null)
			{
				if (GUILayout.Button("Next", GUILayout.Width(200), GUILayout.Height(30)))
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
						BeforeClose(true);
					}
					else
					{
						var wnd = GetWindow<UploadUpdateBundle>(false, "Update");

						// Limit size of the window
						wnd.minSize = new Vector2(450, 280);
						wnd.maxSize = new Vector2(455, 285);
						BeforeClose(false);
					}
				}
			}
		}
	}

	void BeforeClose(bool failed)
	{
		if (startupObject != null)
			DestroyImmediate(startupObject);
#if UNITY_EDITOR
		if (!failed)
		{
			if (EditorPrefs.GetString("SelectedBundleStatus") == "Locked")
				Debug.Log($"<color={"#ffcc66"}>{"Uploading Update step 1 of 3: Building Bundle"}</color>\n");
			else
				Debug.Log($"<color={"#ffcc66"}>{"Uploading Update step 1 of 3: Building Package"}</color>\n");
		}
#endif
		Close();
	}

	private void OnDestroy()
	{
		if (startupObject != null)
			DestroyImmediate(startupObject);
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
			GetBundles(dto.id);
		}
	}

	void GetBundles(int facilityID)
	{
		EditorCoroutine getfacilitiesRoutine = EditorCoroutine.Start(AssetBundleAdapter.instance.GetAllAssetBundles((result) =>
		{
			foreach (var item in result.items)
			{
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
		EditorPrefs.SetString("SelectedBundleStatus", selectedBundle.status == StatusState.Locked ? "Locked" : "Unlocked");
		EditorPrefs.SetInt("UpdateBundleID", selectedBundle.id);
		EditorPrefs.SetString("SelectedBundleVersion", selectedBundle.versionNumber);
	}

	private void Update()
	{
		EditorApplication.QueuePlayerLoopUpdate();
		SceneView.RepaintAll();
	}
}
