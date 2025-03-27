#nullable enable
namespace LMS.Models
{
	public class ScenarioQuery
	{
		public string? Filter { get; set; }
		public string? NameFilter { get; set; }
		public string? AssetBundleDisplayPropertyFilter { get; set; }
		public string? DescriptionFilter { get; set; }
		public string? VersionNumberFilter { get; set; }
		public int? StatusFilter { get; set; }
		public int? FacilityId { get; set; }
		public int? OriginalScenarioId { get; set; }
		public string? Sorting { get; set; }
		public int? SkipCount { get; set; }
		public int? MaxResultCount { get; set; }
	}
	
	public class CreateOrEditScenarioQuery
	{
		public string name { get; set; }
		public string description { get; set; }
		public string sceneConfig { get; set; }
		public string? sceneConfigToken { get; set; }
		public string? versionNumber { get; set; }
		public int? allocatedAttemptsCount { get; set; }
		public StatusState status { get; set; }
		public bool? activeStatus { get; set; }
		public VersionState? versionState { get; set; }
		public int? originalScenarioId { get; set; }
		public int? previousScenarioId { get; set; }
		public int? facilityId { get; set; }
		public int? assetBundleId { get; set; }
		public int? id { get; set; }
	}
}