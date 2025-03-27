using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ImportExportScene.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace LMS.API
{
	public class Header
	{
		public string name { get; set; }
		public string value { get; set; }

		public Header(string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public override string ToString() => $"{name}: {value}";
	}
	
	public sealed class RestClient : IDisposable
	{
		/// <summary>
		/// An event raised on a non-success response from the server.
		/// </summary>
		public static Action<string> onNonSuccessResponse = delegate { };


		public static bool logOutput { get; set; } = true;

		/// The timeout of calls as a milliseconds <see cref="TimeSpan"/>.
		public static TimeSpan timeout { get { return client.Timeout; } set { client.Timeout = value; } }

		public static SerializationSettings serializerSettings { get; set; } = new SerializationSettings();
		public static SerializationSettings deserializerSettings { get; set; } = new SerializationSettings();

		public static string endPoint { get { return ConnectionStrings.url; } }

		public static HttpClient client { get; set; } = new HttpClient();


		private static string requestColor { get => "#ffcc66"; }
		private static string responseColor { get => "#ffcc66"; }
		private static string responseTypeColor { get => "#55e5c5"; }
		private static string responseSuccessColor { get => "#39e564"; }
		private static string responseErrorColor { get => "#e0370d"; }


		public void Dispose() => client.Dispose();


		public static T DeserializeContent<T>(Stream stream)
		{
			if (stream == null || stream.CanRead == false)
			{
				return default;
			}

			var responseType = typeof(T).IsGenericType ? $"{typeof(T).Name}<{typeof(T).GetGenericArguments()?.FirstOrDefault()}>" : typeof(T).Name;

			try
			{
				using (var reader = new StreamReader(stream))
				using (var jsonReader = new JsonTextReader(reader))
				{
					var content = GetJsonSerializer(deserializerSettings).Deserialize<T>(jsonReader);

					Log(content != null ?
						$"<color={responseSuccessColor}>SUCCESS:</color> Deserializing response object: <color={responseTypeColor}>{responseType}</color>\n<color=orange>Response:</color> {JsonConvert.SerializeObject(content, Formatting.Indented)}" :
						$"<color={responseErrorColor}>ERROR:</color> Reponse from server could not be deserialized to provided type: <color={responseTypeColor}>{responseType}</color>.\n<color=orange>Raw response:</color>\n{jsonReader.ReadAsString()}");

					return content;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Error deserializing object response object: {responseType}\n{e}");
				return default;
			}
		}

		public static string GetResourceUrl(string resource)
			=> $"{endPoint}/{resource}";

		public static async Task<T> GetAsync<T>(string resource, string authToken = null, Header[] headers = null)
			=> await GetResponseContent<T>(GetAsync(resource, authToken, headers));

		public static async Task<T> GetAsyncStream<T>(string resource, string authToken = null, Header[] headers = null, bool isBundle = false)
			=> await GetResponseContentStream<T>(GetAsyncStream(resource, authToken, headers, isBundle));

		public static async Task<byte[]> GetStreamAsync(string resource, string authToken = null, Header[] headers = null, bool isBundle = false)
			=> await GetResponseStreamContent(GetAsyncStream(resource, authToken, headers, isBundle));

		public static async Task<HttpResponseMessage> GetAsync(string resource, string authToken = null, Header[] headers = null)
			=> await SendAsync(resource, HttpMethod.Get, authToken, headers);
		
		public static async Task<UnityWebRequest> GetAsyncStream(string resource, string authToken = null, Header[] headers = null, bool isBundle = false)
			=> await SendGetAsync(resource, authToken, headers, isBundle);


		public static async Task<T> PostAsync<T>(string resource, object content, string authToken = null, Header[] headers = null)
			=> await GetResponseContent<T>(PostAsync(resource, content, authToken, headers));

		public static async Task<HttpResponseMessage> PostAsync(string resource, object content, string authToken = null, Header[] headers = null)
			=> await SendAsync(resource, HttpMethod.Post, authToken, content, headers);


		public static async Task<T> PutAsync<T>(string resource, object content, string authToken = null, Header[] headers = null)
			=> await GetResponseContent<T>(SendAsync(resource, HttpMethod.Put, authToken, content, headers));

		public static async Task<HttpResponseMessage> PutAsync(string resource, object content, string authToken = null, Header[] headers = null)
			=> await SendAsync(resource, HttpMethod.Put, authToken, content, headers);


		public static async Task<HttpResponseMessage> DeleteAsync(string resource, string authToken = null, Header[] headers = null)
			=> await SendAsync(resource, HttpMethod.Delete, authToken, headers);
		
		public static async Task<T> DeleteAsync<T>(string resource, string authToken = null, Header[] headers = null)
			=> await GetResponseContent<T>(SendAsync(resource, HttpMethod.Delete, authToken, headers));


		public static async Task<UnityWebRequest> SendGetAsync(string resource, string authToken = null, Header[] headers = null, bool isBundle = false)
		{
			var httpMethod = HttpMethod.Get;
			Log($"<color={requestColor}>{httpMethod}: {GetResourceUrl(resource)}</color>\n" +
				$"<color=orange>Request:</color> null");
			
			var resourceURL = resource;
			if (!resourceURL.Contains("http"))
			{
				resourceURL = resourceURL.Insert(0, $"{ConnectionStrings.url}/");
			}
			
			// Send web request to grab the file
			var unityWebRequest = UnityWebRequest.Get(resourceURL);
			var dlHandler = new DownloadHandlerBuffer();
			unityWebRequest.downloadHandler = dlHandler;
			
			unityWebRequest.SetRequestHeader("Authorization", "Bearer " + authToken);
			unityWebRequest.SendWebRequest();
			
			// Wait for download
			var lastPos = 0f;
			while (unityWebRequest.result == UnityWebRequest.Result.InProgress)
			{
				if (unityWebRequest.downloadProgress > lastPos)
				{
					lastPos = unityWebRequest.downloadProgress;
					Debug.Log($"Download progress: { Mathf.Round(unityWebRequest.downloadProgress*100) }% ({unityWebRequest.downloadedBytes} bytes)");
				}
				await Task.Delay(100);
			}

			// Invoke callback with extracted bundle if/when we got a successful response
			if (unityWebRequest.result == UnityWebRequest.Result.Success)
			{
				Debug.Log("Got success");
				return unityWebRequest;
			}
			
			Debug.Log("Failed :(");
			Debug.Log(unityWebRequest.responseCode);
			return null;
		}

		public static async Task<HttpResponseMessage> SendAsync(string resource, HttpMethod httpMethod, string authToken, Header[] headers)
		{
			Log($"<color={requestColor}>{httpMethod}: {GetResourceUrl(resource)}</color>\n" +
			$"<color=orange>Request:</color> null");

			using (var request = new HttpRequestMessage(httpMethod, GetResourceUrl(resource)))
			{
				AddHeaders(request, authToken, headers);

				return await client.SendAsync(request);
			}
		}

		public static async Task<HttpResponseMessage> SendAsync(string resource, HttpMethod httpMethod, string authToken, object content, Header[] headers)
		{
			var rawUrl = GetResourceUrl(resource);
			using (var body = CreateJsonBody(content))
			using (var request = new HttpRequestMessage(httpMethod, rawUrl))
			{
				Log($"<color={requestColor}>{httpMethod}: {rawUrl}</color>\n" +
					$"<color=orange>Request:</color> {await body.ReadAsStringAsync()}\n");

				request.Content = body;
				AddHeaders(request, authToken, headers);

				return await client.SendAsync(request);
			}
		}


		static void AddHeaders(HttpRequestMessage request, string authToken, Header[] headers)
		{
			request.Headers.Authorization = authToken != null ? new AuthenticationHeaderValue("Bearer", authToken) : null;

			if (headers != null)
			{
				foreach (var header in headers)
				{
					request.Headers.Add(header.name, header.value);
				}
			}
		}

		static HttpContent CreateJsonBody(object value)
		{
			var stream = new MemoryStream();

			SerializeContent(value, stream);
			stream.Seek(0, SeekOrigin.Begin);

			HttpContent httpContent = new StreamContent(stream);
			httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			return httpContent;
		}

		static void SerializeContent(object value, Stream stream)
		{
			using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
			using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.None })
			{
				if (logOutput)
				{
					var t = value.GetType();
					var responseType = t.IsGenericType ? $"{t.Name}<{t.GetGenericArguments()?.FirstOrDefault()}>" : t.Name;
					Log($"<color={responseSuccessColor}>Serializing request object:</color> {responseType}\n{JsonConvert.SerializeObject(value, Formatting.Indented)}");
				}

				GetJsonSerializer(serializerSettings).Serialize(jsonWriter, value);
				jsonWriter.Flush();
			}
		}
		
		static async Task<T> GetResponseContent<T>(Task<HttpResponseMessage> pendingResponse)
		{
			// We could potentially return a base DBO that contains an appropriate error message and handle appropriately from the calling site.
			using var response = await pendingResponse;
			Log($"<color={responseColor}>Raw response:</color>\n{await response.Content.ReadAsStringAsync()}");
			
			// Response is JSON		
			var responseString = await response.Content.ReadAsStringAsync();
			var responseJObj = JObject.Parse(responseString);
		
			// Success
			if (response.IsSuccessStatusCode)
			{
				// Handle a response that doesn't expect a result body (only want success true/false)
				if (typeof(T) == typeof(bool))
				{
					var success = responseJObj["success"];
					if (success != null)
					{
						var final = success.ToObject<T>();
						return final;
					}
				}
				
				// Handle a response that expects a result body
				var result = responseJObj["result"];
				if (result != null)
				{
					var final = result.ToObject<T>();
					return final;
				}
			
				Debug.LogError($"Could not deserialize result in response:\n{responseJObj}");
			}

			// Error
			Debug.LogError($"Response Error Code: {response.StatusCode}");
			var errorObj = responseJObj["error"];

			if (errorObj != null)
			{
				var errMsg = errorObj["message"]?.ToString();
				Debug.LogError($"Error message: {errMsg}");
				onNonSuccessResponse(errMsg);
			}
			else
			{
				Debug.LogWarning("Could not recognize error response!");
				Debug.LogError($"Error response obj: \n{responseJObj}");
				onNonSuccessResponse(errorObj.ToString());
			}
			
			return default;
		}
		
		static async Task<T> GetResponseContentStream<T>(Task<UnityWebRequest> pendingResponse)
		{
			// JSON response stream
			// We could potentially return a base DBO that contains an appropriate error message and handle appropriately from the calling site.
			using var response = await pendingResponse;
			Log($"<color={responseColor}>Raw response:</color>\n{response.result}");
			
			var resultStr = response.downloadHandler.text;

			// Success
			var jsonResult = JsonConvert.DeserializeObject<T>(resultStr, pb_Serialization.ConverterSettings);
			return response.result == UnityWebRequest.Result.Success ? jsonResult : default;
		}

		static async Task<byte[]> GetResponseStreamContent(Task<UnityWebRequest> pendingResponse)
		{
			// JSON response stream
			// We could potentially return a base DBO that contains an appropriate error message and handle appropriately from the calling site.
			using var response = await pendingResponse;
			Log($"<color={responseColor}>Raw response:</color>\n{response.result}");

			return response.downloadHandler.data;
		}

		#region HELPER FUNCTIONS - SERIALIZERS
		static JsonSerializer GetJsonSerializer(SerializationSettings settings)
		{
			return new JsonSerializer {
				 DefaultValueHandling = settings.defaultValueHandling,
				 DateFormatHandling = settings.dateFormatHandling,
				 DateParseHandling = settings.dateParseHandling,
				 MissingMemberHandling = settings.missingMemberHandling,
				 NullValueHandling = settings.nullValueHandling,
				 ReferenceLoopHandling = settings.referenceLoopHandling,
				 StringEscapeHandling = settings.stringEscapeHandling,
				 TypeNameHandling = settings.typeNameHandling
			};
		}
		#endregion


		#region HELPER FUNCTIONS - LOGGING
		public static void Log(string message)
		{
			if (logOutput)
			{
				//Debug.Log(message);
			}
		}
		#endregion
	}
}
