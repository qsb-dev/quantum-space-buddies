using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Player;

[UsedInUnityProject]
public class RemotePlayerFluidDetector : PriorityDetector
{
	private SplashEffect[] _splashEffects;

	[SerializeField]
	private Transform _splashSpawnRoot;

	private FluidTypeData[] _fluidDataByType;

	protected RemotePlayerVelocity _velocity;

	public event SpawnSplashEvent OnSpawnSplashEvent;
	public event FluidTypeEvent OnEnterFluidType;
	public event FluidTypeEvent OnExitFluidType;
	public event FluidEvent OnEnterFluid;
	public event FluidEvent OnExitFluid;

	public delegate void SpawnSplashEvent(FluidVolume splashFluid);
	public delegate void FluidTypeEvent(FluidVolume.Type fluidType);
	public delegate void FluidEvent(FluidVolume volume);

	public override void Awake()
	{
		base.Awake();

		_velocity = gameObject.GetComponentInParent<RemotePlayerVelocity>();
		_fluidDataByType = new FluidTypeData[9];
		_splashSpawnRoot = _velocity.transform;

		_splashEffects = new SplashEffect[4]
		{
			new SplashEffect()
			{
				fluidType = FluidVolume.Type.WATER,
				minImpactSpeed = 15,
				triggerEvent = SplashEffect.TriggerEvent.OnEntry,
				splashPrefab = Resources.Load<GameObject>("prefabs/particles/Prefab_OceanEntry_Player"),
				ignoreSphereAligment = false
			},
			new SplashEffect()
			{
				fluidType = FluidVolume.Type.CLOUD,
				minImpactSpeed = 15,
				triggerEvent = SplashEffect.TriggerEvent.OnEntry,
				splashPrefab = Resources.Load<GameObject>("prefabs/particles/Prefab_CloudEntry_Player"),
				ignoreSphereAligment = false
			},
			new SplashEffect()
			{
				fluidType = FluidVolume.Type.CLOUD,
				minImpactSpeed = 15,
				triggerEvent = SplashEffect.TriggerEvent.OnExit,
				splashPrefab = Resources.Load<GameObject>("prefabs/particles/Prefab_CloudExit_Player"),
				ignoreSphereAligment = true
			},
			new SplashEffect()
			{
				fluidType = FluidVolume.Type.SAND,
				minImpactSpeed = 15,
				triggerEvent = SplashEffect.TriggerEvent.OnEntryOrExit,
				splashPrefab = Resources.Load<GameObject>("prefabs/particles/Prefab_HEA_Player_SandSplash"),
				ignoreSphereAligment = false
			}
		};
	}

	public bool InFluidType(FluidVolume.Type fluidType)
	{
		return _fluidDataByType[(int)fluidType].count > 0;
	}

	public override void AddVolume(EffectVolume eVol)
	{
		var fluidVolume = eVol as FluidVolume;
		if (fluidVolume != null && (!fluidVolume.IsInheritible()))
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		var fluidVolume = eVol as FluidVolume;
		if (fluidVolume != null && (!fluidVolume.IsInheritible()))
		{
			base.RemoveVolume(eVol);
		}
	}

	public override void OnVolumeActivated(PriorityVolume volume)
	{
		var fluidVolume = volume as FluidVolume;
		var fluidType = fluidVolume.GetFluidType();
		var fluidDataByType = _fluidDataByType;
		var type = fluidType;
		fluidDataByType[(int)type].count = fluidDataByType[(int)type].count + 1;
		OnEnterFluid?.Invoke(fluidVolume);

		if (_fluidDataByType[(int)fluidType].count == 1)
		{
			OnEnterFluidType_Internal(fluidVolume);
			OnEnterFluidType?.Invoke(fluidType);
		}
	}

	public override void OnVolumeDeactivated(PriorityVolume volume)
	{
		var fluidVolume = volume as FluidVolume;
		var fluidType = fluidVolume.GetFluidType();
		var fluidDataByType = _fluidDataByType;
		var type = fluidType;
		fluidDataByType[(int)type].count = fluidDataByType[(int)type].count - 1;
		OnExitFluid?.Invoke(fluidVolume);

		if (_fluidDataByType[(int)fluidType].count == 0)
		{
			OnExitFluidType_Internal(fluidVolume);
			OnExitFluidType?.Invoke(fluidType);
		}
	}

	protected virtual void OnEnterFluidType_Internal(FluidVolume fluid)
	{
		SpawnSplash(fluid, SplashEffect.TriggerEvent.OnEntry);
		if (fluid is SphereOceanFluidVolume)
		{
			var component = GetComponent<OceanSplasher>();
			var magnitude = (_velocity.Velocity - fluid._attachedBody.GetVelocity()).magnitude;
			if (component != null && magnitude >= component.minSplashSpeed)
			{
				component.Splash();
			}
		}
	}

	protected virtual void OnExitFluidType_Internal(FluidVolume fluid)
	{
		SpawnSplash(fluid, SplashEffect.TriggerEvent.OnExit);
	}

	private void SpawnSplash(FluidVolume fluid, SplashEffect.TriggerEvent triggerEvent)
	{
		if (CompareName(Name.Player) && PlayerState.IsRidingRaft(false) && fluid.GetFluidType() == FluidVolume.Type.WATER)
		{
			return;
		}

		if (fluid.GetFluidType() == FluidVolume.Type.CLOUD && InFluidType(FluidVolume.Type.WATER))
		{
			return;
		}

		var impactVelocity = _velocity.Velocity - fluid._attachedBody.GetVelocity();
		var magnitude = impactVelocity.magnitude;
		var num = -1;
		for (var i = 0; i < _splashEffects.Length; i++)
		{
			if (_splashEffects[i].fluidType == fluid.GetFluidType() && magnitude > _splashEffects[i].minImpactSpeed && (triggerEvent & _splashEffects[i].triggerEvent) > (SplashEffect.TriggerEvent)0 && (num == -1 || _splashEffects[i].minImpactSpeed > _splashEffects[num].minImpactSpeed))
			{
				num = i;
			}
		}

		if (num > -1)
		{
			var splashPrefab = _splashEffects[num].splashPrefab;
			var toDirection = (_splashEffects[num].ignoreSphereAligment ? (-impactVelocity) : fluid.GetSplashAlignment(_splashSpawnRoot.position, impactVelocity));
			var rotation = Quaternion.FromToRotation(_splashSpawnRoot.up, toDirection) * _splashSpawnRoot.rotation;
			var gameObject = Instantiate(splashPrefab, _splashSpawnRoot.position, rotation);
			if (gameObject.GetComponent<OWRigidbody>() != null)
			{
				Debug.LogError("SPLASHES SHOULD NO LONGER HAVE RIGIDBODIES!!!", gameObject);
				gameObject.GetComponent<OWRigidbody>().MakeKinematic();
			}

			gameObject.transform.parent = fluid.transform;
			fluid.RegisterSplashTransform(gameObject.transform);
			var component = gameObject.GetComponent<SplashAudioController>();
			if (component != null)
			{
				component.PlaySplash();
			}

			OnSpawnSplashEvent?.Invoke(fluid);
		}
	}
}
