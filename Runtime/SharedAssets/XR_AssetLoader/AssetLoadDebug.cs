using System;
using System.Collections.Generic;
using System.Linq;
using ImportExportScene;
using ImportExportScene.Serialization;
using Newtonsoft.Json;
using SharedAssets;
using UnityEngine;

public class AssetLoadDebug : MonoBehaviour
{
    // Fetch an asset bundle on Start \nSpecify if it needs to instantiate, optional assetID, and bundle name
    [Header("Fetch Bundled Asset")]
    [SerializeField] private bool fetchBundle;
    [SerializeField] private bool instantiate;
    [SerializeField] private bool isScene;
    [SerializeField] private string assetID;
        
    [SerializeField] private string soloAssetBundle;
    [SerializeField] private string sceneAssetBundle;
    [Header("WebGL Bundle")]
    [SerializeField] private string soloWebGLAssetBundle;
    [SerializeField] private string sceneWebGLAssetBundle;

    // Clear a specific cached asset bundle on Start
    [Header("Cache")]
    [SerializeField] private bool clearCacheOnStart;
    [SerializeField] private string clearBundleName;

    [Header("Scene Config")]
    [SerializeField] private bool testLoadExport;
    [SerializeField] private TextAsset exportJSON;
    
    [Header("Local Asset Bundle")]
    [SerializeField] private bool testLoadLocalBundle;
    [SerializeField] private string localBundleName;
    
    [SerializeField] private List<TextAsset> sceneConfigJSON;
    
    [Obsolete] private bool testLoadJSON;
    [Obsolete] private TextAsset vpbJSON;
    
    private void Start()
    {
        if (fetchBundle)
        {
            if (isScene)
            {
#if UNITY_EDITOR
                AssetLoader.instance.GetAssetBundleWithScene($"{PlatformUtil.GetCurrentPlatform()}/{soloAssetBundle}", $"{PlatformUtil.GetCurrentPlatform()}/{sceneAssetBundle}");
#elif UNITY_WEBGL
                AssetLoader.instance.GetAssetBundleWithScene($"{PlatformUtil.GetCurrentPlatform()}/{soloWebGLAssetBundle}", $"{PlatformUtil.GetCurrentPlatform()}/{sceneWebGLAssetBundle}");
#endif
            }
            else
            {
#if INSTRUCTOR_STATION
                AssetLoader.instance.GetAssetBundle($"{PlatformUtil.GetCurrentPlatform()}/{soloAssetBundle}", assetID, instantiate);
#endif
            }
        }
        

        if (testLoadExport)
        {
            TestLoadSceneConfig();
        }

        if (testLoadLocalBundle)
        {
            AssetLoader.instance.LoadLocalBundle(localBundleName);
        }
        
        if (clearCacheOnStart)
        {
            bool cleared = Caching.ClearAllCachedVersions(clearBundleName);
            Debug.Log($"Trying to clear bundle from cache: {(cleared ? "success" : "failed")}");
        }
    }
    
    private static Action<GameObject> HandleVPBLoaded(SceneConfig newConfig)
    {
        return (result) =>
        {
            Debug.Log("locked and loaded");
            pb_Scene.currentConfig = newConfig;
            AssetLoader.OnObjectCreatedFromBundle -= HandleVPBLoaded(newConfig);
        };
    }

    public void TestLoadSceneConfig()
    {
        if (testLoadExport && exportJSON != null)
        {
            Debug.Log("Testing load exported scene...");
            try
            {
                pb_Scene.LoadSceneConfig(exportJSON.text);
            }
            catch (InvalidOperationException ioe)
            {
                Debug.LogWarning("Could not serialize");
                Debug.LogWarning(ioe);
            }
        }
    }
    public IEnumerable<SceneConfig> GetAvailableConfigs()
        => sceneConfigJSON.Select(config => JsonConvert.DeserializeObject<SceneConfig>(config.text, pb_Serialization.ConverterSettings)).ToList();
    
    
    [Obsolete]
    public void TestLoadVPB()
    {
        if (vpbJSON != null)
        {
            Debug.Log("Testing load VPB...");
            var vpb = JsonUtility.FromJson<VersionedPlatformBundles>(vpbJSON.text);

            var currentPlatform = PlatformUtil.GetCurrentPlatform();

            // TODO: Support multiple dependencies (if needed)
            string scene = "";
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            scene = vpb.winPaths.mainBundle;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            scene = vpb.macPaths.mainBundle;
#elif UNITY_WEBGL
            scene = vpb.webglPaths.mainBundle;
#endif

            if (scene != "")
            {
                // var newConfig = new SceneConfig() { vpb = vpb };
                // AssetLoader.instance.GetAssetBundleVPB($"{currentPlatform}/{scene}", vpb);
                // AssetLoader.OnObjectCreatedFromBundle += HandleVPBLoaded(newConfig);
            }
            else
            {
                Debug.LogWarning("Could not find scene bundle and dependencies");
            }
        }
    }
}
