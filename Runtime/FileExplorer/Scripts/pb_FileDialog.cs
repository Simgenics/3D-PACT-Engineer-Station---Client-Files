using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LMS.API;
using LMS.Core;
using LMS.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

namespace FileDialogSystem
{

    public class pb_FileDialog : MonoBehaviour
    {
        /// Store the history of the back and forward buttons
        private Stack<string> back = new Stack<string>();
        private Stack<string> forward = new Stack<string>();

        /// Where to put current directory folder buttons.
        public GameObject scrollContent;

        /// Save and cancel buttons.  `onClick` delegates will automatically be added by this script.
        public Button saveButton, cancelButton;

		//open / save button text
		public TMP_Text openButtonText;

        /// The input field that shows the directory path.
        public TMP_InputField directoryCrumbsField;

        /// The input field that allows user to type in file or folder name
        public TMP_InputField fileInputField;

		/// The directory currently being inspected
		public string currentDirectory;

        /// Buttons to navigate folder structures.
        public Button backButton, forwardButton, upButton;

        /// The prefab to populate scrollview contents with
        public pb_SaveDialogButton rowButtonPrefab;
        public pb_LoadDialogButton rowLoadButtonPrefab;
        public pb_SceneButton sceneButtonPrefab;
        public pb_EntityButton entityButtonPrefab;

        /// Stores the LMS data type that is being dealt with in this dialog
        public Type entityTypeLMS { get; set; }
        
        /// If true, files as well as folders will be displayed.  If false, only folders will be
        /// shown.  This also affects the string returned by `OnSave` callback.
        public bool isFileBrowser { get { return _isFileBrowser; } set { _isFileBrowser = value; UpdateDirectoryContents(); } }
        public bool SetWebBrowser { set => _isFileBrowser = value; }

        private bool _isFileBrowser = false;

        /// If `isFileBrowser` is true, this string my be used to filter file results.
        public string filePattern { get { return _filePattern; } set { _filePattern = value; UpdateDirectoryContents(); } }

        private string _filePattern = "";

        /// Called when the user hits the 'Save' button.  The passed variable
        /// is not checked for validity.
        public Action<string> OnSave;

        /// Called when the user hits the 'Load' button for a scene
        public Action<EntityDto> OnLoadScene;
        
        /// Called when the user clicks the LMS button, bool indicates LMS active (cloud) or not (local)
        public Action<bool> OnSetLMS = delegate(bool auth) {};
        
        /// Called if the user cancels this action.
        public Callback OnCancel;

        /// Stores the currently selected item (if any)
        private EntityDto selectedEntity = null;
        
        /// Optional LMS toggle button to switch between local and cloud storage 
        public Toggle lmsToggle;
        
		/**
		* Add a callback when this window is dismissed due to 'Save' being called.
		*/
		public void AddOnSaveListener(Action<string> listener)
		{
			if (OnSave != null)
				OnSave += listener;
			else
				OnSave = listener;
		}

		public void AddOnLoadSceneListener(Action<EntityDto> listener)
		{
			if (OnLoadScene != null)
				OnLoadScene += listener;
			else
				OnLoadScene = listener;
		}

		/**
		 * Add a callback when this window is canceled.
		 */
		public void AddOnCancelListener(Callback listener)
		{
			if (OnCancel != null)
				OnCancel += listener;
			else
				OnCancel = listener;
		}

		private bool mInitialized = false;

		void Awake()
		{
			if (!mInitialized)
				Initialize();
		}
	
		void Initialize()
		{
			mInitialized = true;

			backButton.onClick.RemoveAllListeners();
			forwardButton.onClick.RemoveAllListeners();
			upButton.onClick.RemoveAllListeners();
			cancelButton.onClick.RemoveAllListeners();
			saveButton.onClick.RemoveAllListeners();

			backButton.onClick.AddListener(Back);
			forwardButton.onClick.AddListener(Forward);
			upButton.onClick.AddListener(OpenParentDirectory);
			cancelButton.onClick.AddListener(Cancel);
			saveButton.onClick.AddListener(Save);

			if (lmsToggle)
			{
				lmsToggle.onValueChanged.AddListener(ToggleLMS);
			}
			
			UpdateNavButtonInteractibility();
		}

		private void OnDestroy()
		{
			if (lmsToggle)
			{
				lmsToggle.onValueChanged.RemoveListener(ToggleLMS);
			}
		}
		
		public void SetOpenButtonText(string text)
		{
			openButtonText.text = text;
		}

		/**
		 * Set the currently displayed directory.
		 */
		public void SetDirectory(string directory)
		{
			if (!mInitialized)
				Initialize();

			if (ValidDir(directory))
			{
				forward.Clear();

				if (ValidDir(currentDirectory))
					back.Push(currentDirectory);

				currentDirectory = directory;
			}

			UpdateDirectoryContents();
		}

