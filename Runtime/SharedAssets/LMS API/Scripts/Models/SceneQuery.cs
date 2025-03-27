#nullable enable
namespace LMS.Models
{
	public class SceneQuery
	{
		public string? Filter { get; set; }
		public string? ConfigFilter { get; set; }
		public string? NameFilter { get; set; }
		public string? VersionNumberFilter { get; set; }
		public string? MaxDateFilter { get; set; }
		public string? MinDateFilter { get; set; }
		public int? StatusFilter { get; set; }
		public int? FacilityId { get; set; }
		public string? AssetBundleDisplayPropertyFilter { get; set; }
		public int? OriginalSceneId { get; set; }
		public string? Sorting { get; set; }
		public int? SkipCount { get; set; }
		public int? MaxResultCount { get; set; }
	}

	public class SceneCreateEditQuery
	{
		public string? config { get; set; }
		public string? configPath { get; set; }
		public string? configToken { get; set; }
		public string versionNumber { get; set; }
		public string name { get; set; }
		public int facilityId { get; set; }
		public bool? status { get; set; }
		public bool? activeStatus { get; set; }
		public int assetBundleId { get; set; }
		public int? versionState { get; set; }
		public int? originalSceneId { get; set; }
		public int? previousSceneId { get; set; }
		public int? id { get; set; }
	}
}
