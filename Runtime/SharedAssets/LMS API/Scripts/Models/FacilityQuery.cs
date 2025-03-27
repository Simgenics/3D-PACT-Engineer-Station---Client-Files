#nullable enable

namespace LMS.Models
{
	public class FacilityQuery
	{
		public string? Filter { get; set; }
		public string? NameFilter { get; set; }
		public string? DescriptionFilter { get; set; }
		public string? AddressFilter { get; set; }
		public string? CoordinatesFilter { get; set; }
		public string? ContactNumberFilter { get; set; }
		public string? EmailAddressFilter { get; set; }
		public int? OrganizationalUnit { get; set; }
		public int? StatusFilter { get; set; }
		public string? Sorting { get; set; }
		public int? SkipCount { get; set; }
		public int? MaxResultCount { get; set; }
	}
}
