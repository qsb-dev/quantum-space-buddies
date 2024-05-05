using Mirror;
using OWML.Common;
using QSB.API.Messages;
using QSB.Messaging;
using QSB.Player;
using System;
using System.Linq;
using QSB.HUD;
using QSB.HUD.Messages;
using UnityEngine.Events;
using UnityEngine;

namespace QSB.API;

public class QSBAPI : IQSBAPI
{
	public void RegisterRequiredForAllPlayers(IModBehaviour mod)
	{
		var uniqueName = mod.ModHelper.Manifest.UniqueName;
		QSBCore.Addons.Add(uniqueName, mod);
	}

	public bool GetIsHost() => QSBCore.IsHost;
	public bool GetIsInMultiplayer() => QSBCore.IsInMultiplayer;

	public uint GetLocalPlayerID() => QSBPlayerManager.LocalPlayerId;
	public string GetPlayerName(uint playerId) => QSBPlayerManager.GetPlayer(playerId).Name;
	public GameObject GetPlayerBody(uint playerId) => QSBPlayerManager.GetPlayer(playerId).Body;
	public GameObject GetPlayerCamera(uint playerId) => QSBPlayerManager.GetPlayer(playerId).CameraBody;

	public bool GetPlayerReady(uint playerId)
	{
		var player = QSBPlayerManager.GetPlayer(playerId);
		return player.IsReady && player.Body != null;
	}

	public bool GetPlayerDead(uint playerId)
	{
		var player = QSBPlayerManager.GetPlayer(playerId);
		return player.IsDead;
	}

	public uint[] GetPlayerIDs() => QSBPlayerManager.PlayerList.Select(x => x.PlayerId).ToArray();

	public UnityEvent<uint> OnPlayerJoin() => QSBAPIEvents.OnPlayerJoinEvent;
	public UnityEvent<uint> OnPlayerLeave() => QSBAPIEvents.OnPlayerLeaveEvent;

	public void SetCustomData<T>(uint playerId, string key, T data) => QSBPlayerManager.GetPlayer(playerId).SetCustomData(key, data);
	public T GetCustomData<T>(uint playerId, string key) => QSBPlayerManager.GetPlayer(playerId).GetCustomData<T>(key);

	public void SendMessage<T>(string messageType, T data, uint to = uint.MaxValue, bool receiveLocally = false)
		=> new AddonDataMessage(messageType.GetStableHashCode(), data, receiveLocally) { To = to }.Send();

	public void RegisterHandler<T>(string messageType, Action<uint, T> handler)
		=> AddonDataManager.RegisterHandler(messageType.GetStableHashCode(), handler);

	public UnityEvent<string, uint> OnChatMessage() => MultiplayerHUDManager.OnChatMessageEvent;

	public void SendChatMessage(string message, bool systemMessage, Color color)
	{
		var fromName = systemMessage
			? "QSB"
			: QSBPlayerManager.LocalPlayer.Name;

		new ChatMessage($"{fromName}: {message}", color).Send();
	}
}

internal static class QSBAPIEvents
{
	static QSBAPIEvents()
	{
		QSBPlayerManager.OnAddPlayer += player => OnPlayerJoinEvent.Invoke(player.PlayerId);
		QSBPlayerManager.OnRemovePlayer += player => OnPlayerLeaveEvent.Invoke(player.PlayerId);
	}

	internal class PlayerEvent : UnityEvent<uint> { }

	internal static readonly PlayerEvent OnPlayerJoinEvent = new();
	internal static readonly PlayerEvent OnPlayerLeaveEvent = new();
}
