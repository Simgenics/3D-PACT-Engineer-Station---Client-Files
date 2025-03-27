using System.Net.Http;

namespace LMS.API
{
	/// <summary>
	/// This class handles all the delete/deactivation methods for the HelperController.
	/// Please note that the query parameter 'DeleteEntinty' is incorrect on the server side.
	/// </summary>
	public class HelperAdapter : CrudAdapter<HelperAdapter>
	{
		public override string entityResource => "api/services/app/Helper";
		
		private const string DeleteEntity = "DeleteEntinty";

		public void DeleteOrDeactivateAssetBundle(int id, bool delete, TaskComplete<HttpResponseMessage> callback)
		{
			var url = $"{entityResource}/DeleteOrDeactivateAssetBundle?Id={id}&{DeleteEntity}={delete}";
			AwaitResult(RestClient.DeleteAsync(url, authToken), callback);
		}
		
		public void DeleteOrDeactivateScenario(int id, bool delete, TaskComplete<bool> callback)
		{
			var url = $"{entityResource}/DeleteOrDeactivateScenario?Id={id}&{DeleteEntity}={delete}";
			AwaitResult(RestClient.DeleteAsync<bool>(url, authToken), callback);
		}
	}
}
