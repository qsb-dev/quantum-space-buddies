using Steamworks;
using System;
using UnityEngine;

namespace Mirror.FizzySteam;

public class FizzySteamworks : Transport
{
	private const string STEAM_SCHEME = "steam";

	private static SteamClient client;
	private static SteamServer server;

	[SerializeField]
	public EP2PSend[] Channels = new EP2PSend[2] { EP2PSend.k_EP2PSendReliable, EP2PSend.k_EP2PSendUnreliableNoDelay };

	[Tooltip("Timeout for connecting in seconds.")]
	public int Timeout = 25;

	private void OnEnable()
	{
		Debug.Assert(Channels != null && Channels.Length > 0, "No channel configured for FizzySteamworks.");
		Invoke(nameof(InitRelayNetworkAccess), 1f);
	}

	public override void ClientEarlyUpdate()
	{
		if (enabled)
		{
			client?.ReceiveData();
		}
	}

	public override void ServerEarlyUpdate()
	{
		if (enabled)
		{
			server?.ReceiveData();
		}
	}

	public override void ClientLateUpdate()
	{
		if (enabled)
		{
			client?.FlushData();
		}
	}

	public override void ServerLateUpdate()
	{
		if (enabled)
		{
			server?.FlushData();
		}
	}

	public override bool ClientConnected() => ClientActive() && client.Connected;
	public override void ClientConnect(string address)
	{
		try
		{
			SteamNetworkingUtils.InitRelayNetworkAccess();
			InitRelayNetworkAccess();

			if (ServerActive())
			{
				Debug.LogError("Transport already running as server!");
				return;
			}

			if (!ClientActive() || client.Error)
			{
				Debug.Log($"Starting client [SteamSockets], target address {address}.");
				client = SteamClient.CreateClient(this, address);
			}
			else
			{
				Debug.LogError("Client already running!");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception: " + ex.Message + ". Client could not be started.");
			OnClientDisconnected.Invoke();
		}
	}

	public override void ClientConnect(Uri uri)
	{
		if (uri.Scheme != STEAM_SCHEME)
		{
			throw new ArgumentException($"Invalid url {uri}, use {STEAM_SCHEME}://SteamID instead", nameof(uri));
		}

		ClientConnect(uri.Host);
	}

	public override void ClientSend(ArraySegment<byte> segment, int channelId)
	{
		var data = new byte[segment.Count];
		Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
		client.Send(data, channelId);
	}

	public override void ClientDisconnect()
	{
		if (ClientActive())
		{
			Shutdown();
		}
	}

	public bool ClientActive() => client != null;

	public override bool ServerActive() => server != null;

	public override void ServerStart()
	{
		try
		{
			SteamNetworkingUtils.InitRelayNetworkAccess();
			InitRelayNetworkAccess();

			if (ClientActive())
			{
				Debug.LogError("Transport already running as client!");
				return;
			}

			if (!ServerActive())
			{
				Debug.Log($"Starting server [SteamSockets].");
				server = SteamServer.CreateServer(this, NetworkManager.singleton.maxConnections);
			}
			else
			{
				Debug.LogError("Server already started!");
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return;
		}
	}

	public override Uri ServerUri()
	{
		var steamBuilder = new UriBuilder
		{
			Scheme = STEAM_SCHEME,
			Host = SteamUser.GetSteamID().m_SteamID.ToString()
		};

		return steamBuilder.Uri;
	}

	public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
	{
		if (ServerActive())
		{
			var data = new byte[segment.Count];
			Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
			server.Send(connectionId, data, channelId);
		}
	}
	public override void ServerDisconnect(int connectionId)
	{
		if (ServerActive())
		{
			server.Disconnect(connectionId);
		}
	}
	public override string ServerGetClientAddress(int connectionId) => ServerActive() ? server.ServerGetClientAddress(connectionId) : string.Empty;
	public override void ServerStop()
	{
		if (ServerActive())
		{
			Shutdown();
		}
	}

	public override void Shutdown()
	{
		if (server != null)
		{
			server.Shutdown();
			server = null;
			Debug.Log("Transport shut down - was server.");
		}

		if (client != null)
		{
			client.Disconnect();
			client = null;
			Debug.Log("Transport shut down - was client.");
		}
	}

	public override int GetMaxPacketSize(int channelId)
		=> Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend;

	public override bool Available()
	{
		try
		{
			SteamNetworkingUtils.InitRelayNetworkAccess();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void InitRelayNetworkAccess()
	{
		try
		{
			SteamNetworkingUtils.InitRelayNetworkAccess();
		}
		catch { }
	}

	private void OnDestroy()
		=> Shutdown();
}
