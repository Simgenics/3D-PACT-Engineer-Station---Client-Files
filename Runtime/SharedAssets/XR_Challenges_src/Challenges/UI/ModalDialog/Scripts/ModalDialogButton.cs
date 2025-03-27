using UnityEngine;
using TMPro;
using System;
using static ModalDialog.ModalDialogUI;
using UnityEngine.UI;

namespace ModalDialog
{
    public class ModalDialogButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _buttonLabel;
        [SerializeField] private Button _button;
        public Action<DialogCustomData> Callback { get; private set; }
        public Button Button => _button;

        private ModalDialogUI _modalDialog;

        public void Initialize(ModalDialogUI modalDialog, DialogAnswer answer)
        {
            _modalDialog = modalDialog;
            _buttonLabel.text = answer.Answer;
            Callback = answer.Callback;
            answer.ButtonUI = this;
        }

        public void OnClick()
        {
            _modalDialog.AnswerSelected(this);
        }

        public void ChangeButtonText(string buttonText)
        {
            _buttonLabel.text = buttonText;
        }
    }
}