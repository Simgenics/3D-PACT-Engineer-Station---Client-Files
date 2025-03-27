using LMS.Models;

namespace LMS.API
{
	public class SessionAdapter : CrudAdapter<SessionAdapter>
	{
		public override string entityResource => "api/services/app/Session/GetCurrentLoginInformations";
		
		public void GetCurrentLoginInformations(TaskComplete<GetCurrentLoginInformationsOutput> callback)
		{
			AwaitResult(RestClient.GetAsync<GetCurrentLoginInformationsOutput>(entityResource, authToken), callback);
		}
	}
}
