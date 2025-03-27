using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ImportExportScene;

namespace FileDialogSystem
{

    public class pb_ModalWindow : pb_MonoBehaviourSingleton<pb_ModalWindow>
    {
        /**
			 *	Place content GUI items as content's children.
			 */
        public GameObject contents;

        /**
		 *	The title shown in the header bar.
		 */
        public TMP_Text windowTitle;

        /**
		 *	Set the title text shown for this window.
		 */
        public static void SetTitle(string title)
        {
            instance.windowTitle.text = title;
            
        }

        public static void Show()
        {
            foreach (Transform t in instance.transform)
            {
                t.gameObject.SetActive(true);
            }

            instance.transform.SetAsLastSibling();
        }

        public static bool IsVisible()
        {
            foreach (Transform t in instance.transform)
                if (t.gameObject.activeSelf)
                    return true;
            return false;
        }

        public static void Hide()
        {
            foreach (Transform t in instance.transform)
                t.gameObject.SetActive(false);
        }

        /**
		 * Add an already instantiated prefab to the contents of this window.
		 */
        public static void SetContent(GameObject prefab)
        {
            foreach (Transform t in instance.contents.transform)
                pb_ObjectUtility.Destroy(t.gameObject);

            prefab.transform.SetParent(instance.contents.transform, false);
        }

        private void Update()
        {
            if (IsVisible())
            {
                pb_Scene.BlockHotkeys = true;
            }
        }
    }
}
