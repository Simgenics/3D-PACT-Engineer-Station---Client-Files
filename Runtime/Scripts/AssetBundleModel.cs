namespace LMS.Models
{
	public class PagedResultDtoOfGetAssetBundleForViewDto : EntityCollection<GetAssetBundleForViewDto> 	{}

	public class GetAssetBundleForViewDto
	{
		public AssetBundleDto assetBundle;
		public string facilityName;
		public string creatorUser;
		public string lastModifiedUser;
	}

	public enum VersionState
	{
		Original,
		Versioned
	}

	public class CreateOrEditAssetBundleDto
	{
		public string? versionNumber;
		public string? name;
		public string? assetPath;
		public bool activeStatus;
		public StatusState status;
		public VersionState versionState;
		public int? originalAssetBundleId;
		public int? previousAssetBundleId;
		public int? facilityId;
		public int? id;
	}
}
