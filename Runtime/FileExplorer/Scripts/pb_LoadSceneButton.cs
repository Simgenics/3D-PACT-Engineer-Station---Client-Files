using UnityEngine;
using ImportExportScene;

namespace FileDialogSystem
{
	public class pb_LoadSceneButton : MonoBehaviour
    {
		public pb_FileDialog dialogPrefab;
		
		/**
		 * Open the load dialog.
		 */
		public void OpenLoadPanel()
		{
			pb_FileDialog dlog = Instantiate(dialogPrefab);
			dlog.SetWebDirectory("https://fuzzydevstorage.blob.core.windows.net/addressableassetstesting");
			
			pb_ModalWindow.SetContent(dlog.gameObject);
			pb_ModalWindow.SetTitle("Load asset bundle");
			pb_ModalWindow.Show();

		}
		
		public void Open(string path)
		{
			Debug.Log("ASD");
			string san = pb_FileUtility.SanitizePath(path);

			if (!pb_FileUtility.IsValidPath(san, ".json"))
			{
				Debug.LogWarning(san + " not found, or file is not a JSON scene.");
				//return;
			}

			string level = pb_FileUtility.ReadFile(san);
			pb_Scene.LoadLevel(level);
		}
    }
}
