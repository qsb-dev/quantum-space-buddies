using Steamworks;
using Steamworks.Data;
using System;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class NextServer : NextCommon, IServer
	{
		private event Action<int> OnConnected;
		private event Action<int, byte[], int> OnReceivedData;
		private event Action<int> OnDisconnected;
		private event Action<int, Exception> OnReceivedError;

		private BidirectionalDictionary<Connection, int> connToMirrorID;
		private BidirectionalDictionary<SteamId, int> steamIDToMirrorID;
		private int maxConnections;
		private int nextConnectionID;

		private FizzySocketManager listenSocket;

		private NextServer(int maxConnections)
		{
			this.maxConnections = maxConnections;
			connToMirrorID = new BidirectionalDictionary<Connection, int>();
			steamIDToMirrorID = new BidirectionalDictionary<SteamId, int>();
			nextConnectionID = 1;
			SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;
		}

		public static NextServer CreateServer(FizzyFacepunch transport, int maxConnections)
		{
			NextServer s = new NextServer(maxConnections);

			s.OnConnected += (id) => transport.OnServerConnected.Invoke(id);
			s.OnDisconnected += (id) => transport.OnServerDisconnected.Invoke(id);
			s.OnReceivedData += (id, data, ch) => transport.OnServerDataReceived.Invoke(id, new ArraySegment<byte>(data), ch);
			s.OnReceivedError += (id, exception) => transport.OnServerError.Invoke(id, exception);

			if (!SteamClient.IsValid)
			{
				Debug.LogError("SteamWorks not initialized.");
			}

			s.Host();

			return s;
		}

		private void Host()
		{
			listenSocket = SteamNetworkingSockets.CreateRelaySocket<FizzySocketManager>();
			listenSocket.ForwardMessage = OnMessageReceived;
		}

		private void OnConnectionStatusChanged(Connection conn, ConnectionInfo info)
		{
			ulong clientSteamID = info.Identity.SteamId;
			if (info.State == ConnectionState.Connecting)
			{
				if (connToMirrorID.Count >= maxConnections)
				{
					Debug.LogError($"Incoming connection {clientSteamID} would exceed max connection count. Rejecting.");
					conn.Close(false, 0, "Max Connection Count");
					return;
				}

				Result res;

				if ((res = conn.Accept()) == Result.OK)
				{
					Debug.LogError($"Accepting connection {clientSteamID}");
				}
				else
				{
					Debug.LogError($"Connection {clientSteamID} could not be accepted: {res.ToString()}");
				}
			}
			else if (info.State == ConnectionState.Connected)
			{
				int connectionId = nextConnectionID++;
				connToMirrorID.Add(conn, connectionId);
				steamIDToMirrorID.Add(clientSteamID, connectionId);
				OnConnected.Invoke(connectionId);
				Debug.LogError($"Client with SteamID {clientSteamID} connected. Assigning connection id {connectionId}");
			}
			else if (info.State == ConnectionState.ClosedByPeer)
			{
				if (connToMirrorID.TryGetValue(conn, out int connId))
				{
					InternalDisconnect(connId, conn);
				}
			}
			else
			{
				Debug.LogError($"Connection {clientSteamID} state changed: {info.State.ToString()}");
			}
		}

		private void InternalDisconnect(int connId, Connection socket)
		{
			OnDisconnected.Invoke(connId);
			socket.Close(false, 0, "Graceful disconnect");
			connToMirrorID.Remove(connId);
			steamIDToMirrorID.Remove(connId);
			Debug.LogError($"Client with SteamID {connId} disconnected.");
		}

		public void Disconnect(int connectionId)
		{
			if (connToMirrorID.TryGetValue(connectionId, out Connection conn))
			{
				Debug.LogError($"Connection id {connectionId} disconnected.");
				conn.Close(false, 0, "Disconnected by server");
				steamIDToMirrorID.Remove(connectionId);
				connToMirrorID.Remove(connectionId);
				OnDisconnected(connectionId);
			}
			else
			{
				Debug.LogWarning("Trying to disconnect unknown connection id: " + connectionId);
			}
		}

		public void FlushData()
		{
			foreach (Connection conn in connToMirrorID.FirstTypes)
			{
				conn.Flush();
			}
		}

		public void ReceiveData()
		{
			listenSocket.Receive(MAX_MESSAGES);
		}

		private void OnMessageReceived(Connection conn, IntPtr dataPtr, int size)
		{
			(byte[] data, int ch) = ProcessMessage(dataPtr, size);
			OnReceivedData(connToMirrorID[conn], data, ch);
		}

		public void Send(int connectionId, byte[] data, int channelId)
		{
			if (connToMirrorID.TryGetValue(connectionId, out Connection conn))
			{
				Result res = SendSocket(conn, data, channelId);

				if (res == Result.NoConnection || res == Result.InvalidParam)
				{
					Debug.LogError($"Connection to {connectionId} was lost.");
					InternalDisconnect(connectionId, conn);
				}
				else if (res != Result.OK)
				{
					Debug.LogError($"Could not send: {res.ToString()}");
				}
			}
			else
			{
				Debug.LogError("Trying to send on unknown connection: " + connectionId);
				OnReceivedError.Invoke(connectionId, new Exception("ERROR Unknown Connection"));
			}
		}

		public string ServerGetClientAddress(int connectionId)
		{
			if (steamIDToMirrorID.TryGetValue(connectionId, out SteamId steamId))
			{
				return steamId.ToString();
			}
			else
			{
				Debug.LogError("Trying to get info on unknown connection: " + connectionId);
				OnReceivedError.Invoke(connectionId, new Exception("ERROR Unknown Connection"));
				return string.Empty;
			}
		}

		public void Shutdown()
		{
			if (listenSocket != null)
			{
				SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;
				listenSocket.Close();
			}
		}
	}
}