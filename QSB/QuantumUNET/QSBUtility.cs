using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace QSB.QuantumUNET
{
	public class QSBUtility
	{
		private static readonly Dictionary<NetworkID, NetworkAccessToken> _dictTokens = new Dictionary<NetworkID, NetworkAccessToken>();

		public static SourceID GetSourceID() 
			=> (SourceID)SystemInfo.deviceUniqueIdentifier.GetHashCode();

		public static void SetAccessTokenForNetwork(NetworkID netId, NetworkAccessToken accessToken)
		{
			if (_dictTokens.ContainsKey(netId))
			{
				_dictTokens.Remove(netId);
			}
			_dictTokens.Add(netId, accessToken);
		}

		public static NetworkAccessToken GetAccessTokenForNetwork(NetworkID netId)
		{
			if (!_dictTokens.TryGetValue(netId, out var result))
			{
				result = new NetworkAccessToken();
			}
			return result;
		}
	}
}