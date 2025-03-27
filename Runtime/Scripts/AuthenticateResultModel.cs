namespace LMS.Models
{
	public class AuthenticateResultModel
	{
		public string accessToken;
		public string encryptedAccessToken;
		public int expireInSeconds;
		public bool shouldResetPassword;
		public string passwordResetCode;
		public long userId;
		public bool requiresTwoFactorVerification;
		public string[] twoFactorAuthProviders;
		public string twoFactorRememberClientToken;
		public string returnUrl;
		public string refreshToken;
		public int refreshTokenExpireInSeconds;
	}
	
	public class RefreshTokenResult
	{
		public string accessToken { get; set; }
		public string encryptedAccessToken { get; set; }
		public int expireInSeconds { get; set; }
	}
}
