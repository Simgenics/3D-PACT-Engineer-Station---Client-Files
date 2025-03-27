using UnityEngine;
using UnityEngine.SceneManagement;

namespace LMS.Core
{
	public static class LMSAssets
	{
		/// <summary>
		/// A public static class that is used to load the contents of the LMS scene once
		/// and makes sure the components stay active for the rest of the session
		/// </summary>
		public static void MakeSceneObjectsNotDestroy(Scene scene)
		{
			var objects = scene.GetRootGameObjects();
			foreach (var obj in objects)
			{
				Object.DontDestroyOnLoad(obj);
			}

			if (scene.isLoaded)
			{
				SceneManager.UnloadSceneAsync(scene);
			}
			else
			{
				Debug.LogWarning($"Not unloading scene. {objects.Length} objects, scene loaded: {scene.isLoaded}");
			}
		}
	}
}