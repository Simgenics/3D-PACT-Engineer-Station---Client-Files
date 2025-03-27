#if UNITY_EDITOR
using ThreeDPactXR.Licensing;
using UnityEditor;
using UnityEngine;

public class ExportWindow : SolitaryWindow
{
	private Texture2D backButtonTexture;
	private GUIStyle backButtonStyle = null;

	private void OnGUI()
	{
		if (backButtonTexture == null)
		{
			backButtonTexture = Resources.Load<Texture2D>("Sprites/TransparentButton");
			backButtonStyle = new GUIStyle(GUI.skin.button);
			backButtonStyle.normal.background = backButtonTexture;
		}
		GUIStyle headStyle = new GUIStyle();
		using (var scope = new GUILayout.AreaScope(new Rect(40, 20, 400, 50)))
		{
			headStyle.fontSize = 18;
			headStyle.fontStyle = FontStyle.Bold;
			headStyle.normal.textColor = Color.white;
			EditorGUILayout.LabelField("3D PACT Engineer Station");
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("<  Import / Export", backButtonStyle, GUILayout.Width(120), GUILayout.Height(20)))
			{
				var wnd = GetWindow<ImportExportMenu>(false, "Import Export");

				// Limit size of the window
				wnd.minSize = new Vector2(450, 280);
				wnd.maxSize = new Vector2(455, 285);
				this.Close();
			}
			EditorGUILayout.EndHorizontal();
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(50, 70, 150, 150)))
		{
			GUI.skin.button.wordWrap = true;
			headStyle.fontSize = 17;
			headStyle.normal.textColor = Color.white;
			if (GUILayout.Button("Create an updated version of an existing record", GUILayout.Width(150), GUILayout.Height(150)))
			{
				var wnd = GetWindow<UpdateBundleWindow>(false, "Update");

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
				EditorGUILayout.LabelField("Update", headStyle);
			}
		}
		using (var areaScope = new GUILayout.AreaScope(new Rect(250, 70, 150, 150)))
		{
			GUI.skin.button.wordWrap = true;
			headStyle.fontSize = 17;
			headStyle.normal.textColor = Color.white;
			if (GUILayout.Button("Create a new record", GUILayout.Width(150), GUILayout.Height(150)))
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
				else
				{
					var wnd = GetWindow<NewBundleWindow>(false, "New Bundle");

					// Limit size of the window
					wnd.minSize = new Vector2(900, 450);
					wnd.maxSize = new Vector2(905, 455);
					this.Close();
				}
			}
			GUI.skin.button.wordWrap = false;
			using (var scope = new GUILayout.AreaScope(new Rect(46, 25, 150, 150)))
			{
				headStyle.fontSize = 18;
				headStyle.fontStyle = FontStyle.Bold;
				headStyle.normal.textColor = Color.white;
				EditorGUILayout.LabelField("  New", headStyle);
			}
		}
	}
}
#endif