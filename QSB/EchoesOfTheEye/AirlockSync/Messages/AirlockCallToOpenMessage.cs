using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AirlockSync.Messages;

internal class AirlockCallToOpenMessage : QSBWorldObjectMessage<QSBAirlockInterface, bool>
{
	public AirlockCallToOpenMessage(bool front) : base(front) { }

	public override void OnReceiveRemote()
	{
		var airlockInterface = WorldObject.AttachedObject;

		if (Data)
		{
			if (airlockInterface._frozenFromFlood)
			{
				airlockInterface._backGearEffects.PlayFailure(true, 1f);
				return;
			}

			if (airlockInterface._calledToOpenFromOutside)
			{
				airlockInterface._frontGearEffects.AddRotation(90f, 1f);
				return;
			}

			airlockInterface._frontGearEffects.AddRotation(90f, 1f);
			airlockInterface._rotatingDirection--;
			airlockInterface.enabled = true;
			airlockInterface._calledToOpenFromOutside = true;
			airlockInterface.CallOnRotateEvent();
		}
		else
		{
			if (airlockInterface._frozenFromFlood)
			{
				airlockInterface._backGearEffects.PlayFailure(true, 1f);
				return;
			}

			if (airlockInterface._calledToOpenFromOutside)
			{
				airlockInterface._backGearEffects.AddRotation(90f, 1f);
				return;
			}

			airlockInterface._backGearEffects.AddRotation(90f, 1f);
			airlockInterface._rotatingDirection++;
			airlockInterface.enabled = true;
			airlockInterface._calledToOpenFromOutside = true;
			airlockInterface.CallOnRotateEvent();
		}
	}
}
