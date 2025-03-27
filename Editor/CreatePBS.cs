using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThreeDPactXR.Licensing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CreatePBS : SolitaryWindow
{
	private string filePath;
	private string delimeter;
	private Texture2D textFieldTexture;
	private GUIStyle textFieldStyle = null;
	private Texture2D continueButtonTexture;
	private GUIStyle continueButtonStyle = null;
	private bool initialized;
	private Texture2D logo;
	EditorCoroutine pbsRoutine;
	List<GameObject> createdObjects = new List<GameObject>();

	[MenuItem("Engineer/Create PBS Hierarchy")]
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
		var wind = GetWindow<CreatePBS>(false, "PBS Window");

		// Limit size of the window
		wind.minSize = new Vector2(450, 250);
		wind.maxSize = new Vector2(455, 255);
	}

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

			EditorApplication.update += OnEditorUpdate;
		}

		using (var scope = new GUILayout.AreaScope(new Rect(200, 10, 250, 180)))
		{
			GUILayout.Label(logo);
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 50, 400, 180)))
		{
			GUILayout.Label("Enter the path to a CSV file");
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 75, 400, 180)))
		{
			filePath = EditorGUILayout.TextField(filePath, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 100, 400, 180)))
		{
			GUILayout.Label("Enter a list of delimeters with no spaces between them");
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 125, 350, 30)))
		{
			delimeter = EditorGUILayout.TextField(delimeter, textFieldStyle, GUILayout.Width(350), GUILayout.Height(30));
		}
		using (var scope = new GUILayout.AreaScope(new Rect(50, 200, 350, 30)))
		{
			if (creating)
			{
				if (GUILayout.Button("Cancel PBS", continueButtonStyle, GUILayout.Width(350), GUILayout.Height(30)))
				{
					if (pbsRoutine != null)
					{
						pbsRoutine.Stop();
						foreach (var obj in createdObjects)
						{
							DestroyImmediate(obj);
						}
						createdObjects.Clear();
						DestroyImmediate(pbsParent);
						creating = false;
						createdObjectCount = 0;
						this.Close();
					}
				}
			}
			else
			{
				if (GUILayout.Button("Create PBS", continueButtonStyle, GUILayout.Width(350), GUILayout.Height(30)))
				{
					if (!string.IsNullOrEmpty(filePath))
					{
						pbsRoutine = EditorCoroutine.Start(StartPBSCreation());
					}
				}
			}
		}
		if (creating)
		{
			using (var scope = new GUILayout.AreaScope(new Rect(50, 150, 400, 180)))
			{
				GUILayout.Label("Progress: " + createdObjectCount + "/" + pbsRows.Count);
			}
		}
	}

	bool creating = false;
	int createdObjectCount = 0;
	TextAsset pbsFile;
	List<string> pbsRows = new List<string>();
	List<PBSObject> pbsClassObjects = new List<PBSObject>();
	GameObject pbsParent;
	IEnumerator StartPBSCreation()
	{
		creating = true;
		filePath = filePath.Trim('"');
		string[] lines = File.ReadAllLines(filePath);

		for (int i = 0; i < lines.Length; i++)
		{
			string[] row = lines[i].Split(',');
			//exclude comments and empty cells
			if (!String.IsNullOrEmpty(row[0]) && !row[0].StartsWith("//"))
			{
				string r = row[0].Trim(new char[] { '\r', ' ', '_' });
				pbsRows.Add(r);
			}
		}
		pbsParent = new GameObject("PBSParent"+ delimeter);
		foreach (var row in pbsRows)
		{
			string[] names = row.Split(delimeter);
			for (int i = 0; i < names.Length; i++)
			{
				string precedingNames = "";
				string nameStructure = "";
				List<string> addNames = new List<string>();
				for (int j = 0; j < i; j++)
				{
					addNames.Add(names[j]);
				}
				for (int j = 0; j < addNames.Count; j++)
				{
					precedingNames += addNames[j];
					if (j >= addNames.Count - 1)
						continue;
					precedingNames += delimeter;
				}
				nameStructure = i == 0 ? names[i] : precedingNames + delimeter + names[i];

				if (PBSGameObjectExists(names[i], i, precedingNames, nameStructure)) continue;
				var pbs = new PBSObject();
				pbs.Index = i;
				pbs.Name = names[i];
				pbs.PBSCode = row;
				pbs.RootParentName = names[0];
				pbs.NameStructure = nameStructure;
				pbs.ParentStructure = precedingNames;
				if (i == names.Length - 1)
					pbs.IsLastChild = true;
				pbs.ThisObject = new GameObject(i == names.Length - 1 ? row : names[i]);
				pbsClassObjects.Add(pbs);
				createdObjects.Add(pbs.ThisObject);
				yield return null;
			}
			createdObjectCount++;
		}

		for (int i = 0; i < pbsClassObjects.Count; i++)
		{
			var pbsObject = pbsClassObjects[i];
			PBSObject parent = GetPBSParentGameObject(pbsObject.Index, pbsObject.ParentStructure);
			if (parent != null)
				pbsObject.ThisObject.transform.parent = parent.ThisObject.transform;
			else
				pbsObject.ThisObject.transform.parent = pbsParent.transform;
		}
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		creating = false;
		this.Close();
	}

	bool PBSGameObjectExists(string objectName, int index, string precedingNames, string nameStructure)
	{
		foreach (var obj in pbsClassObjects)
		{
			if (obj.Name == objectName && obj.Index == index &&
				obj.ParentStructure == precedingNames && obj.NameStructure == nameStructure)
			{
				return true;
			}
		}
		return false;
	}

	PBSObject GetPBSParentGameObject(int childIndex, string parentStructure)
	{
		foreach (var obj in pbsClassObjects)
		{
			if (obj.Index == childIndex - 1 && obj.NameStructure == parentStructure)
			{
				return obj;
			}
		}
		return null;
	}

	private void OnEditorUpdate()
	{
		Repaint();
	}

	public class PBSObject
	{
		public int Index;
		public string Name;
		public string PBSCode;
		public string RootParentName;
		public string NameStructure;
		public string ParentStructure;
		public bool IsLastChild;
		public GameObject ThisObject;
		public List<PBSObject> Children;
		public int Depth;
	}
}
