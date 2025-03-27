using System;
using System.Collections.Generic;

namespace LMS.Models
{
	public class PagedResultDtoOfCompanyDto
	{
		public int totalCount;
		public List<CompanyDto> items;
	}

	public class ResultDtoOfCompanyDto
	{
		public CompanyViewDto company;
		public string creatorUser;
		public string lastModifierUser;
	}

	public class CompanyDto : EntityDto
	{
		public Guid? logo;
		public string logoFileName;
		public string description;
		public string address;
		public string coordinates;
		public string contactNumber;
		public string email;
		public bool status;
		public string licencingInformation;
		public Int64 organizationUnitId;
		public string logoPath;
		public int? parentId;
		public List<CompanyDto> children;
		public DateTime? lastModificationTime;
		public Int64? lastModifierUserId;
		public Int64? creatorUserId;
	}

	public class CompanyViewDto : EntityDto
	{
		public string description;
		public string address;
		public string coordinates;
		public string contactNumber;
		public string email;
		public bool status;
		public string licencingInformation;
		public int? organizationUnitId;
		public string logoPath;
		public int? parentId;
		public List<CompanyViewDto> children;
		public DateTime? lastModificationTime;
		public int? lastModifierUserId;
		public string dtoCreationTime;
		public int? creatorUserId;
	}
}
