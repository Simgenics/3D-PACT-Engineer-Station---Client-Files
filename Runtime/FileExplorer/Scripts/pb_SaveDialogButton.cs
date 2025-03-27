using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace FileDialogSystem
{

    public class pb_SaveDialogButton : Button
    {
        public string path;
        public Callback<string> OnClick;

        public void SetDelegateAndPath(Callback<string> del, string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            if (di == null)
            {
                Debug.Log("Invalid Directory: " + path);
                return;
            }

            this.path = path;
            OnClick = del;
            onClick.AddListener(() => OnClick(path));

            Text text = GetComponentInChildren<Text>();

            text.text = di.Name;
        }
    }
}
