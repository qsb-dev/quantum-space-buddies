// https://github.com/MirrorNetworking/Mirror/blob/master/Assets/Mirror/Core/Transport.cs
// https://partner.steamgames.com/doc/api/ISteamNetworkingSockets
// https://partner.steamgames.com/doc/api/steamnetworkingtypes

using Mirror;
using Steamworks;
using System;

namespace SteamTransport;

public class SteamTransport : Transport
{
	private Server _server;
	private Client _client;

	/// <summary>
	/// logs will verbosely go here. must be set
	/// </summary>
	public Action<string> Log;
	/// <summary>
	/// if set, will use this ip address and port for listening/connecting
	/// </summary>
	public string TestIpAddress;

	/// <summary>
	/// timeout in seconds when connecting, and timeout before detecting a loss in connection
	/// </summary>
	// TODO implement
	public int Timeout;
	/// <summary>
	/// whether or not to simulate fake packet loss, lag, reorder, and dup
	/// </summary>
	// TODO implement
	public bool DoFakePacket;

	public override bool Available() => true;

	public override bool ClientConnected() => _client.IsConnected;

	public override void ClientConnect(string address)
	{
		_client = new Client(this);
		_client.Connect(address);
	}

	public override void ClientSend(ArraySegment<byte> segment, int channelId = 0)
	{
		_client.Send(segment, channelId);
	}

	public override void ClientDisconnect()
	{
		// mirror seems to call this when its null sometimes
		if (_client != null)
		{
			_client.Close();
			_client = null;
		}
	}

	public override Uri ServerUri() => throw new NotImplementedException("dont need to implement this i think");

	public override bool ServerActive() => _server != null && _server.IsListening;

	public override void ServerStart()
	{
		_server = new Server(this);
		_server.StartListening();
	}

	public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
	{
		_server.Send(connectionId, segment, channelId);
	}

	public override void ServerDisconnect(int connectionId)
	{
		_server.Disconnect(connectionId);
	}

	public override string ServerGetClientAddress(int connectionId) => throw new NotImplementedException("dont need to implement this i think");

	public override void ServerStop()
	{
		// mirror seems to call this when its null sometimes
		if (_server != null)
		{
			_server.Close();
			_server = null;
		}
	}

	public override int GetMaxPacketSize(int channelId = 0) => Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend;

	public override void Shutdown()
	{
		// gotta null check because might be only one existing
		if (_client != null)
		{
			_client.Close();
			_client = null;
		}
		if (_server != null)
		{
			_server.Close();
			_server = null;
		}
	}

	// all of these update functions run all the time, so we must null check
	public override void ClientEarlyUpdate()
	{
		_client?.Receive();
	}

	public override void ServerEarlyUpdate()
	{
		_server?.Receive();
	}

	public override void ClientLateUpdate()
	{
		_client?.Flush();
	}

	public override void ServerLateUpdate()
	{
		_server?.Flush();
	}
}
