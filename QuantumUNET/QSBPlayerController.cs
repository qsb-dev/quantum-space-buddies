using UnityEngine;

namespace QuantumUNET
{
	public class QSBPlayerController
	{
		public short PlayerControllerId = -1;
		public QSBNetworkIdentity UnetView;
		public GameObject Gameobject;
		public const int MaxPlayersPerClient = 32;

		internal const short kMaxLocalPlayers = 8;

		public QSBPlayerController()
		{
		}

		internal QSBPlayerController(GameObject go, short playerControllerId)
		{
			Gameobject = go;
			UnetView = go.GetComponent<QSBNetworkIdentity>();
			PlayerControllerId = playerControllerId;
		}

		public bool IsValid => PlayerControllerId != -1;

		public override string ToString()
		{
			return string.Format("ID={0} NetworkIdentity NetID={1} Player={2}", new object[]
			{
				PlayerControllerId,
				(UnetView == null) ? "null" : UnetView.NetId.ToString(),
				(Gameobject == null) ? "null" : Gameobject.name
			});
		}
	}
}