using UnityEngine;
using static ModalDialog.ModalDialogUI;

namespace ModalDialog
{
    public abstract class ModalDialogCustomContainerBase : MonoBehaviour
    {
        public virtual DialogCustomData CustomData => null;

        public virtual void Initialize(DialogCustomData customData)
        {
        }
    }
}