using System.Net.Http;
using LMS.Models;

namespace LMS.API
{
	public class ScenesAdapter : CrudAdapter<ScenesAdapter>
	{
		public override string entityResource => "api/services/app/Scenes";
		
		public void GetScenesForFacility(TaskComplete<PagedResultDtoOfGetSceneForViewDto> callback, SceneQuery queryParams = null)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}/GetAllForFacility", queryParams);
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfGetSceneForViewDto>(urlWithParams, authToken), callback);
		}
		
		public void GetAllScenes(TaskComplete<PagedResultDtoOfGetSceneForViewDto> callback, SceneQuery queryParams = null)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}/GetAll", queryParams);
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfGetSceneForViewDto>(urlWithParams, authToken), callback);
		}
		
		public void GetSceneForView(int id, TaskComplete<GetSceneForViewDto> callback)
		{
			var urlWithParams = $"{entityResource}/GetSceneForView?id={id}";
			AwaitResult(RestClient.GetAsync<GetSceneForViewDto>(urlWithParams, authToken), callback);
		}

		public void DeleteScene(int id, TaskComplete<HttpResponseMessage> callback)
		{
			var urlWithParams = $"{entityResource}/Delete?Id={id}";
			AwaitResult(RestClient.DeleteAsync(urlWithParams, authToken), callback);
		}

		public void CreateOrEditScene(TaskComplete<bool> callback, SceneCreateEditQuery query)
		{
			var urlWithParams = $"{entityResource}/CreateOrEdit";
			AwaitResult(RestClient.PostAsync<bool>(urlWithParams, query, authToken), callback);
		}
	}
}
