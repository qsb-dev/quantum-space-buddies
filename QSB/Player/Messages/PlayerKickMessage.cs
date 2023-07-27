using Mirror;
using QSB.HUD;
using QSB.Localization;
using QSB.Menus;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player.Messages;

/// <summary>
/// always sent by host
/// </summary>
internal class PlayerKickMessage : QSBMessage<string>
{
	private uint PlayerId;

	public PlayerKickMessage(uint playerId, string reason) : base(reason) =>
		PlayerId = playerId;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PlayerId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerId = reader.Read<uint>();
	}

	public override void OnReceiveRemote()
	{
		if (PlayerId != QSBPlayerManager.LocalPlayerId)
		{
			if (QSBPlayerManager.PlayerExists(PlayerId))
			{
				MultiplayerHUDManager.Instance.WriteSystemMessage(string.Format(QSBLocalization.Current.PlayerWasKicked, QSBPlayerManager.GetPlayer(PlayerId).Name), Color.red);
				return;
			}

			MultiplayerHUDManager.Instance.WriteSystemMessage(string.Format(QSBLocalization.Current.PlayerWasKicked, PlayerId), Color.red);
			return;
		}

		MultiplayerHUDManager.Instance.WriteSystemMessage(string.Format(QSBLocalization.Current.KickedFromServer, Data), Color.red);
		MenuManager.Instance.OnKicked(Data);

		NetworkClient.Disconnect();
	}
}