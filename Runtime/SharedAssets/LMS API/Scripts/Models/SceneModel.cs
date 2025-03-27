namespace LMS.Models
{
	public class PagedResultDtoOfGetSceneForViewDto : EntityCollection<GetSceneForViewDto> {}

	public class GetSceneForViewDto
	{
		public SceneDto scene { get; set; }
		public string assetBundleDisplayProperty { get; set; }
		public string creatorUser { get; set; }
		public string lastModifiedUser { get; set; }
	}
}