using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SharedAssets
{
	[Obsolete("Use LMS instead, see LMSLoader")]
	public class APIService : SingletonBehaviour<APIService>
	{
		[SerializeField] private string apiURL;

		private Coroutine _currentRoutine;

		public void Post(string bodyJsonString)
		{
			_currentRoutine = StartCoroutine(PostRoutine($"{apiURL}/vpb/", bodyJsonString));
		}
		
		[Obsolete]
		public void Put(string bodyJsonString)
		{
			var routine = PutRoutine<VersionedPlatformBundles>($"{apiURL}/vpb/", bodyJsonString, null);
			_currentRoutine = StartCoroutine(routine);
		}
		
		[Obsolete]
		public void Get(string facility)
		{
			var routine = GetRoutine<List<VersionedPlatformBundles>>($"{apiURL}/vpb/{facility}", Debug.Log);
			_currentRoutine = StartCoroutine(routine);
		}
		
		public void GetAll<T>(Action<T> callback)
		{
			var routine = GetRoutine<T>($"{apiURL}/vpb/", callback.Invoke);
			_currentRoutine = StartCoroutine(routine);
		}

		public IEnumerator GetRoutine<T>(string url, Action<T> callback)
		{
			Debug.Log("getting from: " + url);
			var request = UnityWebRequest.Get(url);
			request.SetRequestHeader("Content-Type", "application/json");
			request.downloadHandler = new DownloadHandlerBuffer();
			yield return request.SendWebRequest();
			
			Debug.Log("Status Code: " + request.responseCode);
			if (request.result == UnityWebRequest.Result.Success)
			{
				var result = request.downloadHandler.text;
				var serialized = JsonConvert.DeserializeObject<T>(result);
				callback.Invoke(serialized);
			}
		}
		
		private static IEnumerator PutRoutine<T>(string url, string bodyJson, Action<T> callback)
		{
			Debug.Log("Putting: " + url);
			var request = UnityWebRequest.Put(url, bodyJson);
			var bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
			request.SetRequestHeader("Content-Type", "application/json");
			request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
			yield return request.SendWebRequest();
			Debug.Log("Status Code: " + request.responseCode);

		}

		private static IEnumerator PostRoutine(string url, string bodyJson)
		{
			Debug.Log("Posting: " + url);
			var request = new UnityWebRequest(url, "POST");
			var bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
			request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			yield return request.SendWebRequest();
			Debug.Log("Status Code: " + request.responseCode);
		}
		
		// TODO: Encrypt before sending
		// TODO: Decrypt after receiving
	}
}