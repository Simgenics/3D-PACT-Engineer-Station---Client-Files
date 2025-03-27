using System.Collections.Generic;

namespace LMS.Models
{
	public class GetCurrentLoginInformationsOutput
	{
	    public UserLoginInfoDto User { get; set; }
	    public UserLoginInfoDto ImpersonatorUser { get; set; }
	    public TenantLoginInfoDto Tenant { get; set; }
	    public TenantLoginInfoDto ImpersonatorTenant { get; set; }
	    public ApplicationInfoDto Application { get; set; }
	    public UiCustomizationSettingsDto Theme { get; set; }
	}

	public class UserLoginInfoDto
	{
	    public string Name { get; set; }
	    public string Surname { get; set; }
	    public string UserName { get; set; }
	    public string EmailAddress { get; set; }
	    public string ProfilePictureId { get; set; }
	    public long Id { get; set; }
	}

	public class TenantLoginInfoDto
	{
	    public string TenancyName { get; set; }
	    public string Name { get; set; }
	    public string LogoId { get; set; }
	    public string LogoFileType { get; set; }
	    public string CustomCssId { get; set; }
	    public string SubscriptionEndDateUtc { get; set; }
	    public bool IsInTrialPeriod { get; set; }
	    public int SubscriptionPaymentType { get; set; }
	    public EditionInfoDto Edition { get; set; }
	    public List<NameValueDto> FeatureValues { get; set; }
	    public string CreationTime { get; set; }
	    public int PaymentPeriodType { get; set; }
	    public string SubscriptionDateString { get; set; }
	    public string CreationTimeString { get; set; }
	    public int Id { get; set; }
	}

	public class EditionInfoDto
	{
	    public string DisplayName { get; set; }
	    public int? TrialDayCount { get; set; }
	    public double? MonthlyPrice { get; set; }
	    public double? AnnualPrice { get; set; }
	    public bool IsHighestEdition { get; set; }
	    public bool IsFree { get; set; }
	    public int Id { get; set; }
	}

	public class NameValueDto
	{
	    public string Name { get; set; }
	    public string Value { get; set; }
	}

	public class ApplicationInfoDto
	{
	    public string Version { get; set; }
	    public string ReleaseDate { get; set; }
	    public string Currency { get; set; }
	    public string CurrencySign { get; set; }
	    public bool AllowTenantsToChangeEmailSettings { get; set; }
	    public bool UserDelegationIsEnabled { get; set; }
	    public double TwoFactorCodeExpireSeconds { get; set; }
	    public object Features { get; set; }
	}
	public class UiCustomizationSettingsDto
	{
		public ThemeSettingsDto BaseSettings { get; set; }
		public bool IsLeftMenuUsed { get; set; }
		public bool IsTopMenuUsed { get; set; }
		public bool IsTabMenuUsed { get; set; }
		public bool AllowMenuScroll { get; set; }
	}

	public class ThemeSettingsDto
	{
		public string Theme { get; set; }
		public ThemeLayoutSettingsDto Layout { get; set; }
		public ThemeHeaderSettingsDto Header { get; set; }
		public ThemeSubHeaderSettingsDto SubHeader { get; set; }
		public ThemeMenuSettingsDto Menu { get; set; }
		public ThemeFooterSettingsDto Footer { get; set; }
	}

	public class ThemeLayoutSettingsDto
	{
		public string LayoutType { get; set; }
		public bool DarkMode { get; set; }
	}

	public class ThemeHeaderSettingsDto
	{
		public bool DesktopFixedHeader { get; set; }
		public bool MobileFixedHeader { get; set; }
		public string MinimizeDesktopHeaderType { get; set; }
	}

	public class ThemeSubHeaderSettingsDto
	{
		public bool FixedSubHeader { get; set; }
		public string SubheaderStyle { get; set; }
		public int SubheaderSize { get; set; }
		public string TitleStyle { get; set; }
		public string ContainerStyle { get; set; }
		public string SubContainerStyle { get; set; }
	}

	public class ThemeMenuSettingsDto
	{
		public string Position { get; set; }
		public string AsideSkin { get; set; }
		public bool FixedAside { get; set; }
		public bool AllowAsideMinimizing { get; set; }
		public bool DefaultMinimizedAside { get; set; }
		public string SubmenuToggle { get; set; }
		public bool SearchActive { get; set; }
		public bool EnableSecondary { get; set; }
		public bool HoverableAside { get; set; }
	}

	public class ThemeFooterSettingsDto
	{
		public bool FixedFooter { get; set; }
	}

}
