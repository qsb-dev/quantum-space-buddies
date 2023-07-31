using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

public class IlluminatingLanternsMessage : QSBWorldObjectMessage<QSBLightSensor, int[]>
{
	public IlluminatingLanternsMessage(IEnumerable<DreamLanternController> lanterns) :
		base(lanterns.Select(x => x.GetWorldObject<QSBDreamLanternController>().ObjectId).ToArray()) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._illuminatingDreamLanternList.Clear();
		WorldObject.AttachedObject._illuminatingDreamLanternList.AddRange(
			Data.Select(x => x.GetWorldObject<QSBDreamLanternController>().AttachedObject));
	}
}
