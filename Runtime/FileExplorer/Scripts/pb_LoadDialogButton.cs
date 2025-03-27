using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FileDialogSystem
{

    public class pb_LoadDialogButton : Button
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
            onClick.AddListener(() => OnClick(di.Name));

            TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();

            text.text = di.Name;
        }
    }
}
