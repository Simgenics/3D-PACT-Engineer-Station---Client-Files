using System.Collections.Generic;
using LMS.Models;

namespace LMS.API
{
	public class RolesAdapter : CrudAdapter<RolesAdapter>
	{
		public override string entityResource => "api/services/app/Role/GetRoles";
		
		public void GetRoles(GetRolesInput permissions, TaskComplete<ListResultDtoOfRoleListDto> callback)
		{
			AwaitResult(RestClient.PostAsync<ListResultDtoOfRoleListDto>(entityResource, permissions, authToken), callback);
		}
	}
	
	public class GetRolesInput
	{
		public List<string> Permissions { get; set; }
	}
}
