using LMS.Models;

namespace LMS.API
{
	public class AssignedScenariosAdapter : CrudAdapter<AssignedScenariosAdapter>
	{
		public override string entityResource => "api/services/app/AssignedScenarios";
		
		public void GetAssignedScenario(int id, TaskComplete<PagedResultDtoOfAssignedScenarioDto> callback)
		{
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfAssignedScenarioDto>($"{entityResource}/GetAssignedScenarios?id={id}", authToken), callback);
		}

		public void GetAllAssignedScenarios(TaskComplete<AssignedScenarioListDto> callback)
		{
			AwaitResult(RestClient.GetAsync<AssignedScenarioListDto>($"{entityResource}/GetAssignedAllScenarios", authToken), callback);
		}

		public void GetAssignedScenarioForView(int id, TaskComplete<GetAssignedScenarioForViewDto> callback)
		{
			AwaitResult(RestClient.GetAsync<GetAssignedScenarioForViewDto>($"{entityResource}/GetAssignedScenarioForView?id={id}", authToken), callback);
		}
	}
}
