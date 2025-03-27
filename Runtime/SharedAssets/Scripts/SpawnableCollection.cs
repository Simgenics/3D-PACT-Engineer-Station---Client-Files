using System.Linq;
using ImportExportScene;
using UnityEngine;

#if SHARED_ASSETS
public class SpawnableCollection : Collection<pb_MetaData>
{
	[SerializeField] private ObjectSpawner spawner;
	public override GameObject GetMatchingObject(pb_MetaData source)
	{
		var allSpawnable = spawner.SpawnableObjects;
		var matchingObj = allSpawnable.FirstOrDefault(spawn => spawn.Type == source.instanceType && spawn.EffectType == source.effectType);
		if (matchingObj != null)
		{
			return matchingObj.Prefab;
		}
		
		Debug.LogWarning("Could not find spawnable object");
		return null;
	}
}
#endif
