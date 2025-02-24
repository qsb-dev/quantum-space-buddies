using Mirror;
using Steamworks;
using System;

namespace SteamTransport;

public static class Util
{
	public static int SendFlag2MirrorChannel(int sendFlag) => sendFlag switch
	{
		Constants.k_nSteamNetworkingSend_Reliable => Channels.Reliable,
		Constants.k_nSteamNetworkingSend_Unreliable => Channels.Unreliable,
		_ => throw new ArgumentOutOfRangeException(nameof(sendFlag), sendFlag, null)
	};

	public static int MirrorChannel2SendFlag(int mirrorChannel) => mirrorChannel switch
	{
		Channels.Reliable => Constants.k_nSteamNetworkingSend_Reliable,
		Channels.Unreliable => Constants.k_nSteamNetworkingSend_Unreliable,
		_ => throw new ArgumentOutOfRangeException(nameof(mirrorChannel), mirrorChannel, null)
	};

	public static string GetDescription(this HSteamNetConnection conn)
	{
		SteamNetworkingSockets.GetConnectionInfo(conn, out var pInfo);
		return pInfo.m_szConnectionDescription;
	}
}
