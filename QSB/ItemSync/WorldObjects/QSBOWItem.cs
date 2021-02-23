using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	class QSBOWItem<T> : WorldObject<T>, IQSBOWItem 
		where T : MonoBehaviour
	{
		public uint HoldingPlayer { get; set; }

		public override void Init(T attachedObject, int id) { }
	}
}
