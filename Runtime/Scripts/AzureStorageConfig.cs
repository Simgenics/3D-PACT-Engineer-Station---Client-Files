using UnityEngine;

[CreateAssetMenu(fileName = "AzureStorageConfig", menuName = "ScriptableObjects/AzureStorageConfig")]
public class AzureStorageConfig : ScriptableObject
{
	[Tooltip("Name of the Azure Storage account")]
	public string storageAccount;
	[Tooltip("Name of target blob container in Storage account")]
	public string storageContainer;

	public string GetAccountURL => $"https://{storageAccount}.{BlobSuffix}/{storageContainer}/";
	private const string BlobSuffix = "blob.core.windows.net";
}
