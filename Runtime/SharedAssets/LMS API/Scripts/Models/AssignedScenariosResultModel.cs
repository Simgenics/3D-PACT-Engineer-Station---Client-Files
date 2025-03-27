using System;
using System.Collections.Generic;

namespace LMS.Models
{
	public class PagedResultDtoOfAssignedScenarioDto
	{
		public int totalCount;
		public AssignedScenarioDto[] items;
	}

	public class AssignedScenarioDto
	{
		public string mode;
		public int numberOfAttemptsPermitted;
		public int? coolOfPeriod;
		public DateTime? deadline;
		public DateTime? creationTime;
		public long? authorId;
		public long? assigneeId;
		public int? scenarioId;
		public long? userGroupId;
		public ScenarioDto scenarioFk;
		public long? scenarioGroupId;
		public bool activeStatus;
		public bool scenarioActiveStatus;
		public long? companyId;
		public CompanyDto companyFk;
		public List<ResultDto> results;
		public string assignmentGroupGuid;
		public bool displayUnmoderated;
		public bool resultsIsModerated;
		public string resourceToken;
		public long id;
	}
	
	public class AssignedScenarioListDto
	{
		public string mode;
		public string scenarioName;
		public string facilityName;
		public string companyName;
		public string scenario;
		public int scenarioId;
		public string userName;
		public string userLastName;
		public string assetBundle;
		public long assignedScenarioId;
		public string facilityHeaderImage;
		public string[] resources;
	}

	public class GetAssignedScenarioForViewDto
	{
		public AssignedScenarioDto assignedScenario;
		public string author;
		public string assignee;
		public string scenarioName;
		public bool scenarioActiveStatus;
		public string scenarioGroupName;
		public string userGroupName;
		public bool activeStatus;
		public string groupName2;
	}
}
