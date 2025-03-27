#if UNITY_EDITOR
using ImportExportScene;
using LMS.API;
using SharedAssets;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ThreeDPactXR.Licensing;
using System.IO;
using UnityEngine.SceneManagement;

public class ImportExportMenu : SolitaryWindow
{
	static string prefabName = "StartUp";
	static string TENANT_FILE_NAME = "/TenantName.txt";
	private static string userInitial;

	private static Texture2D profileButtonTexture;
	private static GUIStyle profileButtonStyle;

	private bool showLogOut;
	private static GameObject startupObject;

	[MenuItem("Engineer/Import Export")]
	static void ShowWindow()
	{
		Caching.ClearCache();
		RLMManager.Instance.LoadPreferences();

		CheckedOutLicense license = RLMManager.Instance.CheckoutLicense("3dpxr_Engineer", "0.1", 1);
		int licStatus = RLMManager.GetLicenseStatus(license);
		if (licStatus != 0)
		{
			if (LicensingWindow.IsOpen)
			{
				LicensingWindow.ShowWindow();
			}
			Debug.Log("License checkout failed, confirm license settings in 3DPactVR->Licensing");
			return;
		}
		Directory.CreateDirectory(Application.streamingAssetsPath);
		if (!File.Exists(Application.streamingAssetsPath + TENANT_FILE_NAME))
		{
			var wind = GetWindow<TenantWindow>(false, "Tenant Details");

			// Limit size of the window
			wind.minSize = new Vector2(450, 250);
			wind.maxSize = new Vector2(455, 255);
			return;
		}
		else
		{
			var lines = File.ReadAllLines(Application.streamingAssetsPath + TENANT_FILE_NAME);
			if (lines.Length <= 1)
			{
				var wind = GetWindow<TenantWindow>(false, "Tenant Details");

				// Limit size of the window
				wind.minSize = new Vector2(450, 250);
				wind.maxSize = new Vector2(455, 255);
				return;
			}
		}

		var window = GetWindow<SolitaryWindow>(false, "check");
		if (window.titleContent.text == "check")
			window.Close();
		else
			return;

		userInitial = EditorPrefs.GetString("UserInitial", "J");

		var objects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (var obj in objects)
		{
			if (obj.name == prefabName)
			{
				DestroyImmediate(obj);
				startupObject = null;
				break;
			}
		}
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

		AddMetaData();
		//AuthAdapter.instance.Logout(null);
		//EditorPrefs.DeleteKey("LMSToken");
		//EditorPrefs.DeleteKey("LMSTokenExpiry");
		if (!ValidateLMSToken())
		{
			var wind = GetWindow<LoginEditorWindow>(false, "Log in");

			// Limit size of the window
			wind.minSize = new Vector2(450, 350);
			wind.maxSize = new Vector2(455, 355);
		}
		else
		{
			var wnd = GetWindow<ImportExportMenu>(false, "Import Export");

			// Limit size of the window
			wnd.minSize = new Vector2(450, 280);
			wnd.maxSize = new Vector2(455, 285);
		}
	}

