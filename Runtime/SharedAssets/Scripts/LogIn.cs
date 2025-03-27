using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LogIn : MonoBehaviour
{
	public static LogIn Instance;

	[SerializeField]
	private TMP_InputField usernameInput;
	[SerializeField]
	private TMP_InputField passwordInput;
	[SerializeField]
	//private TextMeshProUGUI usernameText;

	private string userName;

	public string UserName => userName;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		usernameInput.onEndEdit.AddListener(SaveUserName);
		passwordInput.onEndEdit.AddListener(CheckPassword);
		//usernameText.text = "Not logged in.";
	}

	public void LoginButton()
    {
		SceneManager.LoadScene(1);
	}

	void SaveUserName(string name)
	{
		userName = name;
		
	}

	void CheckPassword(string password)
	{
		//TODO check password
		if (!string.IsNullOrEmpty(password))
		{
			//usernameText.text = "User Name: " + userName;
			//go to next scene
		}
	}

	public void CloseButton()
    {
		Application.Quit();
    }
}
