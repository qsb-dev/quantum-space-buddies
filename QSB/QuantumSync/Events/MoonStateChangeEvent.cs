using OWML.Utils;
using QSB.Events;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	public class MoonStateChangeEvent : QSBEvent<MoonStateChangeMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.MoonStateChange;

		public override void SetupListener() => GlobalMessenger<int, Vector3, int>.AddListener(EventNames.QSBMoonStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<int, Vector3, int>.RemoveListener(EventNames.QSBMoonStateChange, Handler);

		private void Handler(int stateIndex, Vector3 onUnitSphere, int orbitAngle) => SendEvent(CreateMessage(stateIndex, onUnitSphere, orbitAngle));

		private MoonStateChangeMessage CreateMessage(int stateIndex, Vector3 onUnitSphere, int orbitAngle) => new()
		{
			AboutId = LocalPlayerId,
			StateIndex = stateIndex,
			OnUnitSphere = onUnitSphere,
			OrbitAngle = orbitAngle
		};

		public override void OnReceiveRemote(bool server, MoonStateChangeMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var moon = Locator.GetQuantumMoon();
			var wasPlayerEntangled = moon.IsPlayerEntangled();
			var location = new RelativeLocationData(Locator.GetPlayerTransform().GetComponent<OWRigidbody>(), moon.transform);
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

			if (moon.IsPlayerInside())
			{
				moon.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(moon, new object[] { message.StateIndex });
			}
			else
			{
				moon.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(moon, new object[] { -1 });
				moon.GetValue<AudioSignal>("_quantumSignal").SetSignalActivation(message.StateIndex != 5, 2f);
			}

			moon.GetValue<ReferenceFrameVolume>("_referenceFrameVolume").gameObject.SetActive(message.StateIndex != 5);
			moonBody.SetIsTargetable(message.StateIndex != 5);
			foreach (var obj in moon.GetValue<GameObject[]>("_deactivateAtEye"))
			{
				obj.SetActive(message.StateIndex != 5);
			}

			GlobalMessenger<OWRigidbody>.FireEvent("QuantumMoonChangeState", moonBody);

			if (wasPlayerEntangled)
			{
				Locator.GetPlayerTransform().GetComponent<OWRigidbody>().MoveToRelativeLocation(location, moon.transform);
			}
		}
	}
}
