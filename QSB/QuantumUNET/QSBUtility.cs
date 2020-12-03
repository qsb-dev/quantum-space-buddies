using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace QSB.QuantumUNET
{
	public class QSBUtility
	{
		private QSBUtility()
		{
		}

		[Obsolete("This property is unused and should not be referenced in code.", true)]
		public static bool useRandomSourceID
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public static SourceID GetSourceID()
		{
			return (SourceID)((long)SystemInfo.deviceUniqueIdentifier.GetHashCode());
		}

		[Obsolete("This function is unused and should not be referenced in code. Please sign in and setup your project in the editor instead.", true)]
		public static void SetAppID(AppID newAppID)
		{
		}

		[Obsolete("This function is unused and should not be referenced in code. Please sign in and setup your project in the editor instead.", true)]
		public static AppID GetAppID()
		{
			return AppID.Invalid;
		}

		public static void SetAccessTokenForNetwork(NetworkID netId, NetworkAccessToken accessToken)
		{
			if (s_dictTokens.ContainsKey(netId))
			{
				s_dictTokens.Remove(netId);
			}
			s_dictTokens.Add(netId, accessToken);
		}

		public static NetworkAccessToken GetAccessTokenForNetwork(NetworkID netId)
		{
			NetworkAccessToken result;
			if (!s_dictTokens.TryGetValue(netId, out result))
			{
				result = new NetworkAccessToken();
			}
			return result;
		}

		private static Dictionary<NetworkID, NetworkAccessToken> s_dictTokens = new Dictionary<NetworkID, NetworkAccessToken>();
	}
}