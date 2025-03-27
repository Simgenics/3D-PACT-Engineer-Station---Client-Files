using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMS.API;
using LMS.Models;
using SharedAssets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LMS.UI
{
	public enum CreationType
	{
		New,
		Import
	}

	public class UICompanies : SingletonBehaviour<UICompanies>
	{
		[SerializeField]
		private TMPro.TextMeshProUGUI creationTypeText;

		[Header("Buttons")]
		[SerializeField]
		private Button nextButton;

		private List<FacilityDto> listFacilities = new List<FacilityDto>();
		private List<CompanyDto> listCompanies = new List<CompanyDto>();
		private List<AssetBundleDto> listAssets = new List<AssetBundleDto>();


		public Button btnClearCache;
		public Button btnEdit;
		public Button btnDelete;
		public Button btnClose;

		private CompanyDto selectedCompany;
		private FacilityDto selectedFacility;
		private AssetBundleDto selectedAsset;
		private SceneDto selectedScene;
		private ScenarioDto selectedScenario;

		public CompanyDto SelectedCompany => selectedCompany;
		public FacilityDto SelectedFacility => selectedFacility;
		public AssetBundleDto SelectedAsset => selectedAsset;
		public SceneDto SelectedScene => selectedScene;
		public ScenarioDto SelectedScenario => selectedScenario;

		public Button NextButton => nextButton;

		public static Action OnDownloadingEnded = delegate () { };

		private CreationType creationType;

		public CreationType CreationType => creationType;

		public void SetSelectedAsset(AssetBundleDto asset)
		{
			//selectedAsset = asset;
			StartCoroutine(AssetBundleAdapter.instance.GetAssetBundle((result) =>
			{
				selectedAsset = result.assetBundle;
				LoadAssetBundle();
			}, 13));
		}

		public void Initialize()
		{
			Clear();
			GetCompanies();
		}

		public void GetCompanies()
		{
			StartCoroutine(CompaniesAdapter.instance.GetFacilitiesForUser(companiesResult =>
			{
				if (companiesResult != null)
				{
					SetCompanies(companiesResult.items);
				}
			}));
		}

		public static void ClearCache()
		{
			AssetBundle.UnloadAllAssetBundles(true);
			UnityWebRequest.ClearCookieCache();
			Caching.ClearCache();
			Debug.Log("Cleared cache!");
		}

		//clear all entries
		public void Clear()
		{
			selectedCompany = null;
			selectedFacility = null;
			selectedAsset = null;
		}

		#region SET RESULTS
		public void SetCompanies(List<CompanyDto> companies)
		{
			foreach (var dto in companies)
			{
			}
		}

		public void SetFacilities(List<FacilityDto> facilities)
		{
			foreach (var dto in facilities)
			{
			}
		}

		public void SetBundles(List<GetAssetBundleForViewDto> dtos)
		{
			foreach (var dto in dtos)
			{
			}
		}

		public void CompanySelected(CompanyDto dto)
		{

		}

		public void FacilitySelected(FacilityDto dto)
		{

		}

		public void AssetBundleSelected(AssetBundleDto dto)
		{

		}

		private void OnCompanySelected(CompanyDto dto)
		{
			selectedCompany = dto;

			// API fetch facilities
			var query = new FacilityQuery() { OrganizationalUnit = (int)dto.organizationUnitId };
			StartCoroutine(FacilitiesAdapter.instance.GetAllFacilities(result =>
			{
				SetFacilities(result.items.Where(i => i.companyId == dto.id).ToList());
			}, new FacilityQuery()));
		}

		private void OnFacilitySelected(FacilityDto dto)
		{
			selectedFacility = dto;

			// API fetch assets
			StartCoroutine(AssetBundleAdapter.instance.GetAllAssetBundles((result) =>
			{
				var bundles = new List<AssetBundleDto>();
				SetBundles(result.items);
			}, new AssetBundleQuery() { FacilityId = selectedFacility.id, MaxResultCount = int.MaxValue }));
		}
		#endregion

		/// <summary>
		/// Handles the click event for the Load button
		/// This should kick off a process to load an asset bundle, scene config, or scenario
		/// </summary>
		public void LoadAssetBundle()
		{
			LoadAssetBundleRoutine(selectedAsset);
		}

		/// <summary>
		/// Routine to load a selected asset bundle
		/// </summary>
		private void LoadAssetBundleRoutine(AssetBundleDto chosenAsset, LMSAssetType assetType = LMSAssetType.AssetBundle)
		{
			var fileQuery = new FileQuery() { container = $"assets", file = chosenAsset.assetPath, bundleID = (uint)chosenAsset.id };
			FileAdapter.instance.DownloadFileAction<AssetBundle>(result =>
			{
				Debug.Log("got file stream!");
				AssetLoader.instance.LoadAssetBundleFromDto(chosenAsset, result, assetType);
			}, fileQuery, LMSAssetType.AssetBundle);
		}

		void AllDownloadsComplete()
		{
			OnDownloadingEnded?.Invoke();
		}
	}
}