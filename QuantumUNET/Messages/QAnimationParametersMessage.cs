﻿using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QAnimationParametersMessage : QMessageBase
	{
		public QNetworkInstanceId netId;
		public byte[] parameters;

		public override void Deserialize(QNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			parameters = reader.ReadBytesAndSize();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(netId);
			writer.WriteBytesAndSize(parameters, parameters?.Length ?? 0);
		}
	}
}