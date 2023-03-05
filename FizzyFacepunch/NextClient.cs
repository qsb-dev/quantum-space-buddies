using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class NextClient : NextCommon, IClient
	{
		public bool Connected { get; private set; }
		public bool Error { get; private set; }

		private TimeSpan ConnectionTimeout;

		private event Action<byte[], int> OnReceivedData;
		private event Action OnConnected;
		private event Action OnDisconnected;

		private CancellationTokenSource cancelToken;
		private TaskCompletionSource<Task> connectedComplete;
		private SteamId hostSteamID = 0;
		private FizzyConnectionManager HostConnectionManager;
		private Connection HostConnection => HostConnectionManager.Connection;
		private List<Action> BufferedData;

		private NextClient(FizzyFacepunch transport)
		{
			ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
			BufferedData = new List<Action>();
		}

		public static NextClient CreateClient(FizzyFacepunch transport, string host)
		{
			NextClient c = new NextClient(transport);

			c.OnConnected += () => transport.OnClientConnected.Invoke();
			c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
			c.OnReceivedData += (data, ch) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), ch);

			if (SteamClient.IsValid)
			{
				c.Connect(host);
			}
			else
			{
				Debug.LogError("SteamWorks not initialized");
				c.OnConnectionFailed();
			}

			return c;
		}

		private async void Connect(string host)
		{
			cancelToken = new CancellationTokenSource();
			SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;

			try
			{
				hostSteamID = UInt64.Parse(host);
				connectedComplete = new TaskCompletionSource<Task>();
				OnConnected += SetConnectedComplete;
				HostConnectionManager = SteamNetworkingSockets.ConnectRelay<FizzyConnectionManager>(hostSteamID);
				HostConnectionManager.ForwardMessage = OnMessageReceived;
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
					OnConnectionFailed();
				}

				OnConnected -= SetConnectedComplete;
			}
			catch (FormatException)
			{
				Debug.LogError($"Connection string was not in the right format. Did you enter a SteamId?");
				Error = true;
				OnConnectionFailed();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				Error = true;
				OnConnectionFailed();
			}
			finally
			{
				if (Error)
				{
					Debug.LogError("Connection failed.");
					OnConnectionFailed();
				}
			}
		}

		private void OnMessageReceived(IntPtr dataPtr, int size)
		{
			(byte[] data, int ch) = ProcessMessage(dataPtr, size);
			if (Connected)
			{
				OnReceivedData(data, ch);
			}
			else
			{
				BufferedData.Add(() => OnReceivedData(data, ch));
			}
		}

		private void OnConnectionStatusChanged(Connection conn, ConnectionInfo info)
		{
			ulong clientSteamID = info.Identity.SteamId;
			if (info.State == ConnectionState.Connected)
			{
				Connected = true;
				OnConnected.Invoke();
				Debug.LogError("Connection established.");

				if (BufferedData.Count > 0)
				{
					Debug.LogError($"{BufferedData.Count} received before connection was established. Processing now.");
					{
						foreach (Action a in BufferedData)
						{
							a();
						}
					}
				}
			}
			else if (info.State == ConnectionState.ClosedByPeer)
			{
				Connected = false;
				OnDisconnected.Invoke();
				Debug.LogError("Disconnected.");
				conn.Close(false, 0, "Disconnected");
			}
			else
			{
				Debug.LogError($"Connection state changed: {info.State.ToString()}");
			}
		}

		public void Disconnect()
		{
			cancelToken?.Cancel();
			SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;

			if (HostConnectionManager != null)
			{
				Debug.LogError("Sending Disconnect message");
				HostConnection.Close(false, 0, "Graceful disconnect");
				HostConnectionManager = null;
			}
		}

		public void ReceiveData()
		{
			HostConnectionManager.Receive(MAX_MESSAGES);
		}

		public void Send(byte[] data, int channelId)
		{
			Result res = SendSocket(HostConnection, data, channelId);

			if (res != Result.OK)
			{
				Debug.LogError($"Could not send: {res.ToString()}");
			}
		}

		private void SetConnectedComplete() => connectedComplete.SetResult(connectedComplete.Task);
		private void OnConnectionFailed() => OnDisconnected.Invoke();
		public void FlushData()
		{
			HostConnection.Flush();
		}
	}
}