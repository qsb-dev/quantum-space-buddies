using System.ComponentModel;
using UnityEngine;

namespace QuantumUNET.Components
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class QNetworkManagerHUD : MonoBehaviour
	{
		private void Awake()
		{
			this.manager = base.GetComponent<QNetworkManager>();
		}

		private void OnGUI()
		{
			if (this.showGUI)
			{
				int num = 10 + this.offsetX;
				int num2 = 40 + this.offsetY;
				bool flag = this.manager.client == null || this.manager.client.connection == null || this.manager.client.connection.connectionId == -1;
				if (!this.manager.IsClientConnected() && !QNetworkServer.active)
				{
					if (flag)
					{
						if (Application.platform != RuntimePlatform.WebGLPlayer)
						{
							if (GUI.Button(new Rect((float)num, (float)num2, 200f, 20f), "LAN Host(H)"))
							{
								this.manager.StartHost();
							}
							num2 += 24;
						}
						if (GUI.Button(new Rect((float)num, (float)num2, 105f, 20f), "LAN Client(C)"))
						{
							this.manager.StartClient();
						}
						this.manager.networkAddress = GUI.TextField(new Rect((float)(num + 100), (float)num2, 95f, 20f), this.manager.networkAddress);
						num2 += 24;
						if (Application.platform == RuntimePlatform.WebGLPlayer)
						{
							GUI.Box(new Rect((float)num, (float)num2, 200f, 25f), "(  WebGL cannot be server  )");
							num2 += 24;
						}
						else
						{
							if (GUI.Button(new Rect((float)num, (float)num2, 200f, 20f), "LAN Server Only(S)"))
							{
								this.manager.StartServer();
							}
							num2 += 24;
						}
					}
					else
					{
						GUI.Label(new Rect((float)num, (float)num2, 200f, 20f), string.Concat(new object[]
						{
							"Connecting to ",
							this.manager.networkAddress,
							":",
							this.manager.networkPort,
							".."
						}));
						num2 += 24;
						if (GUI.Button(new Rect((float)num, (float)num2, 200f, 20f), "Cancel Connection Attempt"))
						{
							this.manager.StopClient();
						}
					}
				}
				else
				{
					if (QNetworkServer.active)
					{
						string text = "Server: port=" + this.manager.networkPort;
						GUI.Label(new Rect((float)num, (float)num2, 300f, 20f), text);
						num2 += 24;
					}
					if (this.manager.IsClientConnected())
					{
						GUI.Label(new Rect((float)num, (float)num2, 300f, 20f), string.Concat(new object[]
						{
							"Client: address=",
							this.manager.networkAddress,
							" port=",
							this.manager.networkPort
						}));
						num2 += 24;
					}
				}

				if (this.manager.IsClientConnected() && !QClientScene.ready)
				{
					if (GUI.Button(new Rect((float)num, (float)num2, 200f, 20f), "Client Ready"))
					{
						QClientScene.Ready(this.manager.client.connection);
						if (QClientScene.localPlayers.Count == 0)
						{
							QClientScene.AddPlayer(0);
						}
					}
					num2 += 24;
				}

				if (QNetworkServer.active || this.manager.IsClientConnected())
				{
					if (GUI.Button(new Rect((float)num, (float)num2, 200f, 20f), "Stop (X)"))
					{
						this.manager.StopHost();
					}
					num2 += 24;
				}
			}
		}

		public QNetworkManager manager;

		[SerializeField]
		public bool showGUI = true;

		[SerializeField]
		public int offsetX;

		[SerializeField]
		public int offsetY;

		private bool m_ShowServer;
	}
}
