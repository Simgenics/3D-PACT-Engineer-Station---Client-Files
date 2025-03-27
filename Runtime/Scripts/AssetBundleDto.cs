using System;

namespace LMS.Models
{
	public class AssetBundleDto : EntityDto
	{
		#nullable enable
		public string? assetFile;
		public string? assetFileFileName;
		public string? versionNumber;
		public string? assetPath;
		public bool facilityActiveStatus;
		public int? facilityId;
		public VersionState versionState;
		public int? originalAssetBundleId;
		public int? previousAssetBundleId;
		public DateTime? lastModificationTime;
		public long? lastModifierUserId;
		public long? creatorUserId;
		public StatusState status;
		public bool activeStatus;
	}
}
