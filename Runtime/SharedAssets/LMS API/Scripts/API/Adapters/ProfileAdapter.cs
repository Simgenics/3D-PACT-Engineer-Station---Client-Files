namespace LMS.API
{
	public class ProfileAdapter : CrudAdapter<ProfileAdapter>
	{
		public override string entityResource => "api/services/app/Profile/GetProfilePicture";
		
		public void GetProfilePicture(TaskComplete<ProfilePictureResponse> callback)
		{
			AwaitResult(RestClient.GetAsync<ProfilePictureResponse>(entityResource, authToken), callback);
		}
	}

	public class ProfilePictureResponse
	{
		public string profilePicture { get; set; }
	}
}
