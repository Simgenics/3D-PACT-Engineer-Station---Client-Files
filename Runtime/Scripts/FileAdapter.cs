using LMS.Models;
using System;
using UnityEngine;

namespace LMS.API
{
	public class FileAdapter : CrudAdapter<FileAdapter>
	{
		public override string entityResource => "File";
		public static string uploadResource => "FileManager";
		public static bool uploading { get; private set; }

		public static async void UploadFile(string content, TaskComplete<string> callback, FileQuery queryParams = null)
		{
			var jsonBytes = new System.Text.UTF8Encoding().GetBytes(content);
			var urlWithParams = AddQueryParameters($"{uploadResource}/UploadFile", queryParams);
			var fullUrl = $"{ConnectionStrings.url}/{urlWithParams}";
			//Debug.Log("uploading file with url = " + fullUrl);
			uploading = true;

			var uploadedFile = await RestUploader.UploadFile(fullUrl, jsonBytes);
			uploading = false;
			
			callback.Invoke(uploadedFile);
		}

		public static async void UploadFile(byte[] content, TaskComplete<string> callback, FileQuery queryParams = null, bool isBundle = true)
		{
			var urlWithParams = AddQueryParameters($"{uploadResource}/UploadFile", queryParams);
			var fullUrl = $"{ConnectionStrings.GetDeploymentURL()}/{urlWithParams}";
			//Debug.Log("uploading file with url = " + fullUrl);
			uploading = true;

			var uploadedFile = await RestUploader.UploadFileWithProgress(fullUrl, content, isBundle);
			uploading = false;

			callback.Invoke(uploadedFile);
		}

		public void DownloadFileStream<T>(TaskComplete<T> callback, FileQuery queryParams, LMSAssetType assetType = LMSAssetType.AssetBundle) where T : class
		{
			var urlWithParams = $"{entityResource}/GetStreamFile/" + queryParams.container + "/" + queryParams.file;

			var isBundle = typeof(T) == typeof(AssetBundle);
			if (isBundle)
			{
				// TODO: Use fileName to fix the cache issue
				var fileName = queryParams.file;
				var fullURL = ConnectionStrings.url + "/" + urlWithParams;
				AssetLoader.instance.GetAssetBundle(fullURL, queryParams.bundleID, result => callback(result as T), assetType);
			}
			else 
			{
				var headers = new[] { new Header("Accept", "application/octet-stream") };
				var getRequest = RestClient.GetAsyncStream<T>(urlWithParams, authToken, headers, false);
				AwaitResult(getRequest, callback);
			}
		}

		public void DownloadFileAction<T>(Action<T> callback, FileQuery queryParams, LMSAssetType assetType = LMSAssetType.AssetBundle) where T : class
		{
			var urlWithParams = $"{entityResource}/GetStreamFile/" + queryParams.container + "/" + queryParams.file;

			var isBundle = typeof(T) == typeof(AssetBundle);
			if (isBundle)
			{
				// TODO: Use fileName to fix the cache issue
				var fileName = queryParams.file;
				var fullURL = ConnectionStrings.url + "/" + urlWithParams;
				AssetLoader.instance.GetAssetBundle(fullURL, queryParams.bundleID, result => callback(result as T), assetType);
			}
			else
			{
				var headers = new[] { new Header("Accept", "application/octet-stream") };
				StartCoroutine(RestClientWebGL.GetAsync<T>(urlWithParams, authToken, callback, headers));
			}
		}

		public void DownloadFile<T>(TaskComplete<T> callback, FileQuery queryParams, LMSAssetType assetType = LMSAssetType.AssetBundle) where T : class
		{
			var urlWithParams = "File/GetStreamFile/assets/" + queryParams.file;

			// TODO: Use fileName to fix the cache issue
			var fileName = queryParams.file;
			var fullURL = ConnectionStrings.url + "/" + urlWithParams;
			AssetLoader.instance.GetAssetBundle(fullURL, queryParams.bundleID, result => callback(result as T), assetType);
		}

		public void DownloadFile(TaskComplete<byte[]> callback, FileQuery queryParams, LMSAssetType assetType = LMSAssetType.AssetBundle)
		{
			var urlWithParams = "File/GetStreamFile/assets/" + queryParams.file;

			var headers = new[] { new Header("Accept", "application/octet-stream") };
			var getRequest = RestClient.GetStreamAsync(urlWithParams, authToken, headers, false);
			AwaitResult(getRequest, callback);
		}
	}
}

public enum LMSAssetType
{
	AssetBundle,
	SceneConfig,
	Scenario,
	Resource,
	OutagePlan,
	ComparativeOutagePlan,
	UnityPackage
}

