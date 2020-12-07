using UnityEngine;

namespace QuantumUNET
{
	public class QSBNetworkManagerHUD : MonoBehaviour
	{
		private void Awake()
		{
			manager = GetComponent<QSBNetworkManagerUNET>();
		}

		private void OnGUI()
		{
			if (showGUI)
			{
				var num = 10 + offsetX;
				var num2 = 40 + offsetY;
				var flag = manager.client == null || manager.client.connection == null || manager.client.connection.connectionId == -1;
				if (!manager.IsClientConnected() && !QSBNetworkServer.active)
				{
					if (flag)
					{
						if (Application.platform != RuntimePlatform.WebGLPlayer)
						{
							if (GUI.Button(new Rect(num, num2, 200f, 20f), "Host"))
							{
								manager.StartHost();
							}
							num2 += 24;
						}
						if (GUI.Button(new Rect(num, num2, 105f, 20f), "Connect"))
						{
							manager.StartClient();
						}
						manager.networkAddress = GUI.TextField(new Rect(num + 100, num2, 95f, 20f), manager.networkAddress);
						num2 += 24;
						if (Application.platform == RuntimePlatform.WebGLPlayer)
						{
							GUI.Box(new Rect(num, num2, 200f, 25f), "(  WebGL cannot be server  )");
							num2 += 24;
						}
						else
						{
							if (GUI.Button(new Rect(num, num2, 200f, 20f), "LAN Server Only"))
							{
								manager.StartServer();
							}
							num2 += 24;
						}
					}
					else
					{
						GUI.Label(new Rect(num, num2, 200f, 20f), string.Concat(new object[]
						{
						"Connecting to ",
						manager.networkAddress,
						":",
						manager.networkPort,
						".."
						}));
						num2 += 24;
						if (GUI.Button(new Rect(num, num2, 200f, 20f), "Cancel Connection Attempt"))
						{
							manager.StopClient();
						}
					}
				}
				else
				{
					if (QSBNetworkServer.active)
					{
						var text = $"Hosting on port {manager.networkPort}";
						if (manager.useWebSockets)
						{
							text += " (using WebSockets)";
						}
						GUI.Label(new Rect(num, num2, 300f, 20f), text);
						num2 += 24;
					}
					if (manager.IsClientConnected())
					{
						GUI.Label(new Rect(num, num2, 300f, 20f), $"Connected to {manager.networkAddress}, port {manager.networkPort}");
						num2 += 24;
					}
				}
				if (manager.IsClientConnected() && !QSBClientScene.ready)
				{
					if (GUI.Button(new Rect(num, num2, 200f, 20f), "Client Ready"))
					{
						QSBClientScene.Ready(manager.client.connection);
						if (QSBClientScene.localPlayers.Count == 0)
						{
							QSBClientScene.AddPlayer(0);
						}
					}
					num2 += 24;
				}
				if (QSBNetworkServer.active || manager.IsClientConnected())
				{
					if (GUI.Button(new Rect(num, num2, 200f, 20f), "Stop"))
					{
						manager.StopHost();
					}
					num2 += 24;
				}
				if (!QSBNetworkServer.active && !manager.IsClientConnected() && flag)
				{
					num2 += 10;
					if (Application.platform == RuntimePlatform.WebGLPlayer)
					{
						GUI.Box(new Rect(num - 5, num2, 220f, 25f), "(WebGL cannot use Match Maker)");
					}
				}
			}
		}

		public QSBNetworkManagerUNET manager;

		[SerializeField]
		public bool showGUI = true;

		[SerializeField]
		public int offsetX;

		[SerializeField]
		public int offsetY;

		private bool m_ShowServer;
	}
}