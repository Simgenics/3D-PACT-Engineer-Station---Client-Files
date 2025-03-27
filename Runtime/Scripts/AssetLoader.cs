using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LMS.API;
using LMS.Core;
using LMS.Models;
using SharedAssets;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AssetLoader : MonoBehaviour
{
    public static AssetLoader instance;

    static readonly string LOCAL_BUNDLE_PATH = "/AssetBundleLoader/AssetLoader/";

    public static Action OnDownloadStarted = delegate () { };
    public static Action<long> OnDownloadSize = delegate (long totalSize) { };

    public static Action<float, LMSAssetType> OnImportProgress = delegate (float progress, LMSAssetType assetType) { };

    public static Action<GameObject> OnObjectCreatedFromBundle = delegate { };
    public static Action<float, long, LMSAssetType> OnDownloadProgress = delegate (float progress, long size, LMSAssetType assetType) { };
    public static Action OnDownloadComplete = delegate { };

    public List<Texture2D> GetLoadedTextures => _loadedTextures;
    private readonly List<Texture2D> _loadedTextures = new List<Texture2D>();

    [SerializeField] public bool cloudAssets;
    [SerializeField] public string localAssets;
    [SerializeField] public AzureStorageConfig storageConfig;
    [SerializeField] private bool storeTextures = false;


    //Shader s = assetBundle.LoadAsset<Shader>("xxx/xxx.shadergraph");
    //someMaterial.shader = s;

    public AssetBundleDto AssetBundleLMS { get; private set; }
    public void SetAssetBundleLMS(AssetBundleDto current) => AssetBundleLMS = current;

    /// <summary>
    /// Optional authentication token for fetching bundle.
    /// If null there will be not BearerToken setted.
    /// </summary>

    /**
     * Stores the currently loaded asset bundle. It is important to unload this bundle
     * before trying to load the same one again
     */
    public static AssetBundle currentLoadedBundle;

    public string AuthToken { get; private set; } = null;


    /// <summary>
    /// Handles an LMS authentication results
    /// </summary>
    private void HandleAuth(bool authSuccess)
    {
        LMSLoader.OnLMSReady -= HandleAuth;

        if (authSuccess)
        {
            
        }
        else
        {
            AuthAdapter.OnLoginSuccess += HandleLoginSuccess;
        }
    }

    /// <summary>
    /// Hook for when the Auth Adapter successfully logs in
    /// </summary>
    private void HandleLoginSuccess()
    {
        AuthAdapter.OnLoginSuccess -= HandleLoginSuccess;
    }

    public void SetAuthToken(string token) => AuthToken = token;

	public virtual void Awake()
	{
        if (instance == null)
            instance = this;
	}

	/// <summary>
	/// Loads an asset bundle and instantiates a specific asset
	/// </summary>
	public void GetAssetBundle(string url, string assetID = "", bool init = true)
    {
        GetAssetBundle(url, 1, bundle =>
        {
            Debug.Log("Asset bundle retrieved, loading assets...");
            var loadHandle = bundle.LoadAllAssetsAsync();
            loadHandle.completed += operation => HandleBundleLoaded(loadHandle, bundle, assetID, init);
        });
    }

    /// <summary>
    /// Load an asset bundle already contained in memory as an object
    /// </summary>
    public void LoadAssetBundle(AssetBundle bundle)
    {
        var readHandle = bundle.LoadAllAssetsAsync();
        readHandle.completed += operation => HandleBundleLoaded(readHandle, bundle, "", true);
    }

    AssetBundle bundleToLoad;
    public void GetBundleThenLoad(int id, AssetBundle bundleToLoad)
	{
        this.bundleToLoad = bundleToLoad;
        AssetBundleAdapter.instance.GetAssetBundleForView(LoadBundle, id);
    }

    void LoadBundle(GetAssetBundleForViewDto dto)
	{
        LoadAssetBundleFromDto(dto.assetBundle, bundleToLoad, LMSAssetType.AssetBundle);
    }

    public void LoadAssetBundleFromDto(AssetBundleDto entity, AssetBundle bundle, LMSAssetType assetType = LMSAssetType.AssetBundle)
    {
        if (entity == null)
        {
            Debug.LogWarning("Entity is null");
        }

        SetAssetBundleLMS(entity);

        if (bundle.isStreamedSceneAssetBundle)
            HandleSceneAssetLoaded(bundle, assetType);
        else
        {
            var readHandle = bundle.LoadAllAssetsAsync();
            readHandle.completed += operation => HandleBundleLoaded(readHandle, bundle, "", true);
        }
    }

    /// <summary>
    /// Load an asset bundle which is already loaded in memory (i.e. from an API download)
    /// </summary>
    public void LoadAssetBundleFromMemory(byte[] bundleBytes)
    {
        var readHandle = AssetBundle.LoadFromMemoryAsync(bundleBytes);
        var bundle = readHandle.assetBundle;
        readHandle.completed += readOperation =>
        {
            Debug.Log("Asset bundle loaded from memory, loading assets...");
            var loadHandle = bundle.LoadAllAssetsAsync();
            loadHandle.completed += loadOperation => HandleBundleLoaded(loadHandle, bundle, "", true);
        };
    }

    /// <summary>
    /// Loads a local asset bundle using a path to the file
    /// </summary>
    public void LoadLocalBundle(string bundleName)
    {
        string pathToBundle = Application.streamingAssetsPath + /*LOCAL_BUNDLE_PATH + */"/" + bundleName;
        Debug.Log($"Trying to load local bundle from: {pathToBundle}");
        var loadedBundle = AssetBundle.LoadFromFile(pathToBundle, 0, 0);
        if (loadedBundle == null)
        {
            Debug.LogWarning("Could not fetch local bundle");
            return;
        }
        Debug.Log("Local bundle loaded");
        if (loadedBundle.isStreamedSceneAssetBundle)
            HandleSceneAssetLoaded(loadedBundle, LMSAssetType.AssetBundle);
        else
		{
            var loadHandle = loadedBundle.LoadAllAssetsAsync();
            loadHandle.completed += operation => HandleBundleLoaded(loadHandle, loadedBundle, "", true);
        }
        
    }

    /// <summary>
    /// Called after an asset bundle has been fetched and its containing assets have been loaded
    /// </summary>
    private void HandleBundleLoaded(AssetBundleRequest req, AssetBundle loadedBundle, string assetID = "", bool init = false)
    {
        Debug.Log($"{req.allAssets.Length} assets loaded.");
        currentLoadedBundle = loadedBundle;
        ExtractTextures(req);

        // Instantiate asset
        if (init)
        {
            foreach (var asset in req.allAssets)
            {
                var obj = asset as GameObject;
                if (obj != null)
                {
                    InitAndSetupAssets(obj);
                }
            }
        }
        // Unload bundle
        currentLoadedBundle.Unload(false);
        OnDownloadComplete.Invoke();
    }

    /// <summary>
    /// Takes a target gameObject and setup its child metadata components
    /// </summary>
    private static void InitAndSetupAssets(GameObject selectedAsset)
    {

        //Debug.Log("Assets loaded - instantiating " + selectedAsset.name);
        //var go = Instantiate(selectedAsset, pb_Scene.instance.transform);
        //OnObjectCreatedFromBundle.Invoke(go);
        //pb_Scene.GetCurrentConfig();
    }

    /// <summary>
    /// Extracts textures from a requested asset bundle and caches them for later use
    /// </summary>
    private void ExtractTextures(AssetBundleRequest req)
    {
        // Store loaded textures for later use
        if (storeTextures)
        {
            Debug.Log("Extracting textures from asset bundle");
            foreach (var asset in req.allAssets)
            {
                var tex = asset as Texture2D;
                if (tex != null)
                {
                    _loadedTextures.Add(tex);
                }
            }
            Debug.Log($"{_loadedTextures.Count} textures extracted");
        }
    }

    /// <summary>
    /// In order to load the same asset bundle again in the future, we need to unload it fist.
    /// </summary>
    public static void UnloadBundle()
    {
        if (currentLoadedBundle)
        {
            currentLoadedBundle.Unload(false);
            currentLoadedBundle = null;
        }
    }

    IEnumerator _fetchRoutine;
    public void CancelLoading()
    {
        if (_fetchRoutine != null)
            StopCoroutine(_fetchRoutine);
        if (_downloadRequest != null)
            _downloadRequest.Abort();
        //if (_loadSceneRoutine != null)
        //    StopCoroutine(_loadSceneRoutine);
        //_loadSceneRoutine = null;
        _downloadRequest = null;
        _fetchRoutine = null;
    }

    /// <summary>
    /// Shortcut to start the routine to fetch asset bundle
    /// </summary>
    public void GetAssetBundle(string bundle, uint? bundleID, Action<AssetBundle> callback, LMSAssetType assetType = LMSAssetType.AssetBundle)
    {
        if (_fetchRoutine != null) return;
        _fetchRoutine = FetchAssetBundle(bundle, bundleID, callback, assetType);
        StartCoroutine(_fetchRoutine);
    }

    UnityWebRequest _downloadRequest;
    byte[] _downloadedBytes;
    /// <summary>
    /// Fetches an asset bundle, and invokes a callback if successfully done
    /// </summary>
    private IEnumerator FetchAssetBundle(string bundle, uint? bundleID, Action<AssetBundle> callback, LMSAssetType assetType = LMSAssetType.AssetBundle)
    {
        while (!Caching.ready)
            yield return null;
        // Append base URL to bundle
        var bundleURL = bundle;
        if (!bundle.Contains("http"))
        {
            bundleURL = bundle.Insert(0, storageConfig.GetAccountURL);
        }
        Debug.Log("Getting bundle at: " + bundleURL);

        // Send web request to grab the asset bundle
        _downloadRequest = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL, bundleID.Value, 0);
        if (!string.IsNullOrEmpty(AuthToken))
        {
            _downloadRequest.SetRequestHeader("Authorization", "Bearer " + AuthToken);
        }
        _downloadRequest.SendWebRequest();

        // Get file size
        // StartCoroutine(GetFileSize(bundleURL));

        // Wait for download
        OnDownloadStarted?.Invoke();
        var lastPos = 0f;
        while (_downloadRequest.result == UnityWebRequest.Result.InProgress)
        {
            if (_downloadRequest.downloadProgress > lastPos)
            {
                lastPos = _downloadRequest.downloadProgress;
                OnDownloadProgress.Invoke(lastPos, (long)_downloadRequest.downloadedBytes, assetType);
            }
            yield return null;
        }
        OnDownloadProgress.Invoke(1, (long)_downloadRequest.downloadedBytes, assetType);

        // Invoke callback with extracted bundle if/when we got a successful response
        if (_downloadRequest.result == UnityWebRequest.Result.Success)
        {
            var assetBundle = DownloadHandlerAssetBundle.GetContent(_downloadRequest);
            //AssetBundleManifest testManifst = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //Caching.IsVersionCached(bundleURL, testManifst.GetAssetBundleHash(assetBundle.name));
            callback.Invoke(assetBundle);
        }
        else
        {
            Debug.Log("Failed because of " + _downloadRequest.result);
            Debug.Log(_downloadRequest.responseCode);
        }
        _fetchRoutine = null;
    }
    
    /// <summary>
    /// Load an asset bundle that belongs to a scene
    /// </summary>
    public void GetAssetBundleWithScene(string bundleAssets, string bundleScene)
    {
        GetAssetBundle(bundleAssets, 1, bundle =>
        {
            var loadHandle = bundle.LoadAllAssetsAsync();
            loadHandle.completed += operation =>
            {
                Debug.Log("Assets loaded - fetching scene");
                GetAssetBundle(bundleScene, "");
            };
        });  
    }

    public string currentlyLoadedBundleScene = "";
    /// <summary>
    /// An asset bundle containing a scene has been loaded, now it is time to load the scene
    /// This assumes that the scene's assets bundle has already been retrieved and loaded
    /// </summary>
    /// 
    private void HandleSceneAssetLoaded(AssetBundle assetBundle, LMSAssetType assetType = LMSAssetType.AssetBundle)
    {
        // Make sure it is a scene bundle
        if (assetBundle.isStreamedSceneAssetBundle)
        {
            currentLoadedBundle = assetBundle;

            var scenePaths = assetBundle.GetAllScenePaths();
            currentlyLoadedBundleScene = Path.GetFileNameWithoutExtension(scenePaths[0]);
#if UNITY_EDITOR
            EditorPrefs.SetString("scenePath", scenePaths[0]);
            EditorPrefs.SetBool("IsUploading", false);
            EditorApplication.isPlaying = true;
#endif
        }
        else
        {
            Debug.LogWarning("Asset bundle is not a streamed scene.");
        }
    }

    public void Clear()
	{
        if (!string.IsNullOrEmpty(currentlyLoadedBundleScene))
        {
            var scene = SceneManager.GetSceneByName(currentlyLoadedBundleScene);
            if (scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(currentlyLoadedBundleScene);
                currentlyLoadedBundleScene = "";
                UnloadBundle();
            }
            else
            {
                SceneManager.sceneLoaded -= ClearAfterLoad;
                SceneManager.sceneLoaded += ClearAfterLoad;
            }
        }
    }

    void ClearAfterLoad(Scene scene, LoadSceneMode mode)
	{
        if (scene == SceneManager.GetSceneByName(currentlyLoadedBundleScene))
		{
            SceneManager.sceneLoaded -= ClearAfterLoad;
            SceneManager.UnloadSceneAsync(currentlyLoadedBundleScene);
            currentlyLoadedBundleScene = "";
            UnloadBundle();
        }
	}

    /// <summary>
    /// Gets the size of a bundle to be retrieved
    /// </summary>
    private IEnumerator GetFileSize(string url)
    {
        var unityWebRequest = UnityWebRequest.Head(url);
        yield return unityWebRequest.SendWebRequest();
        var size = unityWebRequest.GetResponseHeader("Content-Length");

        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            OnDownloadSize.Invoke(Convert.ToInt64(size));
        }
        else
        {
            Debug.LogWarning("Error While Getting Length: " + unityWebRequest.error);
        }
    }
}
