using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SteamTransport;

public static class Util
{
	public const int MaxMessages = 256; // same as fizzy steamworks

	private static int SendFlag2MirrorChannel(int sendFlag) => sendFlag switch
	{
		Constants.k_nSteamNetworkingSend_Reliable => Channels.Reliable,
		Constants.k_nSteamNetworkingSend_Unreliable => Channels.Unreliable
	};

	private static int MirrorChannel2SendFlag(int mirrorChannel) => mirrorChannel switch
	{
		Channels.Reliable => Constants.k_nSteamNetworkingSend_Reliable,
		Channels.Unreliable => Constants.k_nSteamNetworkingSend_Unreliable
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

	public static EResult Send(this HSteamNetConnection conn, ArraySegment<byte> segment, int channelId)
	{
		var handle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned); // prevent moving or gc when passing to native function
		var result = SteamNetworkingSockets.SendMessageToConnection(conn, handle.AddrOfPinnedObject() + segment.Offset, (uint)segment.Count, MirrorChannel2SendFlag(channelId), out _);
		handle.Free();
		return result;
	}

	public static (ArraySegment<byte> segment, int channelId) Receive(IntPtr ppOutMessage)
	{
		var msg = SteamNetworkingMessage_t.FromIntPtr(ppOutMessage);
		var segment = new ArraySegment<byte>(new byte[msg.m_cbSize]);
		Marshal.Copy(msg.m_pData, segment.Array, 0, msg.m_cbSize);
		var channelId = SendFlag2MirrorChannel(msg.m_nFlags);
		SteamNetworkingMessage_t.Release(ppOutMessage);
		return (segment, channelId);
	}

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

		// 20% change of doing all, all delays are 200 ms. leads to about 1 second of rtt ping if enabled on both ends.
		if (transport.DoFakeNetworkErrors)
		{
			// global scope = dont apply to connection
			static void SetConfigValue(ESteamNetworkingConfigValue key, object value)
			{
				var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
				SteamNetworkingUtils.SetConfigValue(
					key,
					ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
					IntPtr.Zero,
					handle.Target switch
					{
						int => ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
						float => ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float
					},
					handle.AddrOfPinnedObject()
				);
				handle.Free();
			}

			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLoss_Send, (float)20);
			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLoss_Recv, (float)20);

			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLag_Send, (int)200);
			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketLag_Recv, (int)200);

			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketReorder_Send, (float)20);
			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketReorder_Recv, (float)20);
			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketReorder_Time, (int)200);

			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketDup_Send, (float)20);
			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketDup_Recv, (float)20);
			SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_FakePacketDup_TimeMax, (int)200);
		}

		return result.ToArray();
	}
}
