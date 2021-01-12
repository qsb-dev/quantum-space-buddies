using QSB.Messaging;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	class ShrineRotationMessage : PlayerMessage
	{
		public Quaternion Rotation { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Rotation = reader.ReadQuaternion();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Rotation);
		}
	}
}
