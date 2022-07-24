using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using System.Linq;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

/// <summary>
/// always sent by host
/// </summary>
internal class IlluminatedByMessage : QSBWorldObjectMessage<QSBLightSensor, uint[]>
{
	public IlluminatedByMessage(uint[] illuminatedBy) : base(illuminatedBy) { }

	public override void OnReceiveRemote()
	{
		foreach (var added in Data.Except(WorldObject._illuminatedBy).ToList())
		{
			WorldObject.SetIlluminated(added, true);
		}

		foreach (var removed in WorldObject._illuminatedBy.Except(Data).ToList())
		{
			WorldObject.SetIlluminated(removed, false);
		}
	}
}
