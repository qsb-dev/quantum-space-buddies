using System;
using System.Linq;
using QSB.API.Messages;
using QSB.Messaging;
using QSB.Player;
using UnityEngine.Events;

namespace QSB.API;

public class QSBAPI : IQSBAPI
{
	public uint GetLocalPlayerID() => QSBPlayerManager.LocalPlayerId;
	public string GetPlayerName(uint playerId) => QSBPlayerManager.GetPlayer(playerId).Name;
	public uint[] GetPlayerIDs() => QSBPlayerManager.PlayerList.Select(x => x.PlayerId).ToArray();

	public UnityEvent<uint> OnPlayerJoin() => QSBAPIEvents.OnPlayerJoinEvent;

	public UnityEvent<uint> OnPlayerLeave() => QSBAPIEvents.OnPlayerLeaveEvent;

	public void SetCustomData<T>(uint playerId, string key, T data) => QSBPlayerManager.GetPlayer(playerId).SetCustomData(key, data);
	public T GetCustomData<T>(uint playerId, string key) => QSBPlayerManager.GetPlayer(playerId).GetCustomData<T>(key);

	public void SendMessage<T>(string messageType, T data, bool receiveLocally = false)
		=> new AddonDataMessage(messageType, data, receiveLocally).Send();

	public void RegisterHandler<T>(string messageType, Action<uint, T> handler)
		=> AddonDataManager.RegisterHandler(messageType, handler);
}

internal static class QSBAPIEvents
{
	static QSBAPIEvents()
	{
		QSBPlayerManager.OnAddPlayer += player => OnPlayerJoinEvent.Invoke(player.PlayerId);
		QSBPlayerManager.OnRemovePlayer += player => OnPlayerLeaveEvent.Invoke(player.PlayerId);
	}

	public static UnityEvent<uint> OnPlayerJoinEvent = new PlayerEvent();
	public static UnityEvent<uint> OnPlayerLeaveEvent = new PlayerEvent();
}

// i hate OOP sometimes
internal class PlayerEvent : UnityEvent<uint> { }
