using Mirror;
using QSB.Player;
using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB;
public class Ghostbuster : MonoBehaviour, IAddComponentOnStart
{
	private const int UpdateInterval = 60;

	private int _updateCount;

	public void Update()
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return;
		}

		if (!QSBCore.IsHost)
		{
			return;
		}

		if (_updateCount != UpdateInterval)
		{
			_updateCount++;
			return;
		}
		else
		{
			_updateCount = 0;
		}

		var _ghostPlayers = new List<PlayerInfo>();

		foreach (var player in QSBPlayerManager.PlayerList)
		{
			var isGhost = false;

			var networkIdentity = player.TransformSync.netIdentity;

			if (networkIdentity.connectionToClient == null)
			{
				isGhost = true;
			}
			else if (!NetworkServer.connections.ContainsValue(networkIdentity.connectionToClient))
			{
				isGhost = true;
			}

			if (isGhost)
			{
				// WE GOT ONE!!!!!!
				_ghostPlayers.Add(player);
			}
		}

		foreach (var item in _ghostPlayers)
		{
			DebugLog.ToConsole($"Deleting playerId:{item.PlayerId} - It's a ghooOoOoOooost! (hopefully)", OWML.Common.MessageType.Info);
			NetworkServer.DestroyPlayerForConnection(item.TransformSync.netIdentity.connectionToClient);
		}
	}
}
