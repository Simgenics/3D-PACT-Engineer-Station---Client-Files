using UnityEditor;

[InitializeOnLoad]
public static class EditorWindowManager
{
	static EditorWindowManager()
	{
		// Subscribe to the custom event
		EditorApplication.update += ListenForCloseRequest;
	}

	private static void ListenForCloseRequest()
	{
		if (StartUp.CloseWindows)
		{
			CloseUploadWindow();
			StartUp.CloseWindows = false;
		}
	}

	private static void CloseUploadWindow()
	{
		var window = EditorWindow.GetWindow<UploadNewBundle>(false, "UploadNewBundle");
		if (window != null)
			window.Close();
		var newWindow = EditorWindow.GetWindow<UploadUpdateBundle>(false, "UploadUpdateBundle");
		if (newWindow != null)
			newWindow.Close();
	}
}

