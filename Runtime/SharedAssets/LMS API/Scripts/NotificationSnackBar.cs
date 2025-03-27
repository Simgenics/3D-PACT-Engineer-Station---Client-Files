using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NotificationSnackBar : MonoBehaviour
{
    public GameObject notificationSnackBar;
    public TMP_Text nsbMessage;
    public Image nsbIcon;
    public Image nsbDivider;
    public Sprite questionMark;
    public Sprite exclamationMark;
    public Sprite checkMark;
    public Sprite crosskMark;
    public Sprite informationMark;

    // Start is called before the first frame update
    void Start()
    {
        CloseNotificationSnackBar();
    }

    public void displayNotificationSnackbar(SnackbarTemplates messageType, string message)
    {
        CloseNotificationSnackBar();
        nsbMessage.text = message;
        Color newColor;

        switch (messageType)
        {
            case SnackbarTemplates.Success:
                nsbIcon.sprite = checkMark;
                newColor = new Color(0.0172056f, 1.0f, 0.0f);
                nsbDivider.color = newColor;
                nsbIcon.color = newColor;
                nsbMessage.color = newColor;
                break;
            case SnackbarTemplates.Confirmation:
                nsbIcon.sprite = questionMark;
                newColor = new Color(1.0f, 0.6f, 0.0f);
                nsbDivider.color = newColor;
                nsbIcon.color = newColor;
                nsbMessage.color = newColor;
                break;
            case SnackbarTemplates.Error:
                nsbIcon.sprite = exclamationMark;
                newColor = new Color(1.0f, 0.0521f, 0.0f);
                nsbDivider.color = newColor;
                nsbIcon.color = newColor;
                nsbMessage.color = newColor;
                break;
            case SnackbarTemplates.Warning:
                nsbIcon.sprite = exclamationMark;
                newColor = new Color(1.0f, 1.0f, 0.0f);
                nsbDivider.color = newColor;
                nsbIcon.color = newColor;
                nsbMessage.color = newColor;
                break;
            case SnackbarTemplates.Information:
                nsbIcon.sprite = informationMark;
                newColor = new Color(1.0f, 1.0f, 1.0f);
                nsbDivider.color = newColor;
                nsbIcon.color = newColor;
                nsbMessage.color = newColor;
                break;
        }

        notificationSnackBar.SetActive(true);
    }

    public void CloseNotificationSnackBar()
    {
        notificationSnackBar.SetActive(false);
    }
}

public enum SnackbarTemplates
{
    Success,
    Confirmation,
    Error,
    Warning,
    Information
}
