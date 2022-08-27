using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.FlashlightTool;

[UsedInUnityProject]
public class QSBFlashlight : MonoBehaviour, ILightSource
{
	[SerializeField]
	private OWLight2[] _lights;

	[SerializeField]
	private OWLight2 _illuminationCheckLight;

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private Transform _basePivot;

	[SerializeField]
	private Transform _wobblePivot;

	private Vector3 _baseForward;
	private Quaternion _baseRotation;
	private LightSourceVolume _lightSourceVolume;

	public bool FlashlightOn;
	public PlayerInfo Player;

	public void Start()
	{
		_lightSourceVolume = this.GetRequiredComponentInChildren<LightSourceVolume>();
		_lightSourceVolume.LinkLightSource(this);
		_lightSourceVolume.SetVolumeActivation(FlashlightOn);
		if (_basePivot == null)
		{
			DebugLog.DebugWrite($"Error - _basePivot is null!", OWML.Common.MessageType.Error);
			return;
		}

		_baseForward = _basePivot.forward;
		_baseRotation = _basePivot.rotation;
	}

	public void Init()
	{
		foreach (var light in _lights)
		{
			light.GetLight().enabled = false;
			light.GetLight().shadows = LightShadows.Soft;
		}

		FlashlightOn = false;
	}

	public LightSourceType GetLightSourceType()
		=> LightSourceType.FLASHLIGHT;

	public OWLight2[] GetLights()
		=> _lights;

	public void UpdateState(bool value)
	{
		if (value)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	private void TurnOn()
	{
		if (FlashlightOn)
		{
			return;
		}

		foreach (var light in _lights)
		{
			light.SetActivation(true);
		}

		FlashlightOn = true;
		Player.AudioController.PlayTurnOnFlashlight();
		var rotation = _root.rotation;
		_basePivot.rotation = rotation;
		_baseRotation = rotation;
		_baseForward = _basePivot.forward;
		_lightSourceVolume.SetVolumeActivation(FlashlightOn);
	}

	private void TurnOff()
	{
		if (!FlashlightOn)
		{
			return;
		}

		foreach (var light in _lights)
		{
			light.SetActivation(false);
		}

		FlashlightOn = false;
		Player.AudioController.PlayTurnOffFlashlight();
		_lightSourceVolume.SetVolumeActivation(FlashlightOn);
	}

	public bool CheckIlluminationAtPoint(Vector3 worldPoint, float buffer = 0f, float maxDistance = float.PositiveInfinity)
		=> FlashlightOn
		   && _illuminationCheckLight.CheckIlluminationAtPoint(worldPoint, buffer, maxDistance);

	public void FixedUpdate()
	{
		// This really isn't needed... but it makes it look that extra bit nicer. ^_^
		var lhs = Quaternion.FromToRotation(_basePivot.up, _root.up) * Quaternion.FromToRotation(_baseForward, _root.forward);
		var b = lhs * _baseRotation;
		_baseRotation = Quaternion.Slerp(_baseRotation, b, 6f * Time.deltaTime);
		_basePivot.rotation = _baseRotation;
		_baseForward = _basePivot.forward;
		_wobblePivot.localRotation = OWUtilities.GetWobbleRotation(0.3f, 0.15f) * Quaternion.identity;
	}
}