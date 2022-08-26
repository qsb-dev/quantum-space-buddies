using Mirror;
using QSB.Localization;
using QSB.Menus;
using QSB.Messaging;
using QSB.Utility;

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

	public override void OnReceiveLocal()
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		Delay.RunFramesLater(10, KickPlayer);
	}

	private void KickPlayer()
	{
		//var connectionToClient = PlayerId.GetNetworkConnection();
		//connectionToClient.Disconnect();
		//Transport.activeTransport.OnServerDisconnected(connectionToClient.connectionId);
	}

	public override void OnReceiveRemote()
	{
		if (PlayerId != QSBPlayerManager.LocalPlayerId)
		{
			if (QSBPlayerManager.PlayerExists(PlayerId))
			{
				DebugLog.ToAll(string.Format(QSBLocalization.Current.PlayerWasKicked, QSBPlayerManager.GetPlayer(PlayerId).Name));
				return;
			}

			DebugLog.ToAll(string.Format(QSBLocalization.Current.PlayerWasKicked, PlayerId));
			return;
		}

		DebugLog.ToAll(string.Format(QSBLocalization.Current.KickedFromServer, Data));
		MenuManager.Instance.OnKicked(Data);

		NetworkClient.Disconnect();
	}
}