using UnityEngine;

namespace QSB.QuantumUNET
{
	public class QSBPlayerController
	{
		public QSBPlayerController()
		{
		}

		internal QSBPlayerController(GameObject go, short playerControllerId)
		{
			gameObject = go;
			unetView = go.GetComponent<QSBNetworkIdentity>();
			this.playerControllerId = playerControllerId;
		}

		public bool IsValid => playerControllerId != -1;

		public override string ToString()
		{
			return string.Format("ID={0} NetworkIdentity NetID={1} Player={2}", new object[]
			{
				playerControllerId,
				(!(unetView != null)) ? "null" : unetView.NetId.ToString(),
				(!(gameObject != null)) ? "null" : gameObject.name
			});
		}

		internal const short kMaxLocalPlayers = 8;

		public short playerControllerId = -1;

		public QSBNetworkIdentity unetView;

		public GameObject gameObject;

		public const int MaxPlayersPerClient = 32;
	}
}