		public void SetWebDirectory(string directory)
		{
			if (!mInitialized)
				Initialize();

			forward.Clear();
			currentDirectory = directory;
		}

		/**
		 * Update the contents in the scroll view with the available folders (and optionally files) in the `currentDirectory`.
		 */
		public void UpdateDirectoryContents()
		{
			try
			{
				if (currentDirectory == "")
				{
					currentDirectory = Directory.GetCurrentDirectory();
				}
				var children = Directory.GetDirectories(currentDirectory);
				
				UpdateNavButtonInteractibility();

				// hide the contents while working with them, otherwise you get flashes and artifacts
				scrollContent.SetActive(false);

				ClearScrollRect();

				if (children.Length > 0)
				{
					directoryCrumbsField.text = currentDirectory;

					foreach (var t in children)
					{
						var button = GameObject.Instantiate(rowButtonPrefab, scrollContent.transform, true);
						button.SetDelegateAndPath(SetDirectory, t);
						button.transform.localScale = Vector3.one;
					}
				}

				if (isFileBrowser)
				{
					children = Directory.GetFiles(currentDirectory, string.IsNullOrEmpty(filePattern) ? "*" : filePattern);
					foreach (var child in children)
					{
						var button = GameObject.Instantiate(rowButtonPrefab, scrollContent.transform, true);
						button.SetDelegateAndPath(SetFile, child);
						button.transform.localScale = Vector3.one;
					}
				}

				scrollContent.SetActive(true);
			}
			catch (IOException ioError)
			{
				Debug.LogWarning(ioError);
				return;
			}
		}
		
        /**
         * Update the contents in the scroll view with the asset bundles
         */
        public void UpdateDirectoryContents<T>(List<T> entities) where T : EntityDto
        {
	        Debug.Log($"Loading {entities.Count} items...");
	        try
	        {
		        directoryCrumbsField.text = "Available (" + typeof(T).Name + "s)" + " (" + entities.Count + ")";
		        UpdateNavButtonInteractibility();

		        var instance = GetFirstDialog();

		        // hide the contents while working with them, otherwise you get flashes and artifacts
		        scrollContent.SetActive(false);
		        ClearScrollRect();

		        foreach (var entity in entities)
		        {
			        var itemName = $"#{entity.id} '{entity.name}' ({entity.creationTime})";
			        var newButton = Instantiate(entityButtonPrefab, instance.scrollContent.transform);
			        newButton.gameObject.transform.localScale = Vector3.one;
			        newButton.SetDelegateAndValue(instance.SetSelectedEntity<T>, entity);
			        newButton.name += itemName;
		        }

		        scrollContent.SetActive(true);
	        }
	        catch (Exception ioError)
	        {
		        Debug.LogWarning(ioError);
	        }
        }
        
        /// <summary>
        /// Finds a dialog by name
        /// </summary>
        private static pb_FileDialog GetFirstDialog()
        {
	        var instance = FindObjectOfType<pb_FileDialog>(false);
	        if (instance == null)
	        {
		        Debug.LogWarning("Could not find instance of scrollWindow");
		        return null;
	        }
	        return instance;
        }
        
        /// <summary>
        /// Update the details of the selected entity and set its text
        /// </summary>
        private void SetSelectedEntity<T>(EntityDto scene)
        {
            selectedEntity = scene;
            fileInputField.text = Path.GetFileName(scene.name);
        }

        private void ClearScrollRect()
		{
			foreach (Transform t in scrollContent.transform)
				pb_ObjectUtility.Destroy(t.gameObject);
		}

		private bool ValidDir(string dir)
		{
			return dir != null && Directory.Exists(dir);
		}

		private void UpdateNavButtonInteractibility()
		{
			backButton.interactable = back.Count > 0;
			forwardButton.interactable = forward.Count > 0;
			upButton.interactable = ValidDir(currentDirectory) && Directory.GetParent(currentDirectory) != null;
		}

		public void OpenParentDirectory()
		{
			DirectoryInfo parent = Directory.GetParent(currentDirectory);

			if (parent == null)
				return;

			SetDirectory(parent.FullName);
		}

		public void SetFile(string path)
		{
			fileInputField.text = Path.GetFileName(path);
		}

		/**
		 * If OpenParentDirectory() has been called, this opens the Directory that 
		 * it came from.
		 */
		public void Back()
		{
			if (back.Count > 0)
			{
				forward.Push(currentDirectory);
				currentDirectory = back.Pop();
				UpdateDirectoryContents();
			}
		}

		public void Forward()
		{
			if (forward.Count > 0)
			{
				back.Push(currentDirectory);
				currentDirectory = forward.Pop();
				UpdateDirectoryContents();
			}
		}

