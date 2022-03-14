using Mirror;
using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.VariableSync;

internal class AirlockVariableSyncer : RotatingElementsVariableSyncer<QSBGhostAirlock>
{
	protected override Transform[] RotatingElements => WorldObject.AttachedObject._interface._rotatingElements;

	protected override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);

		var airlockInterface = WorldObject.AttachedObject._interface;
		writer.Write(airlockInterface._currentRotation);
		writer.Write(airlockInterface._rotatingSpeed);
		writer.Write(airlockInterface._rotatingDirection);
	}

	protected override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);

		var airlockInterface = WorldObject.AttachedObject._interface;
		airlockInterface._currentRotation = reader.Read<float>();
		airlockInterface._rotatingSpeed = reader.Read<float>();
		airlockInterface._rotatingDirection = reader.Read<int>();
	}
}
