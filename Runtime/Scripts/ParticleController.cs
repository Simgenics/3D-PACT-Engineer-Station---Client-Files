using ImportExportScene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleController : MonoBehaviour
{
	[SerializeField]
	private List<EffectTypeObjects> effectTypes = new List<EffectTypeObjects>();
	[Space(100)]
	[SerializeField]
	private GameObject tempObject;
	[SerializeField]
	private GameObject activeParticleParent;

	private ParticleSystem[] particleSystems;
	private ParticleSystem mainSystem;
	private EmissionModule emissionModule;
	private MainModule mainModule;
	private ShapeModule shapeModule;

	private GameObject activeParent;
	private float originalGravity;
	private ParticleParameters parameters;
	private pb_MetaDataComponent metaDataComponent;

	public ParticleParameters Parameters => parameters;
	public List<EffectTypeObjects> EffectTypes => effectTypes;
	public ParticleSystem[] ParticleSystems => particleSystems;
	public ParticleSystem MainSystem => mainSystem;
	public EmissionModule EmissionModule => emissionModule;
	public MainModule MainModule => mainModule;
	public ShapeModule ShapeModule => shapeModule;

	public void Initialize(pb_MetaDataComponent meta, EffectType effectType)
	{
		activeParent = activeParticleParent != null ? activeParticleParent : gameObject;
		metaDataComponent = meta;
		SetUpModules();
		if (tempObject)
			tempObject.SetActive(true);
		parameters = new ParticleParameters();
		SetupParameters(effectType);
	}

	public string GetSerializedData()
	{
		return JsonUtility.ToJson(parameters);
	}

	public void DeserializeData(pb_MetaDataComponent meta)
	{
		Initialize(meta, meta.metadata.effectType);
		this.parameters = JsonUtility.FromJson<ParticleParameters>(meta.metadata.particleParameters);
		SetTypePrefab(parameters.EffectType);
		SetUpModules();
		SetAngle(parameters.Angle);
		SetEmission(parameters.Emission);
		SetLifetime(parameters.LifeTime);
		SetEmissionForce(parameters.EmissionForce);
		SetGravity(parameters.Gravity);
		SetWind(parameters.Wind.x, parameters.Wind.y, parameters.Wind.z);
		LoopToggled(parameters.IsLooped);
		if (parameters.IsStopped)
			StopAllSystems();
		else
			StartAllSystems();
	}

	void SetUpModules()
	{
		particleSystems = activeParent.GetComponentsInChildren<ParticleSystem>();
		if (particleSystems.Length > 0)
		{
			mainSystem = particleSystems[0];
			emissionModule = mainSystem.emission;
			mainModule = mainSystem.main;
			shapeModule = mainSystem.shape;
			var gravity = mainModule.gravityModifier;
			originalGravity = gravity.constant;
		}
	}

	void SetupParameters(EffectType effectType)
	{
		parameters.EffectType = effectType;
		parameters.Angle = shapeModule.angle;
		parameters.Emission = EmissionModule.rateOverTime.constant;
		parameters.LifeTime = MainModule.startLifetime.constant;
		parameters.Gravity = MainModule.gravityModifier.constant != 0;
		parameters.IsLooped = MainModule.loop;
		parameters.ParticleSize = MainModule.startSize.constant;
		var folt = particleSystems[0].forceOverLifetime;
		parameters.Wind = new Vector3(folt.x.constant, folt.y.constant, folt.z.constant);
		parameters.EmissionForce = MainModule.startSpeedMultiplier;
	}

	public void SetTypePrefab(EffectType effectType)
	{
		parameters.EffectType = effectType;
		metaDataComponent.SetMetaParticleEffectType(effectType);
		if (effectType == EffectType.None || effectType == EffectType.WaterEffect)
		{
			if (activeParent != null)
				activeParent.SetActive(false);
			if (tempObject)
			{
				tempObject.SetActive(true);
				activeParent = tempObject;
			}
			SetUpModules();
			return;
		}
		foreach (var type in effectTypes)
		{
			if (type.EffectType == effectType)
			{
				activeParent.SetActive(false);
				activeParent = type.ParentObject;
				activeParent.SetActive(true);
				SetUpModules();
				break;
			}
		}
		if (tempObject != null)
			tempObject.SetActive(false);
	}

	public void SetAngle(float angle)
	{
		if (particleSystems.Length == 0)
			return;
		shapeModule.angle = angle;
		parameters.Angle = angle;
	}

	public void SetEmission(float value)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			EmissionModule module = particleSystems[i].emission;
			module.rateOverTime = value;
		}
		parameters.Emission = value;
	}

	public void SetLifetime(float value)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule module = particleSystems[i].main;
			var life = module.startLifetime;
			life.constant = value;
			module.startLifetime = life;
		}
		parameters.LifeTime = value;
	}

	public void SetEmissionForce(float value)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule module = particleSystems[i].main;
			module.startSpeedMultiplier = value;
		}
		parameters.EmissionForce = value;
	}

	public void SetGravity(bool isOn)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule module = particleSystems[i].main;
			var gravity = module.gravityModifier;
			gravity.constant = isOn ? originalGravity : 0;
			module.gravityModifier = gravity;
		}
		parameters.Gravity = isOn;
	}

	public void SetWind(float x, float y, float z)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			var folt = particleSystems[i].forceOverLifetime;
			folt.x = x;
			folt.z = y;
			folt.x = z;
		}
		parameters.Wind = new Vector3(x, y, z);
	}

	public void WindXChanged(float x)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			var folt = particleSystems[i].forceOverLifetime;
			folt.x = x;
		}
		parameters.Wind.x = x;
	}

	public void WindYChanged(float y)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			var folt = particleSystems[i].forceOverLifetime;
			folt.y = y;
		}
		parameters.Wind.y = y;
	}

	public void WindZChanged(float z)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			var folt = particleSystems[i].forceOverLifetime;
			folt.z = z;
		}
		parameters.Wind.z = z;
	}

	public void LoopToggled(bool isOn)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule module = particleSystems[i].main;
			module.loop = isOn;
			if (isOn)
				particleSystems[i].Play();
		}
		parameters.IsLooped = isOn;
	}

	public void ParticleSizeChanged(float particleSize)
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule module = particleSystems[i].main;
			var size = module.startSize;
			size.constant = particleSize;
			module.startSize = size;
		}
		parameters.ParticleSize = particleSize;
	}

	public void StartAllSystems()
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Play();
		}
		parameters.IsStopped = false;
	}

	public void StopAllSystems()
	{
		if (particleSystems.Length == 0)
			return;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Stop();
		}
		parameters.IsStopped = true;
	}
}

[System.Serializable]
public class EffectTypeObjects
{
	public GameObject ParentObject;
	public EffectType EffectType;
}

[System.Serializable]
public class ParticleParameters
{
	public EffectType EffectType;
	public float Angle;
	public float Emission;
	public float LifeTime;
	public float EmissionForce;
	public bool Gravity;
	public Vector3 Wind;
	public bool IsLooped;
	public float ParticleSize;
	public bool IsStopped;
}
