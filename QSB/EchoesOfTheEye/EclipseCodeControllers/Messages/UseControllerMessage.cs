using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers.Messages;

public class UseControllerMessage : QSBWorldObjectMessage<QSBEclipseCodeController, uint>
{
	public UseControllerMessage(bool @using) : base(@using ? QSBPlayerManager.LocalPlayerId : 0) { }
	public UseControllerMessage(uint user) : base(user) { }
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => WorldObject.SetUser(Data);
}
