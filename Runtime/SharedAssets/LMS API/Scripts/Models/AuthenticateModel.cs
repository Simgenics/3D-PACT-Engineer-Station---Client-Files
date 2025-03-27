namespace LMS.Models
{
	public class AuthenticateModel
	{
		public string userNameOrEmailAddress;
		public string password;
		public string twoFactorVerificationCode;
		public bool rememberClient;
		public string twoFactorRememberClientToken;
		public bool? singleSignIn;
		public string returnUrl;
		public string tenantName;
		public string captchaResponse;
	}
}
