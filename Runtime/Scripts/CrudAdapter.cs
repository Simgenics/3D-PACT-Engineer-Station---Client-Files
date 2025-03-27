using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using SharedAssets;
using UnityEngine;

namespace LMS.API
{
	public delegate void TaskComplete<T>(T result);
	
	/// <summary>
	/// A base class for web adapters that perform CRUD operations.
	/// </summary>
	public abstract class CrudAdapter<TAdapter> : SingletonBehaviour<TAdapter> where TAdapter : CrudAdapter<TAdapter>
	{
		public readonly string TOKEN_KEY = "LMSToken";
		
		public bool hasAuthToken => !string.IsNullOrEmpty(authToken);

		public string authToken => AssetLoader.instance.AuthToken;

		public abstract string entityResource { get; }

		protected void AwaitResult<T>(Task<T> task, TaskComplete<T> callback, TaskComplete<string> callbackError = null)
			=> StartCoroutine(Await(task, callback, callbackError));

		protected void AwaitResult(Task<byte[]> task, TaskComplete<byte[]> callback, TaskComplete<string> callbackError = null)
			=> StartCoroutine(Await(task, callback, callbackError));


		IEnumerator Await<T>(Task<T> task, TaskComplete<T> callback, TaskComplete<string> callbackError = null)
		{
			yield return new WaitUntil(() => task.IsCompleted);

			if (task.IsFaulted || task.IsCanceled)
			{
				Debug.LogError($"Error occurred: {task.Exception}");
				var errMessage = $"{task.Exception?.Message[..25]}...";
				callbackError?.Invoke(errMessage);
			}
			else
			{
				callback?.Invoke(task.Result);
			}
		}

		IEnumerator Await(Task<byte[]> task, TaskComplete<byte[]> callback, TaskComplete<string> callbackError = null)
		{
			yield return new WaitUntil(() => task.IsCompleted);

			if (task.IsFaulted || task.IsCanceled)
			{
				Debug.LogError($"Error occurred: {task.Exception}");
				var errMessage = $"{task.Exception?.Message[..25]}...";
				callbackError?.Invoke(errMessage);
			}
			else
			{
				callback?.Invoke(task.Result);
			}
		}

		public IEnumerator Await<T>(Task<T> task, Action<T> callback, Action<string> callbackError = null)
		{
			yield return new WaitUntil(() => task.IsCompleted);

			if (task.IsFaulted || task.IsCanceled)
			{
				Debug.LogError($"Error occurred: {task.Exception}");
				var errMessage = $"{task.Exception?.Message[..25]}...";
				callbackError?.Invoke(errMessage);
			}
			else
			{
				callback?.Invoke(task.Result);
			}
		}

		protected static void ErrorCallback(string errorMessage)
		{
			RestClient.onNonSuccessResponse.Invoke(errorMessage);
		}
		
		public static string AddQueryParameters(string entityResource, object queryParams)
		{
			var url = $"{entityResource}";
			if (queryParams != null)
			{
				var firstProp = true;
				var type = queryParams.GetType();
				var props = type.GetProperties();
				foreach (var property in props)
				{
					if (property.GetValue(queryParams) != null)
					{
						var encoded = System.Uri.EscapeDataString(property.GetValue(queryParams).ToString());
						url += $"{(firstProp ? "?" : "&")}{property.Name}={encoded}";
						firstProp = false;
					}
				}
			}
			return url;
		}
    }
}
