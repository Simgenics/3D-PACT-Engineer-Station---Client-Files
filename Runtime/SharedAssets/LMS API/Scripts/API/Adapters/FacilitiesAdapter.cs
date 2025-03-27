using LMS.Models;
using System;
using System.Collections;

namespace LMS.API
{
	public class FacilitiesAdapter : CrudAdapter<FacilitiesAdapter>
	{
		public override string entityResource => "api/services/app/Facilities/";

		public void GetAllTree(TaskComplete<PagedResultDtoOfFacilityDto> callback, FacilityQuery queryParams)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}GetAllTree", queryParams);
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfFacilityDto>(urlWithParams, authToken), callback);
		}

		public IEnumerator GetAllFacilities(Action<PagedResultDtoOfFacilityDto> callback, FacilityQuery queryParams)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}GetAllTree", queryParams);
			yield return StartCoroutine(RestClientWebGL.GetAsync<PagedResultDtoOfFacilityDto>(urlWithParams, authToken, callback));
		}

		public void GetFacilityFoViewById(TaskComplete<ResultDtoOfFacilityDto> callback, int companyId)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}GetFacilityFoView?id={companyId}", callback);
			AwaitResult(RestClient.GetAsync<ResultDtoOfFacilityDto>(urlWithParams, authToken), callback);
		}
	}
}
