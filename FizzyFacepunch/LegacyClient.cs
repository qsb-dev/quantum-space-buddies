using Steamworks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class LegacyClient : LegacyCommon, IClient
	{
		public bool Error { get; private set; }
		public bool Connected { get; private set; }

		private event Action<byte[], int> OnReceivedData;
		private event Action OnConnected;
		private event Action OnDisconnected;

		private TimeSpan ConnectionTimeout;

		private SteamId hostSteamID = 0;
		private TaskCompletionSource<Task> connectedComplete;
		private CancellationTokenSource cancelToken;

		private LegacyClient(FizzyFacepunch transport) : base(transport)
		{
			ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
		}

		public static LegacyClient CreateClient(FizzyFacepunch transport, string host)
		{
			LegacyClient c = new LegacyClient(transport);

			c.OnConnected += () => transport.OnClientConnected.Invoke();
			c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
			c.OnReceivedData += (data, channel) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), channel);

			if (SteamClient.IsValid)
			{
				c.Connect(host);
			}
			else
			{
				Debug.LogError("SteamWorks not initialized.");
				c.OnConnectionFailed(new SteamId());
			}

			return c;
		}

		private async void Connect(string host)
		{
			cancelToken = new CancellationTokenSource();
			try
			{
				hostSteamID = ulong.Parse(host);
				connectedComplete = new TaskCompletionSource<Task>();

				OnConnected += SetConnectedComplete;
				SendInternal(hostSteamID, InternalMessages.CONNECT);
				Task connectedCompleteTask = connectedComplete.Task;
				Task timeOutTask = Task.Delay(ConnectionTimeout, cancelToken.Token);

				if (await Task.WhenAny(connectedCompleteTask, timeOutTask) != connectedCompleteTask)
				{
					if (cancelToken.IsCancellationRequested)
					{
						Debug.LogError($"The connection attempt was cancelled.");
					}
					else if (timeOutTask.IsCompleted)
					{
						Debug.LogError($"Connection to {host} timed out.");
					}
					OnConnected -= SetConnectedComplete;
					Debug.LogError("Connection timed out.");
					OnConnectionFailed(hostSteamID);
				}

				OnConnected -= SetConnectedComplete;
			}
			catch (FormatException)
			{
				Debug.LogError($"Connection string was not in the right format. Did you enter a SteamId?");
				Error = true;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				Error = true;
			}
			finally
			{
				if (Error)
				{
					OnConnectionFailed(new SteamId());
				}
			}
		}

		public void Disconnect()
		{
			Debug.LogError("Sending Disconnect message");
			SendInternal(hostSteamID, InternalMessages.DISCONNECT);
			Dispose();
			cancelToken?.Cancel();

			WaitForClose(hostSteamID);
		}

		private void SetConnectedComplete() => connectedComplete.SetResult(connectedComplete.Task);

		protected override void OnReceiveData(byte[] data, SteamId clientSteamID, int channel)
		{
			if (clientSteamID != hostSteamID)
			{
				Debug.LogError("Received a message from an unknown");
				return;
			}

			OnReceivedData.Invoke(data, channel);
		}

		protected override void OnNewConnection(SteamId id)
		{
			if (hostSteamID == id)
			{
				SteamNetworking.AcceptP2PSessionWithUser(id);
			}
			else
			{
				Debug.LogError("P2P Acceptance Request from unknown host ID.");
			}
		}

		protected override void OnReceiveInternalData(InternalMessages type, SteamId clientSteamID)
		{
			switch (type)
			{
				case InternalMessages.ACCEPT_CONNECT:
					if (!Connected)
					{
						Connected = true;
						Debug.LogError("Connection established.");
						OnConnected.Invoke();
					}
					break;
				case InternalMessages.DISCONNECT:
					if (Connected)
					{
						Connected = false;
						Debug.LogError("Disconnected.");
						OnDisconnected.Invoke();
					}
					break;
				default:
					Debug.LogError("Received unknown message type");
					break;
			}
		}

		public void Send(byte[] data, int channelId) => Send(hostSteamID, data, channelId);
		protected override void OnConnectionFailed(SteamId remoteId) => OnDisconnected.Invoke();
		public void FlushData() { }
	}
}