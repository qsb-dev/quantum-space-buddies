using OWML.Utils;
using QSB.Events;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	public class MoonStateChangeEvent : QSBEvent<MoonStateChangeMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.MoonStateChange;

		public override void SetupListener() => GlobalMessenger<int, Vector3, int>.AddListener(EventNames.QSBMoonStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<int, Vector3, int>.RemoveListener(EventNames.QSBMoonStateChange, Handler);

		private void Handler(int stateIndex, Vector3 onUnitSphere, int orbitAngle) => SendEvent(CreateMessage(stateIndex, onUnitSphere, orbitAngle));

		private MoonStateChangeMessage CreateMessage(int stateIndex, Vector3 onUnitSphere, int orbitAngle) => new MoonStateChangeMessage
		{
			AboutId = LocalPlayerId,
			StateIndex = stateIndex,
			OnUnitSphere = onUnitSphere,
			OrbitAngle = orbitAngle
		};

		public override void OnReceiveRemote(bool server, MoonStateChangeMessage message)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			DebugLog.DebugWrite($"MOON TO INDEX {message.StateIndex}, ANGLE {message.OrbitAngle}, POINT {message.OnUnitSphere}");
			var moon = Locator.GetQuantumMoon();
			var moonBody = moon.GetValue<OWRigidbody>("_moonBody");
			var constantFoceDetector = (ConstantForceDetector)moonBody.GetAttachedForceDetector();
			var orbits = moon.GetValue<QuantumOrbit[]>("_orbits");
			var orbit = orbits.First(x => x.GetStateIndex() == message.StateIndex);
			var orbitRadius = orbit.GetOrbitRadius();
			var owRigidbody = orbit.GetAttachedOWRigidbody();
			var position = (message.OnUnitSphere * orbitRadius) + owRigidbody.GetWorldCenterOfMass();
			moonBody.transform.position = position;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			constantFoceDetector.AddConstantVolume(owRigidbody.GetAttachedGravityVolume(), true, true);
			moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(owRigidbody, moonBody, message.OrbitAngle) + owRigidbody.GetVelocity());
			moon.SetValue("_stateIndex", message.StateIndex);
		}
	}
}
