using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImportExportScene;

namespace FileDialogSystem
{
    public class pb_LoadSceneDialogButton : MonoBehaviour
	{
		public pb_FileDialog dialogPrefab;

		public void OpenLoadPanel()
		{
			pb_FileDialog dlog = GameObject.Instantiate(dialogPrefab);
			dlog.SetDirectory(System.IO.Directory.GetCurrentDirectory());
			dlog.isFileBrowser = true;
			dlog.filePattern = "*.json";
			dlog.AddOnSaveListener(OnOpen);

			pb_ModalWindow.SetContent(dlog.gameObject);
			pb_ModalWindow.SetTitle("Open Scene");
			pb_ModalWindow.Show();
		}

		private void OnOpen(string path)
		{
			Open(path);
		}

		public void Open(string path)
		{
			string san = pb_FileUtility.SanitizePath(path).Replace("%20", " ");

			// if (!pb_FileUtility.IsValidPath(san, ".json"))
			// {
			// 	Debug.LogWarning(san + " not found, or file is not a JSON scene.");
			// 	return;
			// }

			string level = pb_FileUtility.ReadFile(san);

			pb_Scene.LoadSceneConfig(level);
		}
	}
}
