#if UNITY_EDITOR
using ImportExportScene;
using LMS.API;
using SharedAssets;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TenantWindow : EditorWindow
{
	[SerializeField] private string tenantName = "";
	[SerializeField] private string deploymentURL = "";

	static string prefabName = "StartUp";
	static string TENANT_FILE_NAME = "/TenantName.txt";

	private Texture2D textFieldTexture;
	private GUIStyle textFieldStyle = null;
	private Texture2D continueButtonTexture;
	private GUIStyle continueButtonStyle = null;
	private bool initialized;
	private Texture2D logo;
	private GameObject startupObject;

	private void OnGUI()
	{
		if (!initialized)
		{
			initialized = true;

			textFieldTexture = Resources.Load<Texture2D>("Sprites/TextField");
			textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.alignment = TextAnchor.MiddleLeft;
			textFieldStyle.padding = new RectOffset(10, 0, 0, 0);
			textFieldStyle.fontStyle = FontStyle.Bold;
			textFieldStyle.fontSize = 16;
			textFieldStyle.normal.textColor = Color.white;
			textFieldStyle.normal.background = textFieldTexture;

			continueButtonTexture = Resources.Load<Texture2D>("Sprites/LoginButton");
			continueButtonStyle = new GUIStyle(GUI.skin.button);
			continueButtonStyle.fontStyle = FontStyle.Bold;
			continueButtonStyle.fontSize = 16;
			continueButtonStyle.normal.textColor = Color.black;
			continueButtonStyle.normal.background = continueButtonTexture;

			if (File.Exists(Application.streamingAssetsPath + TENANT_FILE_NAME))
			{
				var lines = File.ReadAllLines(Application.streamingAssetsPath + TENANT_FILE_NAME);
				if (lines.Length > 0)
				{
					tenantName = lines[0];
					if (lines.Length > 1)
						deploymentURL = lines[1];
				}
			}
			else
			{
				tenantName = "Tenant Name";
				deploymentURL = "Deployment URL";
			}
			logo = Resources.Load<Texture2D>("Sprites/IconWarning");
		}
		using (var scope = new GUILayout.AreaScope(new Rect(200, 10, 250, 180)))
		{
			GUILayout.Label(logo);
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 50, 400, 180)))
		{
			GUILayout.Label("Enter a tenant name and deployment URL for the project");
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 100, 350, 30)))
		{
			tenantName = EditorGUILayout.TextField(tenantName, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 150, 350, 30)))
		{
			deploymentURL = EditorGUILayout.TextField(deploymentURL, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 200, 350, 30)))
		{
			if (GUILayout.Button("Create File", continueButtonStyle, GUILayout.Width(350), GUILayout.Height(30)))
			{
				if (!string.IsNullOrEmpty(tenantName) && tenantName != "Tenant Name" && !string.IsNullOrEmpty(deploymentURL) && deploymentURL != "Deployment URL")
					CreateTenantFile();
			}
		}
	}

	void CreateTenantFile()
	{
		File.WriteAllText(Application.streamingAssetsPath + TENANT_FILE_NAME, String.Empty);
		using (StreamWriter sw = File.AppendText(Application.streamingAssetsPath + TENANT_FILE_NAME))
		{
			sw.WriteLine(tenantName);
			sw.WriteLine(deploymentURL.Trim());
		}
		var objects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (var obj in objects)
		{
			if (obj.name == prefabName)
			{
				startupObject = obj;
				break;
			}
		}
		if (startupObject == null)
		{
			var obj = Resources.Load(prefabName);
			startupObject = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
			startupObject.name = prefabName;
			startupObject.hideFlags = HideFlags.HideAndDontSave;
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
		this.Close();
	}

	void AddMetaData()
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

	bool ValidateLMSToken()
	{
		string lmsToken = EditorPrefs.GetString("LMSToken", "");
		if (string.IsNullOrEmpty(lmsToken)) return false;
		var expiryTime = EditorPrefs.GetString("LMSTokenExpiry", "");
		DateTime myDate = DateTime.Parse(expiryTime);
		AssetLoader.instance.SetAuthToken(lmsToken);
		return DateTime.UtcNow <= myDate;
	}
}
#endif