		/**
		 * Cancel this dialog.  Calling script is responsible for closing the modal window in the `OnCancel`
		 * callback.
		 */
		public void Cancel()
		{
			if (OnCancel != null)
				OnCancel();

			pb_ModalWindow.Hide();
		}

		/**
		 * Exit dialog and call `OnSave` with the current file path.  Calling script is responsible for closing the modal window in the `OnCancel`
		 * callback.
		 */
		private string _platformURL;

		public void Save()
		{
			if (OnLoadScene != null)
			{
				OnLoadScene(selectedEntity);
				return;
			}
			
			if (OnSave != null)
				OnSave(currentDirectory + "/" + GetFilePath());
			else
				Debug.LogWarning("File dialog was dismissed by user but no callback is registered to perform the action!");

			pb_ModalWindow.Hide();
		}

		public void ClearCache()
		{
			
			AssetBundle.UnloadAllAssetBundles(true);
			UnityWebRequest.ClearCookieCache();
			Caching.ClearCache();
			Debug.Log("Cleared cache!");
		}

		/// <summary>
		/// UI toggle to connect LMS & authorise so that we can see the scenes available on the API.
		/// Switches back to local file system when toggled off
		/// </summary>
		public void ToggleLMS(bool lmsActive)
		{
			OnSetLMS.Invoke(lmsActive);

			upButton.gameObject.SetActive(!lmsActive);
			backButton.gameObject.SetActive(!lmsActive);
			forwardButton.gameObject.SetActive(!lmsActive);
			fileInputField.interactable = !lmsActive;

			if (lmsActive)
			{
				Debug.Log("LMS active");
				switch (entityTypeLMS.Name)
				{
					case "AssetBundleDto": LoadFromLMS(GetAssetBundles); break;
					case "SceneDto": LoadFromLMS(GetScenes); break;
					// TODO: Fix for loading via runtime editor
					// case "ScenarioDto": LoadFromLMS(GetScenarios);break;
				}
			}
			else
			{
				UpdateDirectoryContents();
			}
		}

		/// <summary>
		/// Make sure the LMS is loaded and authorised, then get the scenes from the API
		/// </summary>
		private void LoadFromLMS(Action loadAction) {
		
			LMSLoader.LoadLMS(this, (authorised) =>
			{
				if (authorised)
				{
					loadAction.Invoke();;
				}
				else
				{
					AuthAdapter.OnLoginSuccess += loadAction;
				}
			});
		}
		
		/// Kicks off the API call to get the assets, and populates the UI with the results items
		private void GetAssetBundles() 
			=> AssetBundleAdapter.instance.GetAllBundles(HandleEntitiesAPI<PagedResultDtoOfGetAssetBundleForViewDto, GetAssetBundleForViewDto>);
		private void GetScenes() 
			=> ScenesAdapter.instance.GetAllScenes(HandleEntitiesAPI<PagedResultDtoOfGetSceneForViewDto, GetSceneForViewDto>);
		private void GetScenarios()
			=> ScenariosAdapter.instance.GetScenarios(HandleEntitiesAPI<PagedResultDtoOfGetScenarioForViewDto, GetScenarioForViewDto>);
		
		/// <summary>
		/// Handles an LMS API response, converts it to a list of EntityDto and updates the UI
		/// </summary>
		/// <typeparam name="T">Paged result type of entity</typeparam>
		/// <typeparam name="U">Class of each item in EntityCollection List</typeparam>
		private void HandleEntitiesAPI<T,U>(T results) where T : EntityCollection<U>
		{
			Debug.Log("Handling API response");
			if (results.totalCount > 0)
			{
				var typeU = typeof(U);
				Debug.Log("Type of U is " + typeU);
				var entityList = new List<EntityDto>();
				if (typeU == typeof(GetSceneForViewDto))
				{	// Scene
					entityList = new List<EntityDto>(results.items.Select((item => (item as GetSceneForViewDto)?.scene)).ToList());
				} 
				else if (typeU == typeof(GetAssetBundleForViewDto))
				{	// Asset Bundle
					entityList = new List<EntityDto>(results.items.Select((item => (item as GetAssetBundleForViewDto)?.assetBundle)).ToList());
				}
				else if (typeU == typeof(GetScenarioForViewDto))
				{	// Scenario
					entityList = new List<EntityDto>(results.items.Select((item => (item as GetScenarioForViewDto)?.scenario)).ToList());
				}

				if (entityList.Count > 0)
				{
					Debug.Log("Updating directory contents");
					UpdateDirectoryContents(entityList);
				}
				else
				{
					Debug.LogWarning("Could not find any scenes in the LMS");
				}
			}
			else
			{
				Debug.LogWarning("Could not retrieve scenes from the API");
			}
		}
		
		/**
		 * Read the current string in the file input field and make it an actual path (minus directory).
		 */
		private string GetFilePath()
		{
			string path = fileInputField.text;
			return path;
		}
	}
}
