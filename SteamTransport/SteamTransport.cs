using Mirror;
using System;

namespace SteamTransport;

public class SteamTransport : Transport
{
	public override bool Available() => throw new NotImplementedException();

	public override bool ClientConnected() => throw new NotImplementedException();

	public override void ClientConnect(string address)
	{
		throw new NotImplementedException();
	}

	public override void ClientSend(ArraySegment<byte> segment, int channelId = 0)
	{
		throw new NotImplementedException();
	}

	public override void ClientDisconnect()
	{
		throw new NotImplementedException();
	}

	public override Uri ServerUri() => throw new NotImplementedException();

	public override bool ServerActive() => throw new NotImplementedException();

	public override void ServerStart()
	{
		throw new NotImplementedException();
	}

	public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
	{
		throw new NotImplementedException();
	}

	public override void ServerDisconnect(int connectionId)
	{
		throw new NotImplementedException();
	}

	public override string ServerGetClientAddress(int connectionId) => throw new NotImplementedException();

	public override void ServerStop()
	{
		throw new NotImplementedException();
	}

	public override int GetMaxPacketSize(int channelId = 0) => throw new NotImplementedException();

	public override void Shutdown()
	{
		throw new NotImplementedException();
	}
}
