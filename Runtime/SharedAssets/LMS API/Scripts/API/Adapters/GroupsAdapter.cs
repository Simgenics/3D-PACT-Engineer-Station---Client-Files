using LMS.Models;

namespace LMS.API
{
	public class GroupsAdapter : CrudAdapter<GroupsAdapter>
	{
		public override string entityResource => "api/services/app/Groups/GetAll";
		
		// Gets asset bundles associated with company
		public void GetAll(int groupType, int orgUnitId, TaskComplete<PagedResultDtoOfGetGroupForViewDto> callback)
		{
			var urlParams = $"{entityResource}?GroupType={groupType}{(orgUnitId > 0 ? $"&OrganizationUnitId={orgUnitId}" : "")}";
			var getRequest = RestClient.GetAsync<PagedResultDtoOfGetGroupForViewDto>(urlParams, authToken);
			AwaitResult(getRequest, callback);
		}
	}
}
