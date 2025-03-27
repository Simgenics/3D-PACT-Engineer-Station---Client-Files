using UnityEngine;
using ImportExportScene;

namespace FileDialogSystem
{

    public class pb_SaveSceneButton : MonoBehaviour
    {
        public pb_FileDialog dialogPrefab;

        //public override string tooltip { get { return "Save Scene"; } }

        /**
		 * Open the save dialog.
		 */
        public void OpenSavePanel()
        {
            pb_FileDialog dlog = GameObject.Instantiate(dialogPrefab);
            dlog.SetDirectory(System.IO.Directory.GetCurrentDirectory());
            dlog.isFileBrowser = true;
            dlog.filePattern = "*.json";
            dlog.AddOnSaveListener(OnSave);

            pb_ModalWindow.SetContent(dlog.gameObject);
            pb_ModalWindow.SetTitle("Save Scene");
            pb_ModalWindow.Show();
        }

        private void OnSave(string path)
        {
            Save(path);
        }

        public void Save(string path)
        {
            string san = pb_FileUtility.SanitizePath(path).Replace("%20", " ");

            if (!san.EndsWith(".json"))
                san += ".json";

            //if(!pb_FileUtility.IsValidPath(san, ".json"))
            //{
            //	Debug.LogWarning(san + " is not a valid path.");
            //	return;
            //}

            pb_FileUtility.SaveFile(san, pb_Scene.SaveSceneConfigString());

#if PB_DEBUG
			if(System.IO.File.Exists(path))
				System.Diagnostics.Process.Start(path);
#endif
        }
    }
}
