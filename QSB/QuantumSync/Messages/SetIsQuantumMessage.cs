﻿using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages;

public class SetIsQuantumMessage : QSBWorldObjectMessage<IQSBQuantumObject, bool>
{
	public SetIsQuantumMessage(bool isQuantum) : base(isQuantum) { }

	public override void OnReceiveRemote() => WorldObject.SetIsQuantum(Data);
}