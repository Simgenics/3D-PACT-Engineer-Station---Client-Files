using System;
using System.Net.Http;
using LMS.Models;
using UnityEditor;
using UnityEngine;

namespace LMS.API
{
	public sealed class AuthAdapter : CrudAdapter<AuthAdapter>
	{
		public static Action OnLoginSuccess = delegate {};
		public static Action OnLogOutSuccess = delegate {};

		public override string entityResource => "api/TokenAuth";

		public void AuthWithTenantName(object content, TaskComplete<AuthenticateResultModel> callback, TaskComplete<string> errorCallbcak = null)
		{
			var resource = $"{entityResource}/AuthenticateWithTenantName";
#if UNITY_WEBGL
			StartCoroutine(RestClientWebGL.PostAsync<AuthenticateResultModel>(resource, content, authToken, res => HandleAuthResponse(res, callback)));
#else
			AwaitResult(RestClient.PostAsync<AuthenticateResultModel>(resource, content), res => HandleAuthResponse(res, callback), ErrorCallback);
#endif
		}

		// TODO: Finalise and make sure it works with endpoint
		public void RefreshToken(string refreshToken, TaskComplete<RefreshTokenResult> callback, TaskComplete<string> errorCallbcak = null)
		{
			var resource = $"{entityResource}/RefreshToken?refreshToken={refreshToken}";
			Debug.Log("Trying to refresh token at " + resource);
			AwaitResult(RestClient.PostAsync<RefreshTokenResult>(resource, null), callback, ErrorCallback);
		}

		private static void HandleAuthResponse(AuthenticateResultModel response, TaskComplete<AuthenticateResultModel> cb)
		{
			if (response != null)
			{
				Debug.Log("Successfully Authenticated");
				OnLoginSuccess.Invoke();
				cb.Invoke(response);
			}
		}

		public void Logout(TaskComplete<bool> callback)
		{
			var resource = $"{entityResource}/LogOut";
#if UNITY_WEBGL
			// StartCoroutine(RestClientWebGL.PostAsync<AuthenticateResultModel>(resource, content, authToken, res => HandleAuthResponse(res, callback)));
#else
			AwaitResult(RestClient.GetAsync(resource), res => HandleLogoutResponse(res, callback));
#endif
		}

		private static void HandleLogoutResponse(HttpResponseMessage response, TaskComplete<bool> callback)
		{
			if (response.IsSuccessStatusCode)
			{
				Debug.Log("Successfully Logged Out");
#if UNITY_EDITOR
				EditorPrefs.DeleteKey("LMSToken");
				EditorPrefs.DeleteKey("LMSTokenExpiry");
#endif
				OnLogOutSuccess.Invoke();
				callback.Invoke(true);
			}
			else
			{
				Debug.LogWarning("Something went wrong logging out");
				callback.Invoke(false);
			}
		}
	}
}
