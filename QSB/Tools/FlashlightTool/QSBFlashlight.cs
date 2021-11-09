using UnityEngine;

namespace QSB.Tools.FlashlightTool
{
	public class QSBFlashlight : MonoBehaviour, ILightSource
	{
		private OWLight2[] _lights;
		internal OWLight2 _illuminationCheckLight;
		private Transform _root;
		private Transform _basePivot;
		private Transform _wobblePivot;
		private Vector3 _baseForward;
		private Quaternion _baseRotation;
		private LightSourceVolume _lightSourceVolume;

		public bool FlashlightOn;

		public void Start()
		{
			_lightSourceVolume = this.GetRequiredComponentInChildren<LightSourceVolume>();
			_lightSourceVolume.LinkLightSource(this);
			_lightSourceVolume.SetVolumeActivation(FlashlightOn);
			_baseForward = _basePivot.forward;
			_baseRotation = _basePivot.rotation;
		}

		public void Init(Flashlight oldComponent)
		{
			_lights = oldComponent._lights;
			_illuminationCheckLight = oldComponent._illuminationCheckLight;
			_root = oldComponent._root;
			_basePivot = oldComponent._basePivot;
			_wobblePivot = oldComponent._wobblePivot;
			Destroy(oldComponent.GetComponent<LightLOD>());

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
}