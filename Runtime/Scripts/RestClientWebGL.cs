using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LMS.API
{
	public class RestClientWebGL
	{
		public static bool logOutput { get;} = true;
		public static SerializationSettings serializerSettings { get; set; } = new SerializationSettings();
		public static SerializationSettings deserializerSettings { get; set; } = new SerializationSettings();
		public static string endPoint { get { return ConnectionStrings.url; } }


		private static string requestColor { get => "#ffcc66"; }
		private static string responseColor { get => "#ffcc66"; }
		private static string responseTypeColor { get => "#55e5c5"; }
		private static string responseSuccessColor { get => "#39e564"; }
		private static string responseErrorColor { get => "#e0370d"; }

		public static string GetResourceUrl(string resource)
			=> $"{endPoint}/{resource}";

		public static IEnumerator GetAsync<T>(string resource, string authToken, Action<T> callback = null, Header[] headers = null)
			=> Request("GET", resource, null, authToken, callback, headers);

		public static IEnumerator PostAsync<T>(string resource, object content, string authToken, Action<T> callback = null, Header[] headers = null)
			=> Request("POST", resource, content, authToken, callback, headers);

		public static IEnumerator PostAsync(string resource, object content, string authToken, Action<bool> callback = null, Header[] headers = null)
			=> Request("POST", resource, content, authToken, callback, headers);

		public static IEnumerator PutAsync<T>(string resource, object content, string authToken, Action<T> callback = null, Header[] headers = null)
			=> Request("PUT", resource, content, authToken, callback, headers);

		public static IEnumerator PutAsync(string resource, object content, string authToken, Action<bool> callback = null, Header[] headers = null)
			=> Request("PUT", resource, content, authToken, callback, headers);

		public static IEnumerator DeleteAsync(string resource, string authToken, Action<bool> callback = null, Header[] headers = null)
			=> Request("DELETE", resource, null, authToken, callback, headers);

		public static IEnumerator Request<T>(string method, string resource, object data = null, string authToken = null, Action<T> callback = null, Header[] headers = null)
		{
			//Log("req start");
			if (!resource.Contains("http"))
			{
				resource = GetResourceUrl(resource); ;
			}
			var uri = resource;  
			var json = JsonConvert.SerializeObject(data, GetJsonSerializerSettings(serializerSettings));

			//Log($"<color={requestColor}>{method}: {uri}</color>\n" +
				//$"<color=orange>Request:</color> {JsonConvert.SerializeObject(data, Formatting.Indented)}\n" +
				//$"<color=orange>Token:</color> {authToken}");

			switch (method.ToUpper())
			{
				case "GET":
					using (var getRequest = UnityWebRequest.Get(uri))
					{
						AddHeaders(getRequest, authToken, headers);
						var request = getRequest.SendWebRequest();
						while (!request.webRequest.isDone)
							yield return null;
						var responseObject = DeserializeContent<T>(getRequest.downloadHandler.text);
						callback?.Invoke(responseObject);
					}
					break;

				case "POST":
					using (UnityWebRequest postRequest = UnityWebRequest.PostWwwForm(uri, json))
					{
						Debug.Log("auth url: " + uri);
						postRequest.method = UnityWebRequest.kHttpVerbPOST;
						postRequest.downloadHandler = new DownloadHandlerBuffer();
						postRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
						postRequest.uploadHandler.contentType = "application/json";
						AddHeaders(postRequest, authToken, headers);

						yield return postRequest.SendWebRequest();

						//Log($"<color={responseColor}>Raw response:</color>\n{postRequest.downloadHandler.text}");
						var responseObject = DeserializeContent<T>(postRequest.downloadHandler.text);
						callback?.Invoke(responseObject);
					}
					break;

				case "PUT":
					using (UnityWebRequest putRequest = UnityWebRequest.Put(uri, json))
					{
						putRequest.method = UnityWebRequest.kHttpVerbPUT;
						putRequest.downloadHandler = new DownloadHandlerBuffer();
						putRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
						putRequest.uploadHandler.contentType = "application/json";

						AddHeaders(putRequest, authToken, headers);
						yield return putRequest.SendWebRequest();

						//Log($"<color={responseColor}>Raw response:</color>\n{putRequest.downloadHandler.text}");
						var responseObject = DeserializeContent<T>(putRequest.downloadHandler.text);
						callback?.Invoke(responseObject);
					}
					break;

				case "DELETE":
					using (UnityWebRequest deleteRequest = UnityWebRequest.Delete(uri))
					{
						deleteRequest.method = UnityWebRequest.kHttpVerbDELETE;
						deleteRequest.downloadHandler = new DownloadHandlerBuffer();

						AddHeaders(deleteRequest, authToken, headers);
						yield return deleteRequest.SendWebRequest();

						var responseObject = DeserializeContent<T>(deleteRequest.downloadHandler.text);
						callback?.Invoke(responseObject);
					}
					break;
			}
		}

		private static void AddHeaders(UnityWebRequest request, string authToken, Header[] headers)
		{
			if (headers != null && headers.Length > 0)
			{
				foreach (Header header in headers)
				{
					request.SetRequestHeader(header.name, header.value);
				}
			}
			request.SetRequestHeader("Content-Type", "application/json");
			//request.SetRequestHeader("Content-Type", "multipart/form-data");
			request.SetRequestHeader("Accept", "application/json");
			if (!string.IsNullOrEmpty(authToken))
			{
				request.SetRequestHeader("Authorization", $"Bearer {authToken}");
			}
		}

		public static T DeserializeContent<T>(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return default;
			}
			var responseJObj = JObject.Parse(text);

			// Success
			//if (!responseJObj.IsEmpty())
			//{
				var result = responseJObj["result"];
				if (result != null)
				{
					var final = result.ToObject<T>();
					return final;
				}
			//}
			return JsonConvert.DeserializeObject<T>(text);
		}

		//public static bool IsEmpty<T>(this IList<T> objects)
		//{
		//	if (objects == null)
		//		return true;

		//	for (int i = objects.Count - 1; i >= 0; i--)
		//	{
		//		if (!objects[i].IsNull())
		//			return false;
		//	}

		//	return true;
		//}

		static JsonSerializerSettings GetJsonSerializerSettings(SerializationSettings settings)
		{
			return new JsonSerializerSettings {
				DefaultValueHandling = settings.defaultValueHandling,
				DateFormatHandling = settings.dateFormatHandling,
				DateParseHandling = settings.dateParseHandling,
				MissingMemberHandling = settings.missingMemberHandling,
				NullValueHandling = settings.nullValueHandling,
				ReferenceLoopHandling = settings.referenceLoopHandling,
				StringEscapeHandling = settings.stringEscapeHandling
			};
		}

		static JsonSerializer GetJsonSerializer(SerializationSettings settings)
		{
			return new JsonSerializer {
				DefaultValueHandling = settings.defaultValueHandling,
				DateFormatHandling = settings.dateFormatHandling,
				DateParseHandling = settings.dateParseHandling,
				MissingMemberHandling = settings.missingMemberHandling,
				NullValueHandling = settings.nullValueHandling,
				ReferenceLoopHandling = settings.referenceLoopHandling,
				StringEscapeHandling = settings.stringEscapeHandling
			};
		}

		static void Log(string message)
		{
			if (logOutput)
			{
				Debug.Log(message);
			}
		}
	}
}