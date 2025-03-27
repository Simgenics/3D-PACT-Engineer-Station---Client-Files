using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace FileDialogSystem
{
    public enum HostType
    {
        Local,
        Cloud,
    }

    [Obsolete]
    public class pb_HostSelector : MonoBehaviour
    {
        [SerializeField] private Button localButton;
        [SerializeField] private Button cloudButton;
        [SerializeField] private Button directoryButton;

        [SerializeField] private TMP_InputField locationInput;
        [SerializeField] private TextMeshProUGUI textInvalidLocation;
        
        [SerializeField] private HostType selectedType;

#if UNITY_EDITOR
        private pb_FileDialog parentDialog;
        
        private void Awake()
        {
            parentDialog = transform.parent.GetComponent<pb_FileDialog>();
            
            if (selectedType == HostType.Local)
            {
                ListenForCloud();
            }
            else
            {
                ListenForLocal();
            }
        }

        private void ListenForCloud()
        {
            cloudButton.onClick.AddListener(HandleButtonClick);
            AssetLoader.instance.cloudAssets = false;
            cloudButton.interactable = true;
            localButton.interactable = false;
            directoryButton.interactable = true;
            locationInput.text = AssetLoader.instance.localAssets;

            SetLocalContents();
        }

        private void ListenForLocal()
        {
            localButton.onClick.AddListener(HandleButtonClick);
            AssetLoader.instance.cloudAssets = true;
            localButton.interactable = true;
            cloudButton.interactable = false;
            directoryButton.interactable = false;
            locationInput.text = AssetLoader.instance.storageConfig.GetAccountURL;
            
            SetWebContents();
        }

        public void SetDirectory()
        {
            // TODO: Replace with folder browser that works in runtime
            var directory = EditorUtility.OpenFolderPanel("Select Asset Directory", "", "");
            var localFiles = Directory.GetFiles(directory, "vpb*.json");
            if (localFiles.Length == 0)
            {
                textInvalidLocation.enabled = true;
                Debug.LogWarning("No vpb files found in directory. Be sure to point to a directory " +
                    "that has vpb.json files and platform folders containing bundles, like StandaloneWindows64");
            }
            else
            {
                AssetLoader.instance.localAssets = directory;
                SetLocalContents();
            }
        }

        private void SetLocalContents()
        {
            parentDialog.UpdateDirectoryContents();
            locationInput.text = AssetLoader.instance.localAssets;
            textInvalidLocation.enabled = false;
        }

        private void SetWebContents()
        {
            // APIService.instance.GetAll<List<VersionedPlatformBundles>>(HandleFetchedVPBs);
            // var vpbs = AssetLoader.instance.GetAvailableBundles();
            // parentDialog.UpdateDirectoryContentsWeb(vpbs);
            textInvalidLocation.enabled = false;
        }
        
        private void HandleButtonClick()
        {
            if (selectedType == HostType.Local)
            {
                cloudButton.onClick.RemoveListener(HandleButtonClick);
                selectedType = HostType.Cloud;
                ListenForLocal();
            }
            else
            {
                localButton.onClick.RemoveListener(HandleButtonClick);
                selectedType = HostType.Local;
                ListenForCloud();
            }
        }
#endif
    }
}