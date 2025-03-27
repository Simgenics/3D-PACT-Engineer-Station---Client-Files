using SharedAssets;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LMS.API
{
	/// <summary>
	/// A component for specifying a base URL for server call operations.
	/// </summary>
	[AddComponentMenu("API/Connection Strings")]
	public sealed class ConnectionStrings : MonoBehaviour
	{
		public static ConnectionStrings instance;

		#region PROPERTIES
		public static string devUrl { get { return instance?.m_DevUrl; } set { instance.m_DevUrl = value; } }
		public static string prodUrl { get { return instance?.m_ProdUrl; } set { instance.m_ProdUrl = value; } }
		public static string apiRoute { get { return instance?.m_ApiRoute; } set { instance.m_ApiRoute = value; } }
		static string TENANT_FILE_NAME = "/TenantName.txt";
		public static string url => rawUrl + (!string.IsNullOrEmpty(apiRoute) ? $"/{apiRoute}" : string.Empty);

		public static string rawUrl
		{
			get
			{
				if (devUrl.EndsWith("/") || prodUrl.EndsWith("/") || apiRoute.EndsWith("/"))
				{
					Debug.LogError("Final forward slash should not be included on any specified URLs. These will be added automitically when concatenating.");
				}
				return DeploymentURL;
			}
		}

		public static string Tenant => instance.m_Tenant;
		public static string DeploymentURL => instance.m_DeploymentURL;
		#endregion


		#region VARIABLES
		[SerializeField]
		private string m_DevUrl = "https://<resource-name>.azurewebsites.net";

		[SerializeField]
		private string m_ProdUrl = "https://<resource-name>.azurewebsites.net";

		[SerializeField, Tooltip(
			 "API route that is appended to the base URL. Include this if all API endpoints are note located directly after the base URL." +
			 "That is: {{baseUrl}}/{{apiRoute}}/{{resource}}" +
			 "For example: https://<resource-name>.azurewebsites.net/api/{{resource}}")]
		private string m_ApiRoute = "api";

		[SerializeField]
		private string m_Tenant;
		[SerializeField]
		private string m_DeploymentURL;
		#endregion

		public static void SetDeploymentURL(string url)
		{
			instance.m_DeploymentURL = url;
		}

		public static string GetDeploymentURL()
		{
			if (File.Exists(Application.streamingAssetsPath + TENANT_FILE_NAME))
			{
				var lines = File.ReadAllLines(Application.streamingAssetsPath + TENANT_FILE_NAME);
				if (lines.Length > 1)
					return lines[1];
			}
			return null;
		}

		public void Awake()
		{
			if (instance == null)
			{
				instance = this;
				if (!string.IsNullOrEmpty(instance.m_DeploymentURL)) return;
				if (File.Exists(Application.streamingAssetsPath + TENANT_FILE_NAME))
				{
					var lines = File.ReadAllLines(Application.streamingAssetsPath + TENANT_FILE_NAME);
					if (lines.Length > 1)
						SetDeploymentURL(lines[1]);
				}
			}
			
		}
	}
}
