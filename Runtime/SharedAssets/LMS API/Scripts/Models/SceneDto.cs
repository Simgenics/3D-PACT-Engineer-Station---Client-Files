#nullable enable

namespace LMS.Models
{
	public class SceneDto : EntityDto
	{
		public string? config;
		public string? configFileName;
		public string? configPath;
		public string? versionNumber;
		public string? scenarioName;
		public int assetBundleId;
		public AssetBundleDto assetBundleFk;
		public int facilityId;
		public bool facilityActiveStatus;
		public VersionState versionState;
		public int? originalSceneId;
		public int? previousSceneId;
		public string? lastModificationTime;
		public long? lastModifierUserId;
		public long? creatorUserId;
		public bool status;
		public bool activeStatus;
	}
}
