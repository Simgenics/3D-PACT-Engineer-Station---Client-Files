using LMS.API;
using LMS.Models;
using SharedAssets;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class StartUp : MonoBehaviour
{
#if !INSTRUCTOR_STATION
	[SerializeField]
	private GameObject progressPanel;
	[SerializeField]
	private TMPro.TextMeshProUGUI header;
	[SerializeField]
	private Image progressImage;
	[SerializeField]
	private TMPro.TextMeshProUGUI percentText;

	private static string FOLDER_NAME = "/BundleFiles/";
	private static string PACKAGE_FOLDER_NAME = "/PackageFiles/";

	private string bundleName = "";
	private string bundlePathName = "";

	static bool _uploading;
	static bool uploadStarted;

	public static bool CloseWindows;

	public static bool IsUploading => _uploading;

	IEnumerator _upload;

	private void Start()
	{
		progressPanel.SetActive(false);
		if (_upload != null) return;
		_upload = UploadFiles();
		StartCoroutine(_upload);
	}

#region UPLOAD
	IEnumerator UploadFiles()
	{
		_uploading = true;
		uploadStarted = true;
		while (!Application.isPlaying)
			yield return null;
		yield return null;
		progressPanel.SetActive(true);
#if UNITY_EDITOR
		bundleName = EditorPrefs.GetString("BUNDLE_NAME");
		bundlePathName = EditorPrefs.GetString("BUNDLE_PATH_NAME");

		if (EditorPrefs.GetString("SelectedBundleStatus") == "Unlocked")
		{
			string packagePath = Application.streamingAssetsPath + PACKAGE_FOLDER_NAME + bundleName + ".unitypackage";
			yield return StartCoroutine(Upload(packagePath, false));
		}
		else if (!string.IsNullOrEmpty(bundleName))
		{
			string match = bundlePathName.Replace(" ", "");
			var files = Directory.GetFiles(Application.streamingAssetsPath + FOLDER_NAME);
			string bundlePath = "";
			foreach (var file in files)
			{
				if (file.Contains(match) && file.Contains(".bundle"))
				{
					bundlePath = Application.streamingAssetsPath + FOLDER_NAME + bundlePathName;
					break;
				}
			}
			yield return StartCoroutine(Upload(bundlePath, true));
		}
		_upload = null;
		_uploading = false;
#endif
	}

    IEnumerator Upload(string filePath, bool isLocked)
	{
		if (!File.Exists(filePath)) yield break;
		bool updating = false;
#if UNITY_EDITOR
		updating = EditorPrefs.GetBool("UpdatingBundle", false) && EditorPrefs.GetInt("UpdateBundleID", -1) != -1;
#endif
		if (updating)
		{
			if (isLocked)
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading Update step 2 of 3: Uploading Bundle"}</color>\n");
			}
			else
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading Update step 2 of 3: Uploading Package"}</color>\n");
			}
		}
		else
		{
			if (isLocked)
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading New step 2 of 3: Uploading Bundle"}</color>\n");
			}
			else
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading New step 2 of 3: Uploading Package"}</color>\n");
			}
		}

		byte[] buff = File.ReadAllBytes(filePath);

		string uploadedFileName = "";

		var fileQuery = new FileQuery() { container = $"\"assets\"" };
#if UNITY_EDITOR
		string lmsToken = EditorPrefs.GetString("LMSToken", "");
		
		AssetLoader.instance.SetAuthToken(lmsToken);
#endif
		FileAdapter.UploadFile(buff, results => uploadedFileName = results, fileQuery, isLocked);

		// Wait for upload to complete and update progress
		while (FileAdapter.uploading)
		{
			progressImage.fillAmount = (float)RestUploader.UploadProgress;
			percentText.text = (RestUploader.UploadProgress * 100).ToString("0") + "%";
			yield return null;
		}

		if (string.IsNullOrEmpty(uploadedFileName))
		{
			Debug.Log($"<color={"#e0370d"}>{"Upload failed"}</color>\n");
			_upload = null;
			_uploading = false;
			yield break; ;
		}
		//Debug.Log("upload complete, file name: " + uploadedFileName);

		var query = new CreateOrEditAssetBundleDto()
		{
			name = bundleName + (isLocked ? "_bundle" : "_unitypackage"),
			assetPath = uploadedFileName,
			activeStatus = true,
			facilityId = PlayerPrefs.GetInt("selectedFacilityID"),
			status = isLocked ? StatusState.Locked : StatusState.Unlocked,
		};
		
#if UNITY_EDITOR
		if (updating)
		{
			if (isLocked)
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading Update step 3 of 3: Uploading Bundle"}</color>\n");
				query = new CreateOrEditAssetBundleDto()
				{
					name = bundleName + "_bundle",
					assetPath = uploadedFileName,
					activeStatus = true,
					facilityId = PlayerPrefs.GetInt("selectedFacilityID"),
					status = StatusState.Locked,
					id = EditorPrefs.GetInt("UpdateBundleID")
				};
			}
			else
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading Update step 3 of 3: Uploading package"}</color>\n");
				string newVersion = "";
				var version = EditorPrefs.GetString("SelectedBundleVersion");
				string[] parts = version.Split('_');
				if (parts.Length == 3)
				{
					if (int.TryParse(parts[2], out int v))
					{
						newVersion = parts[0] + "_" + parts[1] + "_" + (v + 1);
					}
				}

				query = new CreateOrEditAssetBundleDto()
				{
					versionNumber = newVersion,
					name = bundleName + "_unitypackage",
					assetPath = uploadedFileName,
					activeStatus = true,
					facilityId = PlayerPrefs.GetInt("selectedFacilityID"),
					status = StatusState.Unlocked,
					id = EditorPrefs.GetInt("UpdateBundleID")
				};
			}
		}

		else
		{
			if (isLocked)
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading New step 3 of 3: Creating Bundle Entry"}</color>\n");
			}
			else
			{
				Debug.Log($"<color={"#ffcc66"}>{"Uploading New step 3 of 3: Creating Package Entry"}</color>\n");
			}
		}
#endif
		bool bundleCreated = false;
		if (AssetBundleAdapter.instance == null)
		{
			var assAdapter = GetComponent(typeof(AssetBundleAdapter));
			if (assAdapter == null)
			{
				assAdapter = gameObject.AddComponent(typeof(AssetBundleAdapter));
			}
		}
		AssetBundleAdapter.instance.CreateOrEditAssetBundle(results => {
			Debug.Log($"<color={"#ffcc66"}>{"Upload asset result success: " + results}</color>\n");

			bundleCreated = true;
		}, query);
		while (!bundleCreated)
			yield return null;

		_upload = null;
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#endif
	}
#endregion

#if UNITY_EDITOR
	[ExecuteInEditMode]
	private void OnDrawGizmos()
	{
		if (uploadStarted && !_uploading)
		{
			uploadStarted = false;
			CloseWindows = true;
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
#endif
#endif
}

[System.Serializable]
public class FileData
{
	public byte[] Bytes;
}