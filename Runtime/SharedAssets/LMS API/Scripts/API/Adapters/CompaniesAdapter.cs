using LMS.Models;
using System;
using System.Collections;

namespace LMS.API
{
	public sealed class CompaniesAdapter : CrudAdapter<CompaniesAdapter>
	{
		public override string entityResource => "api/services/app/Companies";

		public void GetTreeOfFacilitiesForUser(TaskComplete<PagedResultDtoOfCompanyDto> callback)
		{
			AwaitResult(RestClient.GetAsync<PagedResultDtoOfCompanyDto>(entityResource + "/GetAllTreeForUser", authToken), callback);
		}

		public void GetCompanyForViewById(TaskComplete<ResultDtoOfCompanyDto> callback, int companyId)
		{
			AwaitResult(RestClient.GetAsync<ResultDtoOfCompanyDto>($"{entityResource}/GetCompanyForView?id={companyId}", authToken), callback);
		}

		public IEnumerator GetCompanyById(Action<ResultDtoOfCompanyDto> callback, int companyId)
		{
			yield return StartCoroutine(RestClientWebGL.GetAsync<ResultDtoOfCompanyDto>($"{entityResource}/GetCompanyForView?id={companyId}", authToken, callback));
		}

		public IEnumerator GetFacilitiesForUser(Action<PagedResultDtoOfCompanyDto> callback)
		{
			yield return StartCoroutine(RestClientWebGL.GetAsync<PagedResultDtoOfCompanyDto>(entityResource + "/GetAllTreeForUser", authToken, callback));
		}
	}
}
