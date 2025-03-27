using System;
using LMS.Models;

namespace LMS.API
{
	public class ScenariosAdapter : CrudAdapter<ScenariosAdapter>
	{
		public override string entityResource => "api/services/app/Scenarios";
		
		public void GetScenarios(TaskComplete<PagedResultDtoOfGetScenarioForViewDto> callback, ScenarioQuery queryParams = null)
		{
			var urlWithParams = AddQueryParameters($"{entityResource}/GetAllCurrent", queryParams);
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfGetScenarioForViewDto>(urlWithParams, authToken), callback);
		}

		public void GetScenarioForView(int scenarioId, TaskComplete<GetScenarioForViewDto> callback)
		{
			AwaitResult(RestClient.GetAsync<GetScenarioForViewDto>($"{entityResource}/GetScenarioForView?id={scenarioId}", authToken), callback);
		}
		
		public void CreateOrEditScenario(TaskComplete<bool> callback, CreateOrEditScenarioQuery query)
		{
			var urlWithParams = $"{entityResource}/CreateOrEdit";
			AwaitResult(RestClient.PostAsync<bool>(urlWithParams, query, authToken), callback);
		}
	}
}
