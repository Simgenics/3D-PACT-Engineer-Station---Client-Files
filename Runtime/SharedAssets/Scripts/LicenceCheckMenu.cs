using UnityEngine;
using ThreeDPactXR.Licensing;
using FileDialogSystem;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LicenceCheckMenu : MonoBehaviour
{
	public static LicenceCheckMenu Instance;

	[Header("General")]
	[SerializeField]
	private pb_FileDialog dialogPrefab;
	[SerializeField]
	private GameObject mainPanel;
	[SerializeField]
	private GameObject hostPanel;
	[SerializeField]
	private bool isSettingsMenu;
	[Header("Local")]
	[SerializeField]
	private TMP_InputField localFolderInputField;
	[Header("Server")]
	[SerializeField]
	private TMP_InputField serverInputField;
	[SerializeField]
	private TMP_InputField portInputField;

	private CheckedOutLicense checkedOutLicense;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
	}

	private void Start()
	{
		if (isSettingsMenu)
		{
			hostPanel.SetActive(true);
			return;
		}
		hostPanel.SetActive(false);
		CheckLicense();
		SaveLicenseLocation();
	}

	void CheckLicense()
	{
		RLMManager.Instance.LoadPreferences();
		bool licensedFailed = false;
		CheckedOutLicense checkedOutLicense = RLMManager.Instance.CheckoutLicense("3dpxr_constructor", "0.1", 1);
		if (checkedOutLicense.Handle != IntPtr.Zero)
		{
			int licenseStatus = RLMManager.GetLicenseStatus(checkedOutLicense);
			if (licenseStatus != 0)
			{
				licensedFailed = true;
			}
		}
		else
			licensedFailed = true;
		if (licensedFailed)
		{
			Open();
		}
		else if (!isSettingsMenu)
		{
			int loadedScene = SceneManager.GetActiveScene().buildIndex;
			SceneManager.LoadScene(loadedScene + 1);
		}
	}

	public void Open()
	{
		if (!string.IsNullOrEmpty(RLMManager.Instance._prefs.LicenseFile))
		{
			var index = RLMManager.Instance._prefs.LicenseFile.LastIndexOf("/");
			if (index > 0)
			{
				string folderName = RLMManager.Instance._prefs.LicenseFile.Remove(index);
				if (Directory.Exists(folderName))
					localFolderInputField.text = folderName;
				else
					localFolderInputField.text = "";
			}
		}
		if (!string.IsNullOrEmpty(RLMManager.Instance._prefs.LicenseServer))
		{
			serverInputField.text = RLMManager.Instance._prefs.LicenseServer;
		}
		if (!string.IsNullOrEmpty(RLMManager.Instance._prefs.LicenseText))
		{
			portInputField.text = RLMManager.Instance._prefs.LicenseText;
		}
		mainPanel.SetActive(true);
	}

	public void Close()
	{
		mainPanel.SetActive(false);
	}

	public void TestServerConnection()
	{
		if (!string.IsNullOrEmpty(RLMManager.Instance._prefs.LicenseServer) &&
			!string.IsNullOrEmpty(RLMManager.Instance._prefs.LicenseText))
		{
			RLMManager.Instance._prefs.Type = LicensingPrefs.licenseType.server;
			RLMManager.Instance._prefs.LicenseServer = serverInputField.text;
			RLMManager.Instance._prefs.LicenseText = portInputField.text;
			RLMManager.Instance.SavePreferences();
			SaveLicenseLocation();
		}
	}

	public void LoadLicense()
	{
		pb_FileDialog dlog = GameObject.Instantiate(dialogPrefab);
		if (!string.IsNullOrEmpty(localFolderInputField.text))
			dlog.SetDirectory(localFolderInputField.text);
		else
			dlog.SetDirectory(System.IO.Directory.GetCurrentDirectory());
		dlog.isFileBrowser = true;
		dlog.filePattern = "*.lic";
		dlog.AddOnSaveListener(OnLoadLicence);

		pb_ModalWindow.SetContent(dlog.gameObject);
		pb_ModalWindow.SetTitle("Locate Licence File");
		dlog.SetOpenButtonText("Load");
		dlog.fileInputField.text = "locate license file";
		pb_ModalWindow.Show();
	}

	void OnLoadLicence(string path)
	{
		RLMManager.Instance._prefs.Type = LicensingPrefs.licenseType.file;
		RLMManager.Instance._prefs.LicenseFile = path;
		RLMManager.Instance.SavePreferences();
		var index = RLMManager.Instance._prefs.LicenseFile.LastIndexOf("/");
		if (index > 0)
		{
			string folderName = RLMManager.Instance._prefs.LicenseFile.Remove(index);
			if (Directory.Exists(folderName))
				localFolderInputField.text = folderName;
			else
				localFolderInputField.text = "";
		}
		SaveLicenseLocation();
	}

	public void SaveLicenseLocation()
	{
		RLMManager.Instance.ResetInitialization();
		if (RLMManager.Instance.Initialize() != 0)
		{
			Debug.LogError("Licensing Initialization failed.");
		}
		else
		{
			CheckLicense();
		}
	}

	public void Quit()
	{
		if (isSettingsMenu)
			Close();
		else
		{
			if (Application.isPlaying)
			{
#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			}
		}
	}
}
