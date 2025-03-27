using System;
using System.Collections.Generic;

namespace LMS.Models
{
	public class PagedResultDtoOfGetGroupForViewDto
	{
		public int totalCount { get; set; }
		public List<GetGroupForViewDto> items { get; set; }
	}

	public class GetGroupForViewDto
	{
		public GroupDto group { get; set; }
		public long companyId { get; set; }
	}

	public class GroupDto
	{
		public string name { get; set; }
		public DateTime creationTime { get; set; }
		public long? creatorUserId { get; set; }
		public string creatorName { get; set; }
		public long organizationUnitId { get; set; }
		public string companyName { get; set; }
		public int? companyId { get; set; }
		public GroupType groupType { get; set; }
		public long id { get; set; }
	}

	public enum GroupType
	{
		Unknown = 0,
		Other = 1
	}
}
