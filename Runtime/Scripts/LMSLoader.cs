using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LMS.Core
{
	/// <summary>
	/// This class is used to load the LMS API into a scene before it is used.
	/// </summary>
	public static class LMSLoader
	{
		public static readonly string CoreSceneName = "LMS Core";

		public static Action<bool> OnLMSReady = delegate {};

		public static bool LMSReady { get; private set; } = false;
		public static bool LMSAuthenticated { get; private set; } = false;

		/// <summary>
		/// Public method that can be called from a MonoBehaviour in a scene to make sure the LMS API is present and ready to use.
		/// Starts the LMS API if it is not already running.
		/// </summary>
		/// <param name="caller">MonoBehaviour to be used to run the IEnumerator on.</param>
		/// <param name="onReady">Optional hook for when LMS API is ready, with auth result.</param>
		public static void LoadLMS(MonoBehaviour caller, Action<bool> onReady = null)
		{
			if (LMSReady)
			{
				InvokeAction(LMSAuthenticated, onReady);
				return;
			}

			Debug.Log("LMS not loaded, loading...");
			caller.StartCoroutine(LoadLMSScene(onReady));
		}

		/// <summary>
		/// Loads the LMS Core scene, then checks if it is authenticated.
		/// If it has auth result (success/fail), it will invoke the onReady action with the result.
		/// </summary>
		private static IEnumerator LoadLMSScene(Action<bool> onReady = null)
		{
			SceneManager.sceneLoaded += HandleSceneLoaded;
			var sceneHandle = SceneManager.LoadSceneAsync(CoreSceneName, LoadSceneMode.Additive);

			// Wait for the scene to load
			yield return new WaitUntil(() => sceneHandle.isDone);
			Debug.Log("LMS Core loaded ");

			var authDone = false;
			var authResult = false;
			//Login.OnAutoAuth += (result) =>
			//{
			//	authDone = true;
			//	authResult = result;
			//	Debug.Log($"Auto auth done, auth {(authResult ? "success" : "fail")}.");
			//};

			// Wait for auto auth
			yield return new WaitUntil(() => authDone);

			LMSReady = true;
			LMSAuthenticated = authResult;
			InvokeAction(authResult, onReady);
			OnLMSReady(authResult);
		}

		/// <summary>
		/// Handle the core LMS scene loaded, makes all it's content not destroy on load,
		/// then removes the scene
		/// </summary>
		private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			// Make sure the LMS scene is loaded in additive mode
			if (scene.name == CoreSceneName && loadMode == LoadSceneMode.Additive)
			{
				SceneManager.sceneLoaded -= HandleSceneLoaded;
				LMSAssets.MakeSceneObjectsNotDestroy(scene);
			}
		}

		/// <summary>
		/// Generic method to invoke an action if it is not null.
		/// </summary>
		private static void InvokeAction<T>(T value, Action<T> action = null)
		{
			if (action != null)
			{
				action.Invoke(value);
			}
		}
	}
}