	static void AddMetaData()
	{
		List<GameObject> allObjects = new List<GameObject>();
		var objects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
		foreach (var obj in objects)
		{
			allObjects.Add(obj);
			var transforms = obj.GetComponentsInChildren<Transform>();
			foreach (var trans in transforms)
			{
				allObjects.Add(trans.gameObject);
			}
		}
		foreach (var obj in allObjects)
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
			}
		}
		EditorApplication.SaveScene();
	}

	static bool ValidateLMSToken()
	{
		string lmsToken = EditorPrefs.GetString("LMSToken", "");
		if (string.IsNullOrEmpty(lmsToken)) return false;
		var expiryTime = EditorPrefs.GetString("LMSTokenExpiry", "");
		DateTime myDate = DateTime.Parse(expiryTime);
		AssetLoader.instance.SetAuthToken(lmsToken);
		return DateTime.UtcNow <= myDate;
	}

	private void OnGUI()
	{
		if (profileButtonTexture == null)
		{
			profileButtonTexture = Resources.Load<Texture2D>("Sprites/ProfileButton");
			profileButtonStyle = new GUIStyle(GUI.skin.button);
			profileButtonStyle.alignment = TextAnchor.MiddleCenter;
			profileButtonStyle.fontSize = 12;
			profileButtonStyle.normal.background = profileButtonTexture;
		}
		GUIStyle headStyle = new GUIStyle();
		using (var scope = new GUILayout.AreaScope(new Rect(405, 10, 50, 50)))
		{
			if (GUILayout.Button(userInitial, profileButtonStyle, GUILayout.Width(35), GUILayout.Height(30)))
			{
				showLogOut = !showLogOut;
			}
		}
		if (showLogOut)
		{
			using (var scop = new GUILayout.AreaScope(new Rect(345, 40, 100, 30)))
			{
				if (GUILayout.Button("Log Out", GUILayout.Width(100), GUILayout.Height(15)))
				{
					LogOut();
				}
			}
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 20, 400, 50)))
		{
			headStyle.fontSize = 18;
			headStyle.fontStyle = FontStyle.Bold;
			headStyle.normal.textColor = Color.white;
			EditorGUILayout.LabelField("3D PACT Engineer Station");
			EditorGUILayout.LabelField("Select an Option");
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(50, 70, 150, 150)))
		{
			GUI.skin.button.wordWrap = true;
			headStyle.fontSize = 17;
			headStyle.normal.textColor = Color.white;
			if (GUILayout.Button("Retrieve and load an existing asset bundle", GUILayout.Width(150), GUILayout.Height(150)))
			{
				var wnd = GetWindow<CompanyFacilityBundleWindow>(false, "Select a Company, Facility and Bundle");

				// Limit size of the window
				wnd.minSize = new Vector2(1350, 450);
				wnd.maxSize = new Vector2(1355, 455);
				this.Close();
			}
			GUI.skin.button.wordWrap = false;
			using (var scope = new GUILayout.AreaScope(new Rect(46, 25, 150, 150)))
			{
				headStyle.fontSize = 18;
				headStyle.fontStyle = FontStyle.Bold;
				headStyle.normal.textColor = Color.white;
				EditorGUILayout.LabelField("Import", headStyle);
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(250, 70, 150, 150)))
		{
			GUI.skin.button.wordWrap = true;
			headStyle.fontSize = 17;
			headStyle.normal.textColor = Color.white;
			if (GUILayout.Button("Update or create new asset bundle", GUILayout.Width(150), GUILayout.Height(150)))
			{
				var wnd = GetWindow<ExportWindow>(false, "Export");

				// Limit size of the window
				wnd.minSize = new Vector2(450, 280);
				wnd.maxSize = new Vector2(455, 285);
				this.Close();
			}
			GUI.skin.button.wordWrap = false;
			using (var scope = new GUILayout.AreaScope(new Rect(46, 25, 150, 150)))
			{
				headStyle.fontSize = 18;
				headStyle.fontStyle = FontStyle.Bold;
				headStyle.normal.textColor = Color.white;
				EditorGUILayout.LabelField("Export", headStyle);
			}
		}
	}

	private void Update()
	{
		EditorApplication.QueuePlayerLoopUpdate();
		SceneView.RepaintAll();
	}

	void LogOut()
	{
		AuthAdapter.instance.Logout((logoutSuccess) =>
		{
			if (logoutSuccess)
			{
				EditorPrefs.DeleteKey("LMSToken");
				EditorPrefs.DeleteKey("LMSTokenExpiry");
				EditorPrefs.DeleteKey("UserInitial");
				if (startupObject != null)
					DestroyImmediate(startupObject);
				this.Close();
			}
		});
	}
}
#endif