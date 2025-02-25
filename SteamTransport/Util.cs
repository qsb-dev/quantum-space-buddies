using Mirror;
using Steamworks;
using System;

namespace SteamTransport;

public static class Util
{
	public const int MaxMessages = 256; // same as fizzy steamworks

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

	public static string DebugToString(this HSteamNetConnection conn)
	{
		SteamNetworkingSockets.GetConnectionInfo(conn, out var pInfo);
		return pInfo.m_szConnectionDescription;
	}

	public static string DebugToString(this SteamNetworkingIdentity ident)
	{
		ident.ToString(out var s);
		return s;
	}
	public static string DebugToString(this SteamNetworkingIPAddr addr)
	{
		addr.ToString(out var s, true);
		return s;
	}
}
