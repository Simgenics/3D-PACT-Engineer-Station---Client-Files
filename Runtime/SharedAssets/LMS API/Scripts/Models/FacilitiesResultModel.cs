using System;
using System.Collections.Generic;

namespace LMS.Models
{
	public class PagedResultDtoOfPagedFacilityResult
	{
		public int totalCount;
		public List<PagedResultDtoOfFacilityDto> items;
	}

	public class PagedResultDtoOfFacilityDto
	{
		public int totalCount;
		public List<FacilityDto> items;
	}

	public class ResultDtoOfFacilityDto
	{
		public FacilityDto dto;
		public string creatorUser;
		public string lastModifiedUser;
	}

	public class FacilityDto : EntityDto
	{
		public int companyId;
		public CompanyDto company;
		public int? assetBundleId;
		public string description;
		public string address;
		public string coordinates;
		public string contactNumber;
		public string emailAddress;
		public bool activeStatus;
		public bool status;
		public string letterHeadPath;
		public int? parentId;
		public List<FacilityDto> children;
		public List<AssetBundleDto> assetBundles;
		public bool companyActiveStatus;
		public List<ResourcesDto> resources;
		public DateTime? lastModificationTime;
		public Int64? lastModifierUserId;
		public Int64? creatorUserId;
	}

}
