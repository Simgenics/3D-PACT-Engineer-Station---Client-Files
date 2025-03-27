using ImportExportScene;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectSpawner", menuName = "ScriptableObjects/ObjectSpawner", order = 1)]
public class ObjectSpawner : ScriptableObject
{
	[Header("Create menu objects")]
    [SerializeField]
    private SpawnableObject[] spawnableObjects;

	public SpawnableObject[] SpawnableObjects => spawnableObjects;

	[Header("Bot models")]
	//in order of Ethnicity enum
	[SerializeField]
	private GameObject[] femaleNPCs;
	[SerializeField]
	private GameObject[] maleNPCs;

	public GameObject GetNPC(int gender, int ethnicity)
	{
		var models = (int)gender == 1 ? femaleNPCs : maleNPCs;
		return models[ethnicity];
	}

	[Header("Environment objects")]
	[SerializeField]
	private GameObject mountain;
	[SerializeField]
	private GameObject ocean;
}

[System.Serializable]
public class SpawnableObject 
{
	public string Name;
	public Sprite Icon;
	public GameObject Prefab;
	public InstanceType Type;
	public EffectType EffectType;
}