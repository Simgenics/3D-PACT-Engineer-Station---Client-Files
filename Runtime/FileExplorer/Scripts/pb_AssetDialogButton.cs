using System;
using LMS.Models;
using TMPro;

namespace FileDialogSystem
{
    public class pb_AssetDialogButton : pb_DialogButton<AssetBundleDto>
    {
        private Action<AssetBundleDto> callbackAction;
        public override void SetDelegateAndValue(Action<AssetBundleDto> del, AssetBundleDto buttonValue)
        {
            value = buttonValue;
            callbackAction = del;
            onClick.AddListener(HandleButtonClicked);
                
            SetText();
        }
        
        private void HandleButtonClicked()
        {
            callbackAction.Invoke(value);
        }

        public override void SetText()
        {
            if (value != null)
            {
                var text = GetComponentInChildren<TextMeshProUGUI>();
                text.text = value.assetFileFileName;
            }
        }
    }
}