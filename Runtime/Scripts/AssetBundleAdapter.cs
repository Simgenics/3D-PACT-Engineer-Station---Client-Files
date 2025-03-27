using LMS.Models;
using System;
using System.Collections;

namespace LMS.API
{
	public class AssetBundleAdapter : CrudAdapter<AssetBundleAdapter>
	{
		public override string entityResource => "api/services/app/AssetBundles";
		
		public void GetAllBundles(TaskComplete<PagedResultDtoOfGetAssetBundleForViewDto> callback, AssetBundleQuery queryParams = null)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}/GetAllCurrent", queryParams);
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfGetAssetBundleForViewDto>(urlWithParams, authToken), callback);
		}

		public void GetAssetBundleForView(TaskComplete<GetAssetBundleForViewDto> callback, int id)
		{
			var url = $"{entityResource}/GetAssetBundleForView?id={id}";
			AwaitResult(RestClient.GetAsync<GetAssetBundleForViewDto>(url, authToken), callback);
		}

		public IEnumerator GetAssetBundle(Action<GetAssetBundleForViewDto> callback, int id)
		{
			var urlWithParams = $"{entityResource}/GetAssetBundleForView?id={id}";
			yield return StartCoroutine(RestClientWebGL.GetAsync<GetAssetBundleForViewDto>(urlWithParams, authToken, callback));
		}

		public IEnumerator GetAllAssetBundles(Action<PagedResultDtoOfGetAssetBundleForViewDto> callback, AssetBundleQuery queryParams = null)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}/GetAllCurrent", queryParams);
			yield return StartCoroutine(RestClientWebGL.GetAsync<PagedResultDtoOfGetAssetBundleForViewDto>(urlWithParams, authToken, callback));
		}

		public void CreateOrEditAssetBundle(TaskComplete<bool> callback, CreateOrEditAssetBundleDto query)
		{
			var urlWithParams = $"{entityResource}/CreateOrEdit";
			AwaitResult(RestClient.PostAsync<bool>(urlWithParams, query, authToken), callback);
		}
	}
}
