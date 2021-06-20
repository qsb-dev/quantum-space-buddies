using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ShipSync.Events.Hull
{
	public class HullChangeIntegrityMessage : WorldObjectMessage
	{
		public float Integrity { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
		}
	}
}
