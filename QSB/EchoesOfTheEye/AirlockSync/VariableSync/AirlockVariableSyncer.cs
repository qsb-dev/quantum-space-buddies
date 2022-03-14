using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.Utility.VariableSync;
using System.Linq;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.VariableSync;

internal class AirlockVariableSyncer : WorldObjectVariableSyncer<Vector3[], QSBGhostAirlock>
{
	protected override void Update()
	{
		base.Update();

		var rotatingElements = AttachedWorldObject.AttachedObject._interface._rotatingElements;

		if (hasAuthority)
		{
			Value = rotatingElements.Select(x => x.localRotation.eulerAngles).ToArray();
		}
		else
		{
			for (var i = 0; i < rotatingElements.Length; i++)
			{
				rotatingElements[i].localRotation = Quaternion.Euler(Value[i]);
			}
		}
	}
}
