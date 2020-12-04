using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBEmptyMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
		}
	}
}
