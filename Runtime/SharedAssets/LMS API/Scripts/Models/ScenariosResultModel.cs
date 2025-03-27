namespace LMS.Models
{
	public class PagedResultDtoOfGetScenarioForViewDto : EntityCollection<GetScenarioForViewDto> {}

	public class GetScenarioForViewDto
	{
		public ScenarioDto scenario;
		public int? companyId;
		public string creatorUser;
		public string lastModifiedUser;
		public string assetBundleDisplayProperty;
		public string assetBundle;
	}
}
