using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.ShipSync
{
	class ShipManager : MonoBehaviour
	{
		public static ShipManager Instance;

		private void Awake() 
			=> Instance = this;

		private uint _currentFlyer = uint.MaxValue;
		public uint CurrentFlyer
		{
			get => _currentFlyer;
			set
			{
				if (_currentFlyer != uint.MaxValue && value != uint.MaxValue)
				{
					DebugLog.ToConsole($"Warning - Trying to set current flyer while someone is still flying? Current:{_currentFlyer}, New:{value}", MessageType.Warning);
				}
				_currentFlyer = value;
			}
		}
	}
}
