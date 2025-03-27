using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NotificationBanner : MonoBehaviour
{
    public GameObject notificationBanner;
    public Image ngHeaderColour;
    public Image nbIcon;
    public TMP_Text nbHeading;
    public TMP_Text nbMessage;
    public GameObject nbButton1;
    public GameObject nbButton2;
    public GameObject nbButton3;
    public TMP_Text nbButton1Text;
    public TMP_Text nbButton2Text;
    public TMP_Text nbButton3Text;
    public Sprite questionMark;
    public Sprite exclamationMark;
    public Sprite checkMark;
    public Sprite crosskMark;
    public Sprite informationMark;


    public delegate void Method1ToDelegate(); // This defines what type of method you're going to call.
    public Method1ToDelegate DelegatedMethod1; // This is the variable holding the method you're going to call.

    public delegate void Method2ToDelegate(); // This defines what type of method you're going to call.
    public Method2ToDelegate DelegatedMethod2; // This is the variable holding the method you're going to call.

    public delegate void Method3ToDelegate(); // This defines what type of method you're going to call.
    public Method3ToDelegate DelegatedMethod3; // This is the variable holding the method you're going to call.

    void Start()
    {
        //CloseNotificationBanner();
    }

    public void displayNotificationBanner(
        MessageTemplate messageType,
        string heading, string message,
        string button1Text, Method1ToDelegate button1Method,
        string button2Text = null, Method2ToDelegate button2Method = null,
        string button3Text = null, Method3ToDelegate button3Method = null)
    {
        nbHeading.text = heading;
        nbMessage.text = message;

        DelegatedMethod1 = button1Method;

        if (DelegatedMethod1 != null)
        {
            DelegatedMethod1 = button1Method;
        }
        else
        {
            DelegatedMethod1 = CloseNotificationBanner;
        }

        nbButton1Text.text = button1Text;

        nbButton1.transform.localPosition = new Vector3(0, -97, 0);

        nbButton2.SetActive(false);
        nbButton3.SetActive(false);
        if (!string.IsNullOrWhiteSpace(button2Text))
        {
            nbButton2Text.text = button2Text;
            if (DelegatedMethod2 != null)
            {
                DelegatedMethod2 = button2Method;
            }
            else
            {
                DelegatedMethod2 = CloseNotificationBanner;
            }
            nbButton2.SetActive(true);
            nbButton1.transform.localPosition = new Vector3(-81, -97, 0);
            nbButton2.transform.localPosition = new Vector3(100, -97, 0);
        }

        if (!string.IsNullOrWhiteSpace(button3Text))
        {
            nbButton3Text.text = button3Text;
            if (DelegatedMethod3 != null)
            {
                DelegatedMethod3 = button3Method;
            }
            else
            {
                DelegatedMethod3 = CloseNotificationBanner;
            }
            nbButton3.SetActive(true);
            nbButton1.transform.localPosition = new Vector3(-187, -97, 0);
            nbButton2.transform.localPosition = new Vector3(-6, -97, 0);
            nbButton3.transform.localPosition = new Vector3(177, -97, 0);
        }
        Color newColor;
        
        switch (messageType)
        {
            case MessageTemplate.Success:
                nbIcon.sprite = checkMark;
                newColor = new Color(0.0172056f, 1.0f, 0.0f);
                ngHeaderColour.color = newColor;
                break;
            case MessageTemplate.Confirmation:
                nbIcon.sprite = questionMark;
                newColor = new Color(1.0f, 0.6f, 0.0f);
                ngHeaderColour.color = newColor;
                break;
            case MessageTemplate.Error:
                nbIcon.sprite = exclamationMark;
                newColor = new Color(1.0f, 0.0521f, 0.0f);
                ngHeaderColour.color = newColor;
                break;
            case MessageTemplate.Warning:
                nbIcon.sprite = exclamationMark;
                newColor = new Color(1.0f, 1.0f, 0.0f);
                ngHeaderColour.color = newColor;
                break;
            case MessageTemplate.Information:
                nbIcon.sprite = informationMark;
                newColor = new Color(0.83f, 0.83f, 0.83f);
                ngHeaderColour.color = newColor;
                break;
        }

        notificationBanner.SetActive(true);
    }

    public void CloseNotificationBanner()
    {
        notificationBanner.SetActive(false);
    }


    public void button1Click()
    {
        DelegatedMethod1();
    }

    public void button2Click()
    {
        DelegatedMethod2();
    }

    public void button3Click()
    {
        DelegatedMethod3();
    }
}

public enum MessageTemplate
{
    Success,
    Confirmation,
    Error,
    Warning,
    Information
}