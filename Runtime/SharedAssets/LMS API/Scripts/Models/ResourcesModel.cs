using System;

namespace LMS.Models
{
    public class PagedResultDtoOfGetResourceForViewDto : EntityCollection<GetResourceForViewDto> { }

    public class GetResourceForViewDto
	{
        public ResourcesDto resource;
        public string creatorUser;
        public string lastModifiedUser;
    }

    public class ResourcesDto : EntityDto
    {
        #nullable enable
        public string? description;
        public string versionNumber;
        public string? filePath;
        public int? facilityId;
        public string? authorName;
        public bool? status;
        public bool? activeStatus;
        public int? versionState;
        public int? originalResourceId;
        public int? previousResourceId;
        public DateTime? lastModificationTime;
        public int? lastModifierUserId;
        public int? creatorUserId;
        public string accessToken;
        public string Id;
        public string Name;
        public Uri AccessToken;
        public string OrganizationId;
        public string FacilityId;
        public DateTimeOffset? ExpiresOn;
    }
}
