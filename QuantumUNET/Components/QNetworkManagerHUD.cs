using UnityEngine;

namespace QuantumUNET.Components
{
	public class QNetworkManagerHUD : MonoBehaviour
	{
		public QNetworkManager manager;
		public bool showGUI = true;
		public int offsetX;
		public int offsetY;

		private void Awake() => manager = GetComponent<QNetworkManager>();

		private void OnGUI()
		{
			if (showGUI)
			{
				var num = 10 + offsetX;
				var num2 = 40 + offsetY;
				var flag = manager.client == null || manager.client.connection == null || manager.client.connection.connectionId == -1;
				if (!manager.IsClientConnected() && !QNetworkServer.active)
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
					}
					else
					{
						GUI.Label(new Rect(num, num2, 200f, 20f),
							$"Connecting to {manager.networkAddress}:{manager.networkPort}..");
						num2 += 24;
						if (GUI.Button(new Rect(num, num2, 200f, 20f), "Cancel Connection Attempt"))
						{
							manager.StopClient();
						}
					}
				}
				else
				{
					if (QNetworkServer.active)
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
				if (manager.IsClientConnected() && !QClientScene.ready)
				{
					if (GUI.Button(new Rect(num, num2, 200f, 20f), "Client Ready"))
					{
						QClientScene.Ready(manager.client.connection);
						if (QClientScene.localPlayers.Count == 0)
						{
							QClientScene.AddPlayer(0);
						}
					}
					num2 += 24;
				}
				if (QNetworkServer.active || manager.IsClientConnected())
				{
					if (GUI.Button(new Rect(num, num2, 200f, 20f), "Stop"))
					{
						manager.StopHost();
					}
					num2 += 24;
				}
			}
		}
	}
}