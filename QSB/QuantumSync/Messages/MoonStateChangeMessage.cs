using Mirror;
using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Messages
{
	public class MoonStateChangeMessage : QSBMessage
	{
		private int StateIndex;
		private Vector3 OnUnitSphere;
		private int OrbitAngle;

		public MoonStateChangeMessage(int stateIndex, Vector3 onUnitSphere, int orbitAngle)
		{
			StateIndex = stateIndex;
			OnUnitSphere = onUnitSphere;
			OrbitAngle = orbitAngle;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(StateIndex);
			writer.Write(OnUnitSphere);
			writer.Write(OrbitAngle);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			StateIndex = reader.Read<int>();
			OnUnitSphere = reader.ReadVector3();
			OrbitAngle = reader.Read<int>();
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var moon = Locator.GetQuantumMoon();
			var wasPlayerEntangled = moon.IsPlayerEntangled();
			var location = new RelativeLocationData(Locator.GetPlayerTransform().GetComponent<OWRigidbody>(), moon.transform);
			var moonBody = moon._moonBody;
			var constantForceDetector = (ConstantForceDetector)moonBody.GetAttachedForceDetector();
			var orbits = moon._orbits;
			var orbit = orbits.First(x => x.GetStateIndex() == StateIndex);
			var orbitRadius = orbit.GetOrbitRadius();
			var owRigidbody = orbit.GetAttachedOWRigidbody();
			var position = OnUnitSphere * orbitRadius + owRigidbody.GetWorldCenterOfMass();
			moonBody.transform.position = position;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}

			constantForceDetector.AddConstantVolume(owRigidbody.GetAttachedGravityVolume(), true, true);
			moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(owRigidbody, moonBody, OrbitAngle) + owRigidbody.GetVelocity());
			moon._stateIndex = StateIndex;

			if (moon.IsPlayerInside())
			{
				moon.SetSurfaceState(StateIndex);
			}
			else
			{
				moon.SetSurfaceState(-1);
				moon._quantumSignal.SetSignalActivation(StateIndex != 5);
			}

			moon._referenceFrameVolume.gameObject.SetActive(StateIndex != 5);
			moonBody.SetIsTargetable(StateIndex != 5);
			foreach (var obj in moon._deactivateAtEye)
			{
				obj.SetActive(StateIndex != 5);
			}

			GlobalMessenger<OWRigidbody>.FireEvent("QuantumMoonChangeState", moonBody);

			if (wasPlayerEntangled)
			{
				Locator.GetPlayerTransform().GetComponent<OWRigidbody>().MoveToRelativeLocation(location, moon.transform);
			}
		}
	}
}