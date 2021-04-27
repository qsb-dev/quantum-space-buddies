using UnityEngine;

namespace QuantumUNET.Components
{
	public class QNetworkManagerHUD : MonoBehaviour
	{
		public QNetworkManager Manager;
		public bool ShowGUI = true;

		private void Awake()
			=> Manager = GetComponent<QNetworkManager>();

		private void OnGUI()
		{
			if (ShowGUI)
			{
				var xOffset = 10;
				var yOffset = 30;
				var flag = Manager.client == null || Manager.client.connection == null || Manager.client.connection.connectionId == -1;
				if (!Manager.IsClientConnected() && !QNetworkServer.active)
				{
					if (flag)
					{
						if (Application.platform != RuntimePlatform.WebGLPlayer)
						{
							if (GUI.Button(new Rect(xOffset, yOffset, 200f, 20f), "Host"))
							{
								Manager.StartHost();
							}
							yOffset += 20;
						}
						if (GUI.Button(new Rect(xOffset, yOffset, 105f, 20f), "Connect"))
						{
							Manager.StartClient();
						}
						Manager.networkAddress = GUI.TextField(new Rect(xOffset + 100, yOffset, 95f, 20f), Manager.networkAddress);
						yOffset += 20;
					}
					else
					{
						GUI.Label(new Rect(xOffset, yOffset, 200f, 20f),
							$"Connecting to {Manager.networkAddress}:{Manager.networkPort}..");
						yOffset += 24;
						if (GUI.Button(new Rect(xOffset, yOffset, 200f, 20f), "Cancel Connection Attempt"))
						{
							Manager.StopClient();
						}
					}
				}
				else
				{
					if (QNetworkServer.active)
					{
						var text = $"Hosting on port {Manager.networkPort}";
						if (Manager.useWebSockets)
						{
							text += " (using WebSockets)";
						}
						GUI.Label(new Rect(xOffset, yOffset, 300f, 20f), text);
						yOffset += 20;
					}
					if (Manager.IsClientConnected())
					{
						GUI.Label(new Rect(xOffset, yOffset, 300f, 20f), $"Connected to {Manager.networkAddress}, port {Manager.networkPort}");
						yOffset += 20;
					}
				}
				if (Manager.IsClientConnected() && !QClientScene.ready)
				{
					if (GUI.Button(new Rect(xOffset, yOffset, 200f, 20f), "Client Ready"))
					{
						QClientScene.Ready(Manager.client.connection);
						if (QClientScene.localPlayers.Count == 0)
						{
							QClientScene.AddPlayer(0);
						}
					}
					yOffset += 20;
				}
				if (QNetworkServer.active || Manager.IsClientConnected())
				{
					if (GUI.Button(new Rect(xOffset, yOffset, 200f, 20f), "Stop"))
					{
						Manager.StopHost();
					}
					yOffset += 20;
				}
			}
		}
	}
}