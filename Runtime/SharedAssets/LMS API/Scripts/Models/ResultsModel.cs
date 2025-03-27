using System;

namespace LMS.Models
{
	public class ResultDto
	{
		public int? numberOfAttempts;
		public string resourcesAccessed;
		public string pdfResults;
		public int? stepsFailed;
		public int? stepsPassed;
		public int? score;
		public DateTime? endTime;
		public DateTime? startTime;
		public DateTime? deadline;
		public DateTime? dateAssigned;
		public string instructor;
		public string userSurname;
		public string userName;
		public string facilityName;
		public string scenarioName;
		public long? assigneeId;
		public long? authorId;
		public int? facilityId;
		public int? scenarioId;
		public long? assignedScenarioId;
		public string mode;
		public bool moderated;
		public int? parentId;
		public DateTime? lastModificationTime;
		public bool displayUnmoderated;
		public DateTime? executionDate;
		public long id;
	}
}
