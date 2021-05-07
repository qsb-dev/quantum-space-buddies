using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	class RemoteThrusterWashController : MonoBehaviour
	{
		private float _raycastDistance = 10f;
		private AnimationCurve _emissionDistanceScale;
		private AnimationCurve _emissionThrusterScale;
		private ParticleSystem _defaultParticleSystem;

		private ParticleSystem.MainModule _defaultMainModule;
		private ParticleSystem.EmissionModule _defaultEmissionModule;
		private float _baseDefaultEmissionRate;

		private PlayerInfo _attachedPlayer;

		private bool _isReady;
		private bool _initialised;

		public void InitFromOld(AnimationCurve distanceScale, AnimationCurve thrusterScale, ParticleSystem defaultParticleSystem, PlayerInfo player)
		{
			DebugLog.DebugWrite($"InitFromOld for player {player.PlayerId}");
			_emissionDistanceScale = distanceScale;
			_emissionThrusterScale = thrusterScale;
			_defaultParticleSystem = defaultParticleSystem;
			_attachedPlayer = player;
			_isReady = true;
		}

		private void Init()
		{
			DebugLog.DebugWrite($"Init for player {_attachedPlayer.PlayerId}");
			if (_defaultParticleSystem == null)
			{
				DebugLog.ToConsole($"Error - DefaultParticleSystem is null!", OWML.Common.MessageType.Error);
				return;
			}
			else
			{
				DebugLog.DebugWrite($"DefaultParticleSystem OK.");
			}
			_defaultMainModule = _defaultParticleSystem.main;
			_defaultEmissionModule = _defaultParticleSystem.emission;
			_baseDefaultEmissionRate = _defaultEmissionModule.rateOverTime.constant;
			_initialised = true;
		}

		private void Update()
		{
			if (_isReady && !_initialised)
			{
				Init();
			}

			if (!_initialised)
			{
				return;
			}

			RaycastHit hitInfo = default;
			var aboveSurface = false;
			var emissionThrusterScale = _emissionThrusterScale.Evaluate(_attachedPlayer.JetpackAcceleration.LocalAcceleration.y);
			if (emissionThrusterScale > 0f)
			{
				aboveSurface = Physics.Raycast(transform.position, transform.forward, out hitInfo, _raycastDistance, OWLayerMask.physicalMask);
			}

			emissionThrusterScale = (!aboveSurface) ? 0f : (emissionThrusterScale * _emissionDistanceScale.Evaluate(hitInfo.distance));

			if (emissionThrusterScale > 0f)
			{
				var position = hitInfo.point + (hitInfo.normal * 0.25f);
				var rotation = Quaternion.LookRotation(hitInfo.normal);
				if (!_defaultParticleSystem.isPlaying)
				{
					DebugLog.DebugWrite($"{_attachedPlayer.PlayerId} play");
					_defaultParticleSystem.Play();
				}
				_defaultEmissionModule.rateOverTimeMultiplier = _baseDefaultEmissionRate * emissionThrusterScale;
				_defaultParticleSystem.transform.SetPositionAndRotation(position, rotation);
				if (_defaultMainModule.customSimulationSpace != hitInfo.transform)
				{
					_defaultMainModule.customSimulationSpace = hitInfo.transform;
					_defaultParticleSystem.Clear();
				}
			}
			else
			{
				if (_defaultParticleSystem.isPlaying)
				{
					DebugLog.DebugWrite($"{_attachedPlayer.PlayerId} stop");
					_defaultParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
				}
			}
		}
	}
}
