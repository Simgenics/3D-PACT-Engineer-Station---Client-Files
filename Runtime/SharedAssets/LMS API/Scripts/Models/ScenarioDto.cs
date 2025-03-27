using System;

namespace LMS.Models
{
	public class ScenarioDto : EntityDto
	{
		public string description;
		public string sceneConfig;
		public string sceneConfigFileName;
		public string versionNumber;
		public int allocatedAttemptsCount;
		public int versionState;
		public int? originalScenarioId;
		public int? previousScenarioId;
		public int? facilityId;
		public FacilityDto facilityFk;
		public int? assetBundleId;
		public DateTime? lastModificationTime;
		public long? lastModifierUserId;
		public long? creatorUserId;
		public StatusState status;
		public bool activeStatus;
	}
}
