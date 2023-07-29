using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QSB.API.Messages;
using QSB.Messaging;
using QSB.Player;

namespace QSB.API;

internal class QSBAPI : IQSBAPI
{
	public uint GetLocalPlayerID() => QSBPlayerManager.LocalPlayerId;

	public void SetCustomData<T>(uint playerId, string key, T data) => QSBPlayerManager.GetPlayer(playerId).SetCustomData(key, data);
	public T GetCustomData<T>(uint playerId, string key) => QSBPlayerManager.GetPlayer(playerId).GetCustomData<T>(key);

	public void SendMessage<T>(string messageType, T data)
	{
		new AddonDataMessage(messageType, data).Send();
	}

	public void RegisterHandler<T>(string messageType, Action<T> action)
	{
		AddonDataManager.RegisterHandler(messageType, action);
	}
}
