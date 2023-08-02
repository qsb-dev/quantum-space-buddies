using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.API.Messages;

public class AddonCustomDataSyncMessage : QSBMessage<(uint playerId, string key, byte[] data)>
{
	public AddonCustomDataSyncMessage(uint playerId, string key, object data) : base((playerId, key, data.ToBytes())) { }
	public override void OnReceiveRemote() => QSBPlayerManager.GetPlayer(Data.playerId).SetCustomData(Data.key, Data.data.ToObject());
}
