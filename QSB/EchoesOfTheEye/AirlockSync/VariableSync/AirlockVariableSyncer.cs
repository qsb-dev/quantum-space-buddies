using Mirror;
using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.Utility.LinkedWorldObject;
using System.Linq;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.VariableSync;

internal class AirlockVariableSyncer : LinkedVariableSyncer<Vector3[], QSBGhostAirlock>
{
	protected override bool HasChanged()
	{
		var rotatingElements = WorldObject.AttachedObject._interface._rotatingElements;
		Value = rotatingElements.Select(x => x.localRotation.eulerAngles).ToArray();

		return base.HasChanged();
	}

	protected override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);

		var rotatingElements = WorldObject.AttachedObject._interface._rotatingElements;
		for (var i = 0; i < rotatingElements.Length; i++)
		{
			rotatingElements[i].localRotation = Quaternion.Euler(Value[i]);
		}
	}
}
