using QSB.Player;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	internal class RemoteThrusterFlameController : MonoBehaviour
	{
		private Thruster _thruster;
		private Light _light;
		private AnimationCurve _scaleByThrust = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		private DampedSpring _scaleSpring = new DampedSpring();
		private float _belowMaxThrustScalar = 1f;
		private MeshRenderer _thrusterRenderer;
		private Vector3 _thrusterFilter;
		private float _baseLightRadius;
		private float _currentScale;

		private bool _isReady;
		private bool _initialized;
		private PlayerInfo _attachedPlayer;

		// TODO : Make flames not appear underwater (Check original code!)

		public void InitFromOld(Thruster thruster, Light light, AnimationCurve scaleByThrust, DampedSpring scaleSpring, float belowMaxThrustScalar, float baseLightRadius, PlayerInfo player)
		{
			_thruster = thruster;
			_light = light;
			_scaleByThrust = scaleByThrust;
			_scaleSpring = scaleSpring;
			_belowMaxThrustScalar = belowMaxThrustScalar;
			_attachedPlayer = player;
			_baseLightRadius = baseLightRadius;
			_isReady = true;
		}

		private void Init()
		{
			_thrusterRenderer = GetComponent<MeshRenderer>();
			_thrusterFilter = OWUtilities.GetShipThrusterFilter(_thruster);
			_currentScale = 0f;
			_thrusterRenderer.enabled = false;
			_light.enabled = false;
			_light.shadows = LightShadows.Soft;
			_initialized = true;
		}

		private void Update()
		{
			if (_isReady && !_initialized)
			{
				Init();
			}

			if (!_initialized)
			{
				return;
			}

			var num = _scaleByThrust.Evaluate(GetThrustFraction());
			if (_belowMaxThrustScalar < 1f)
			{
				num *= _belowMaxThrustScalar;
			}

			_currentScale = _scaleSpring.Update(_currentScale, num, Time.deltaTime);
			if (_currentScale < 0f)
			{
				_currentScale = 0f;
				_scaleSpring.ResetVelocity();
			}

			if (_currentScale <= 0.001f)
			{
				_currentScale = 0f;
				_scaleSpring.ResetVelocity();
			}

			transform.localScale = Vector3.one * _currentScale;
			_light.range = _baseLightRadius * _currentScale;
			_thrusterRenderer.enabled = _currentScale > 0f;
			_light.enabled = _currentScale > 0f;
		}

		private float GetThrustFraction() => Vector3.Dot(_attachedPlayer.JetpackAcceleration.LocalAcceleration, _thrusterFilter);

		private void OnRenderObject()
		{
			if (!QSBCore.WorldObjectsReady || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}

			Popcron.Gizmos.Sphere(_light.transform.position, 0.05f, Color.yellow, false, 4);
			Popcron.Gizmos.Line(_light.transform.position, _light.transform.parent.position, Color.yellow);
		}
	}
}
