using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LMS.API
{
	public static class RestUploader
	{
		public static Action<double> OnUploadProgress = delegate { };
		public static double UploadProgress;

		static HttpClient client = new HttpClient();
		/// <summary>
		/// Upload a multipart form file to a target URL
		/// </summary>
		/// <summary>
		/// Upload a multipart form file to a target URL
		/// </summary>
		public static async Task<string> UploadFile(string targetUrl, byte[] fileData, bool isBundle = true)
		{
			var bytesSent = 0L;

			// Create HTTP request
			//using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AssetLoader.instance.AuthToken);
			client.Timeout = TimeSpan.FromMinutes(30);

			var fileContent = new ByteArrayContent(fileData);
			using var content = new MultipartFormDataContent();

			string fileName = isBundle ? "test.bundle" : "text.unitypackage";
			content.Add(fileContent, "formFile", fileName);
			Debug.Log("content type = " + content.Headers.ContentType);
			var totalBytes = fileData.Length;

			// POST
			Debug.Log("Uploading file...");
			Debug.Log("File size: " + fileData.Length + " bytes");
			var response = await client.PostAsync(targetUrl, content);

			// Track the download progress
			await using (var stream = await response.Content.ReadAsStreamAsync())
			{
				var buffer = new byte[4096];
				int bytesRead;

				int lastLoggedPercentage = 0;
				while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
				{
					bytesSent += bytesRead;

					var progress = (double)bytesSent / totalBytes;
					var progressPercentage = (int)(progress * 100);

					if (progressPercentage > lastLoggedPercentage)
					{
						OnUploadProgress.Invoke(progress);
						lastLoggedPercentage = progressPercentage;
						Debug.Log("Upload progress: " + progressPercentage + "% (" + bytesSent + "/" + totalBytes + ")");
					}
				}
			}

			// Read response
			var contentString = await response.Content.ReadAsStringAsync();

			// Check if this is a server error HTML page
			if (response.Content.Headers.ContentType.MediaType == "text/html")
			{
				if (response.RequestMessage.RequestUri.AbsoluteUri != targetUrl)
				{
					Debug.Log("Redirected to " + response.RequestMessage.RequestUri.AbsoluteUri);
					return "";
				}
				Debug.LogWarning("Unexpected response: " + contentString);
				return "";
			}

			// Response is JSON		
			var responseJObj = JObject.Parse(contentString);

			// Success
			if (response.IsSuccessStatusCode)
			{
				var result = responseJObj["result"];
				if (result != null)
				{
					return result.ToString();
				}
			}

			// Failed
			Debug.LogWarning("Upload failed: " + responseJObj["message"]);
			return "";
		}

		public static async Task<string> UploadFileWithProgress(string targetUrl, byte[] fileData, bool isBundle = true)
		{
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AssetLoader.instance.AuthToken);
			client.Timeout = TimeSpan.FromMinutes(30);

			// Create progressable byte array content
			var fileContent = new ProgressableByteArrayContent(fileData, (sent, total) =>
			{
				UploadProgress = (float)sent / total;
				//Debug.Log($"Upload progress: {progress:P2}");
			});

			using var content = new MultipartFormDataContent();
			string fileName = isBundle ? "test.bundle" : "text.unitypackage";
			content.Add(fileContent, "formFile", fileName);

			try
			{
				var response = await client.PostAsync(targetUrl, content);
				response.EnsureSuccessStatusCode(); // This will throw if the status code is not successful (2xx)
				//Debug.Log("Response Reason Phrase: " + response.ReasonPhrase);
				var responseBody = await response.Content.ReadAsStringAsync();

				// Check if this is a server error HTML page
				if (response.Content.Headers.ContentType.MediaType == "text/html")
				{
					if (response.RequestMessage.RequestUri.AbsoluteUri != targetUrl)
					{
						Debug.Log($"<color={"#e0370d"}>{"Redirected to " + response.RequestMessage.RequestUri.AbsoluteUri}</color>\n");
						return "";
					}
					Debug.LogWarning("Unexpected response: " + responseBody);
					return "";
				}

				// Response is JSON		
				var responseJObj = JObject.Parse(responseBody);

				// Success
				if (response.IsSuccessStatusCode)
				{
					var result = responseJObj["result"];
					if (result != null)
					{
						return result.ToString();
					}
				}
				return "";
			}
			catch (HttpRequestException ex)
			{
				Debug.LogError($"Request error: {ex.Message}");
				throw; // Rethrow the exception for further handling
			}
			catch (Exception ex)
			{
				Debug.LogError($"An error occurred: {ex.Message}");
				throw; // Rethrow the exception for further handling
			}
		}

		public static async Task<string> Upload(string targetUrl, string filePath, bool isZip = false)
		{
			List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
			var bytes = File.ReadAllBytes(filePath);
			string filename = Path.GetFileName(filePath);
			formData.Add(new MultipartFormFileSection("file", bytes, filename, "application/octet-stream"));
			byte[] boundary = UnityWebRequest.GenerateBoundary();
			// Debug.Log($"boundary: {System.Text.Encoding.UTF8.GetString(boundary, 0, boundary.Length)}");
			// byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
			UnityWebRequest content = UnityWebRequest.Post(targetUrl, formData, boundary);
			content.method = "POST";

			content.downloadHandler = new DownloadHandlerBuffer();
			content.SetRequestHeader("AUTHORIZATION", "Bearer" + AssetLoader.instance.AuthToken);
			content.SetRequestHeader("Content-Type", "multipart/form-data");

			content.SendWebRequest();
			while (content.result == UnityWebRequest.Result.InProgress)
			{
				await Task.Delay(10);
			}
			if (content.result == UnityWebRequest.Result.Success)
			{
				var responseJObj = JObject.Parse(content.downloadHandler.text);
				var result = responseJObj["result"];
				if (result != null)
				{
					return result.ToString();
				}
			}
			Debug.LogError("Upload failed!");
			return "";
		}
	}
}
