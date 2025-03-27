using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using SharedAssets;
using UnityEngine.UI;

namespace ModalDialog
{
    public class ModalDialogUI : SingletonBehaviour<ModalDialogUI>
    {
        public class DialogCustomData
        {
            public ModalDialogCustomContainerBase customDataPrefab;
        }

        public class Dialog
        {
            public string Title;
            public string Description;
            public DialogCustomData CustomData;
            public List<DialogAnswer> Answers;
        }

        public class DialogAnswer
        {
            public string Answer;
            public Action<DialogCustomData> Callback;
            public ModalDialogButton ButtonUI;

            public DialogAnswer(string answer, Action<DialogCustomData> callback)
            {
                Answer = answer;
                Callback = callback;
            }
        }

        [SerializeField] private Canvas _canvas;
        [SerializeField] private ModalDialogButton _buttonPrefab;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private RectTransform _bodyContainer;
        [SerializeField] private RectTransform _buttonContainer;

        [SerializeField] private List<ModalDialogButton> _buttons;

        private ModalDialogCustomContainerBase _customDataContainer;
        private List<Dialog> _dialogs = new List<Dialog>();
        private bool active = false;

        public void Show(Dialog dialog)
        {
            _dialogs.Add(dialog);
            ShowDialog();
        }

        internal void AnswerSelected(ModalDialogButton modalDialogButton)
        {
            modalDialogButton.Callback.Invoke(_customDataContainer != null ? _customDataContainer.CustomData : null);
            active = false;
            ShowDialog();
        }

        private void ShowDialog()
        {
            if (!active && _dialogs.Count > 0)
            {
                active = true;
                _buttons.ForEach(x => x.gameObject.SetActive(false));
                _title.text = _dialogs[0].Title;
                bool simpleDesctiption = _dialogs[0].CustomData == null;
                _description.gameObject.SetActive(simpleDesctiption);
                _dialogs[0].Answers.ForEach(x => AddButton(x));
                if (_customDataContainer != null)
                {
                    Destroy(_customDataContainer.gameObject);
                }
                if (simpleDesctiption)
                {
                    _description.text = _dialogs[0].Description;
                }
                else
                {
                    _customDataContainer = Instantiate(_dialogs[0].CustomData.customDataPrefab, _bodyContainer);
                    _customDataContainer.transform.SetSiblingIndex(1);
                    _customDataContainer.Initialize(_dialogs[0].CustomData);
                }
                _canvas.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
                _dialogs.RemoveAt(0);
            }
            else if (_dialogs.Count == 0)
            {
                _canvas.enabled = false;
            }
        }

        private ModalDialogUI AddButton(DialogAnswer answer)
        {
            ModalDialogButton button = _buttons.FirstOrDefault(x => !x.gameObject.activeSelf);
            if (button == null)
            {
                button = Instantiate(_buttonPrefab, _buttonContainer);
            }
            button.Initialize(this, answer);
            button.gameObject.SetActive(true);
            return this;
        }
    }
}