#if UNITY_EDITOR
using LMS.API;
using LMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LoginEditorWindow : EditorWindow
{
	[SerializeField] private string tenantName = "";
	[SerializeField] private string deploymentURL = "";
	[SerializeField] private string userName = "";
	[SerializeField] private string password = "";

	static string TENANT_FILE_NAME = "/TenantName.txt";

	private Texture2D textFieldTexture;
	private GUIStyle textFieldStyle = null;

	private Texture2D loginButtonTexture;
	private GUIStyle loginButtonStyle = null;

	private Texture2D logo;

	private void OnGUI()
	{
		if (textFieldTexture == null)
		{
			textFieldTexture = Resources.Load<Texture2D>("Sprites/TextField");
			textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.alignment = TextAnchor.MiddleLeft;
			textFieldStyle.padding = new RectOffset(10, 0, 0, 0);
			textFieldStyle.fontStyle = FontStyle.Bold;
			textFieldStyle.fontSize = 16;
			textFieldStyle.normal.textColor = Color.white;
			textFieldStyle.normal.background = textFieldTexture;

			loginButtonTexture = Resources.Load<Texture2D>("Sprites/LoginButton");
			loginButtonStyle = new GUIStyle(GUI.skin.button);
			loginButtonStyle.fontStyle = FontStyle.Bold;
			loginButtonStyle.fontSize = 16;
			loginButtonStyle.normal.textColor = Color.black;
			loginButtonStyle.normal.background = loginButtonTexture;

			logo = Resources.Load<Texture2D>("Sprites/Logo");

			if (File.Exists(Application.streamingAssetsPath + TENANT_FILE_NAME))
			{
				var lines = File.ReadAllLines(Application.streamingAssetsPath + TENANT_FILE_NAME);
				if (lines.Length > 0)
				{
					tenantName = lines[0];
					if (lines.Length > 1)
						deploymentURL = lines[1];
					else
						deploymentURL = "DeploymentURL URL";
				}
				else
				{
					tenantName = "Tenant Name";
					deploymentURL = "DeploymentURL URL";
				}
			}
			else
			{
				tenantName = "Tenant Name";
				deploymentURL = "DeploymentURL URL";
			}
			userName = "Username";
			password = "Password";
		}
		using (var scope = new GUILayout.AreaScope(new Rect(100, -70, 250, 180)))
		{
			GUILayout.Label(logo);
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
			userName = EditorGUILayout.TextField(userName, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 250, 350, 30)))
		{
			password = EditorGUILayout.PasswordField(password, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 300, 350, 30)))
		{
			if (GUILayout.Button("LOGIN", loginButtonStyle, GUILayout.Width(350), GUILayout.Height(30)))
			{
				processLogin();
			}
		}
	}

	private void Update()
	{
		EditorApplication.QueuePlayerLoopUpdate();
		SceneView.RepaintAll();
	}

	public static readonly List<string> WorkflowRoles = new List<string>() { "Roles", "Facilities", "Companies", "Asset Bundles", "Scenarios" };
	public void processLogin()
	{
		if (string.IsNullOrEmpty(tenantName) || tenantName == "Tenant Name" || string.IsNullOrEmpty(deploymentURL) || deploymentURL == "Deployment URL") return;
		CreateTenantFile();
		//Validate input fields
		if (validateUserEntry(userName, password) == false)
		{

		}
		else
		{
			ConnectionStrings.SetDeploymentURL(deploymentURL);
			// Attempt to login
			var userModel = new AuthenticateModel()
			{
				userNameOrEmailAddress = userName,
				password = password,
				tenantName = this.tenantName
			};
			AuthAdapter.instance.AuthWithTenantName(userModel, (authResult) =>
			{
				if (authResult != null)
				{
					AssetLoader.instance.SetAuthToken(authResult.accessToken);
					EditorPrefs.SetString("LMSToken", authResult.accessToken);
					EditorPrefs.SetString("LMSTokenExpiry", DateTimeOffset.UtcNow.AddSeconds(authResult.expireInSeconds).ToString());
					EditorPrefs.SetString("UserInitial", userName.First().ToString());
					CheckPermissions(WorkflowRoles, null);
				}
			});
		}
	}

	public void CheckPermissions(List<string> expectedPermissions, Action permissionsSuccess = null)
	{
		var permissions = new GetRolesInput() { Permissions = expectedPermissions };
		RolesAdapter.instance.GetRoles(permissions, result =>
		{
			if (result != null)
			{
				var roles = result.items;
				var hasPermissions = result.items.Count > 0;
				if (hasPermissions)
				{
					Debug.Log($"Authorized Role: {roles.First().name}");
					permissionsSuccess?.Invoke();

					// Limit size of the window
					var wnd = GetWindow<ImportExportMenu>(false, "Import Export");
					wnd.minSize = new Vector2(450, 280);
					wnd.maxSize = new Vector2(455, 285);

					this.Close();
				}
				else
				{
					Debug.Log("User does not have sufficient permissions.");
				}
			}
		});
	}

	void CreateTenantFile()
	{
		File.WriteAllText(Application.streamingAssetsPath + TENANT_FILE_NAME, String.Empty);
		using (StreamWriter sw = File.AppendText(Application.streamingAssetsPath + TENANT_FILE_NAME))
		{
			sw.WriteLine(tenantName);
			sw.WriteLine(deploymentURL);
		}
	}

	private bool validateUserEntry(string enteredUsername, string enteredPassword)
		=> !string.IsNullOrWhiteSpace(enteredUsername) && !string.IsNullOrWhiteSpace(enteredPassword);
}
#endif