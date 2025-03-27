using System.Collections.Generic;

namespace LMS.Models
{
	public class ListResultDtoOfRoleListDto
	{
		public List<RoleListDto> items;
	}
	
	public class RoleListDto
	{
		public string name;
		public string displayName;
		public bool isStatic;
		public bool isDefault;
		public string creationTime;
		public int id;
	}
}
