#nullable enable

namespace LMS.Models
{
	public class AssetBundleQuery
	{
		public string? Filter { get; set; }
		public int? VersionState { get; set; }
		public int? OriginalAssetBundleId { get; set; }
		public string? VersionNumberFilter { get; set; }
		public string? NameFilter { get; set; }
		public int? StatusFilter { get; set; }
		public string? FacilityNameFilter { get; set; }
		public int? FacilityId { get; set; }
		public string? Sorting { get; set; }
		public int? SkipCount { get; set; }
		public int? MaxResultCount { get; set; }
	}
}
