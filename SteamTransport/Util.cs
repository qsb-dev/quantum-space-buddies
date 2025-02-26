﻿using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

	public static string ToDebugString(this HSteamNetConnection conn)
	{
		SteamNetworkingSockets.GetConnectionInfo(conn, out var pInfo);
		return pInfo.m_szConnectionDescription;
	}

	public static string ToDebugString(this SteamNetworkingIdentity ident)
	{
		ident.ToString(out var s);
		return s;
	}

	public static string ToDebugString(this SteamNetworkingIPAddr addr)
	{
		addr.ToString(out var s, true);
		return s;
	}

	// could do send/recv util, but i wanna inline for performance

	public static SteamNetworkingConfigValue_t[] MakeOptions(SteamTransport transport)
	{
		var result = new List<SteamNetworkingConfigValue_t>();

		result.Add(new SteamNetworkingConfigValue_t
		{
			m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial,
			m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
			m_val = new SteamNetworkingConfigValue_t.OptionValue
			{
				m_int32 = transport.Timeout
			}
		});
		result.Add(new SteamNetworkingConfigValue_t
		{
			m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected,
			m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
			m_val = new SteamNetworkingConfigValue_t.OptionValue
			{
				m_int32 = transport.Timeout
			}
		});

		// 20% change of doing all, all delays are 200 ms. leads to about 1 second of rtt ping if enables on both ends.
		if (transport.DoFakeNetworkErrors)
		{
			var floatHandle = GCHandle.Alloc((float)20, GCHandleType.Pinned);
			var intHandle = GCHandle.Alloc((int)200, GCHandleType.Pinned);

			// global scope = dont apply to connection
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLoss_Send,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float,
				floatHandle.AddrOfPinnedObject()
			);
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLoss_Recv,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float,
				floatHandle.AddrOfPinnedObject()
			);

			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLag_Send,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				intHandle.AddrOfPinnedObject()
			);
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLag_Recv,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				intHandle.AddrOfPinnedObject()
			);

			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketReorder_Send,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float,
				floatHandle.AddrOfPinnedObject()
			);
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketReorder_Recv,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float,
				floatHandle.AddrOfPinnedObject()
			);
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketReorder_Time,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				intHandle.AddrOfPinnedObject()
			);

			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketDup_Send,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float,
				floatHandle.AddrOfPinnedObject()
			);
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketDup_Recv,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float,
				floatHandle.AddrOfPinnedObject()
			);
			SteamNetworkingUtils.SetConfigValue(
				ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketDup_TimeMax,
				ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
				IntPtr.Zero,
				ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				intHandle.AddrOfPinnedObject()
			);

			floatHandle.Free();
			intHandle.Free();
		}

		return result.ToArray();
	}
}
