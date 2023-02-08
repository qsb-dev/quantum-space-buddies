using QSB.Messaging;
using QSB.QuantumSync.Messages;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects;

internal class QSBQuantumMoon : QSBQuantumObject<QuantumMoon>
{
	public override bool HostControls => true;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		if (QSBCore.IsHost)
		{
			var moon = AttachedObject;
			var moonBody = moon._moonBody;
			var stateIndex = moon.GetStateIndex();
			var orbit = moon._orbits.First(y => y.GetStateIndex() == stateIndex);
			var orbitBody = orbit.GetAttachedOWRigidbody();
			var relPos = moonBody.GetWorldCenterOfMass() - orbitBody.GetWorldCenterOfMass();
			var relVel = moonBody.GetVelocity() - orbitBody.GetVelocity();
			var onUnitSphere = relPos.normalized;
			var perpendicular = Vector3.Cross(relPos, Vector3.up).normalized;
			var orbitAngle = (int)OWMath.WrapAngle(OWMath.Angle(perpendicular, relVel, relPos));

			new MoonStateChangeMessage(stateIndex, onUnitSphere, orbitAngle) { To = to }.Send();
		}
	}
}