using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDownPanel : MonoBehaviour
{


    public GameObject panel;

    private void Start()
    {
        
        panel.SetActive(false);
    }

    public void ChangePanelActive()
    {

        panel.SetActive(!panel.activeSelf);
    }
}
